#region

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

[RequireComponent(typeof(FiringMachineViewController))]
[RequireComponent(typeof(FiringMachineShootingController))]
[RequireComponent(typeof(FiringMachineHealthController))]
public class FiringMachineController : NetworkBehaviour, IInitializable, IFiringMachine, IFiringMachineDataProvider,
    ISceneResettable, IPovProvider, IDisposable
{
    #region Events

    public event Action<int> HealthChanged;

    public event Action<int> HealthDepleted;

    public event Action AmmoCountChanged;

    public event Action PovAnglesChanged;

    public event Action PoVFocusChanged;

    public event Action PoVZoomChanged;

    public event Action PovStatusChanged;

    public event Action ActiveChanged;

    public event Action<ShootingBlockType, ShootingType> ShotInitiated;

    public event Action<bool> FiringModeStateChanged;

    #endregion

    #region Variables & References

    private FiringMachineStatsSO _firingMachineStatsSO;

    private int _firingMachineIndex;
    private readonly NetworkVariable<bool> _isFiringMachineEnabled = new();

    private bool _isChangingToDifferentPreSetting;
    private PreSettingSingle _changingPreSetting;
    [SerializeField] private Transform _projectorLightingTransform;
    private float _firingMachineSwitchStateTime;

    private bool _isPreparingWarningShot;
    private bool _isWarningShotCompleted;
    private Vector3 _preWarningShotEulerAngles;

    private CancellationTokenSource _switchingStateCancellationToken = new();

    private readonly NetworkVariable<bool> _isDestroyed = new();

    private FiringMachineViewController _viewController;
    private FiringMachineShootingController _shootingController;
    private FiringMachineHealthController _healthController;
    private IFocusDataProvider _focusDataProvider;

    #endregion

    #region Properties

    public IPovProvider PovProvider => this;

    public IFiringMachineDataProvider DataProvider => this;

    public IHaveHealth HealthComponent => _healthController;

    public IReadonlyHealthComponent ReadonlyHealthComponent => _healthController;

    public bool CurrentPoVStatus => _isFiringMachineEnabled.Value;

    public bool IsActive { get; private set; }

    public int FiringMachineNumber => _firingMachineIndex + 1;

    public Vector3 CurrentEulerAngles => _viewController.CurrentEulerAngles;

    public Vector3 MinEulerAngles { get; private set; }

    public Vector3 MaxEulerAngles { get; private set; }

    public int ExplosiveBlockDistance => _viewController.CurrentExplosiveBlockAdditionalDistance;
    public int MinExplosiveBlockDistance => _viewController.MinExplosiveBlockDistance;
    public int MaxExplosiveBlockDistance => _viewController.MaxExplosiveBlockDistance;

    public int FocusLevel => _viewController.PoVFocusLevel;

    public int ZoomLevel => _viewController.PoVZoomLevel;

    public AmmoType SelectedAmmoType => _shootingController.SelectedAmmoType;

    public int CurrentAmmoCount => _shootingController.CurrentAmmoCount;

    public bool IsDestroyed => _healthController.IsDestroyed;

    public List<ShootingBlockType> AllShootingBlockTypes => _shootingController.GetAllShootingBlockTypes();

    public Vector3 CurrentPovEulerAngles => _viewController.CurrentEulerAngles;

    public Vector3 CurrentEulerAnglesWithoutBase => _viewController.CurrentEulerAnglesWithoutBase;

    public Vector3 CurrentPovCameraPosition => _viewController.BasePoVPosition;

    public int CurrentPovFocusLevel => _viewController.PoVFocusLevel;

    public int CurrentPovZoomLevel => _viewController.PoVZoomLevel;

    public float CurrentPovZoomValue => _viewController.CurrentPoVZoomValue;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(Transform firingMachineTransform, int firingMachineIndex,
        IFocusDataProvider focusDataProvider, FiringMachineViewController viewController,
        FiringMachineShootingController shootingController, FiringMachineHealthController healthController, FiringMachineStatsSO firingMachineStatsSO)
    {
        _firingMachineIndex = firingMachineIndex;
        _firingMachineStatsSO = firingMachineStatsSO;

        transform.position = firingMachineTransform.position;
        transform.rotation = firingMachineTransform.rotation;

        Debug.Log($"spawned firing machine {firingMachineIndex} with position {transform.position}");

        _focusDataProvider = focusDataProvider;
        _viewController = viewController;
        _shootingController = shootingController;
        _healthController = healthController;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _isFiringMachineEnabled.OnValueChanged += IsFiringMachineEnabled_OnValueChanged;

        if (IsServer)
            TurnOffProjector();
    }

    public void Initialize()
    {
        _healthController.HealthChanged += HealthController_OnHealthChanged;
        _healthController.HealthDepleted += HealthController_OnHealthDepleted;

        _viewController.PoVFocusChanged += ViewController_OnPoVFocusChanged;
        _viewController.PovAnglesChanged += ViewController_OnPovAnglesChanged;
        _viewController.PoVZoomChanged += ViewController_OnPoVZoomChanged;

        _shootingController.ShotInitiated += ShootingController_OnShotInitiated;
        _shootingController.FiringModeStateChanged += ShootingController_OnFiringModeStateChanged;
        _shootingController.AmmoCountsUpdated += ShootingController_OnAmmoCountsUpdated;

        _firingMachineSwitchStateTime = _firingMachineStatsSO.StateSwitchTime;

        MinEulerAngles =
            new Vector3(_viewController.MinVerticalAngle,
                _viewController.BasePoVEulerAngles.y - _viewController.AdditionalHorizontalAngle);

        MaxEulerAngles = new Vector3(_viewController.MaxVerticalAngle,
            _viewController.BasePoVEulerAngles.y + _viewController.AdditionalHorizontalAngle);
    }

    private void ViewController_OnPoVZoomChanged()
    {
        PoVZoomChanged?.Invoke();
    }

    private void ViewController_OnPovAnglesChanged()
    {
        PovAnglesChanged?.Invoke();
    }

    private void IsFiringMachineEnabled_OnValueChanged(bool previousValue, bool newValue)
    {
        if (previousValue == newValue)
            return;

        _viewController.ResetCurrentZoom();
    }

    private void ShootingController_OnAmmoCountsUpdated()
    {
        AmmoCountChanged?.Invoke();
    }

    private void ShootingController_OnFiringModeStateChanged(bool firingModeState)
    {
        FiringModeStateChanged?.Invoke(firingModeState);
    }

    private void ShootingController_OnShotInitiated(ShootingBlockType shootingBlockType, ShootingType shootingType)
    {
        ShotInitiated?.Invoke(shootingBlockType, shootingType);
    }

    private void ViewController_OnPoVFocusChanged()
    {
        PoVFocusChanged?.Invoke();
    }

    private void HealthController_OnHealthChanged(float newHealth, float damageTaken)
    {
        HealthChanged?.Invoke((int)newHealth);
    }

    private void HealthController_OnHealthDepleted()
    {
        _isFiringMachineEnabled.Value = false;
        PovStatusChanged?.Invoke();
        HealthDepleted?.Invoke(FiringMachineNumber);
        HealthChanged?.Invoke((int)_healthController.CurrentHealth);

        TurnOffProjector();
    }

    #endregion

    #region View

    public void StartRotation(Vector2 rotationDelta)
    {
        _viewController.StartRotation(rotationDelta);
    }

    public void StopRotation()
    {
        _viewController.StopRotation();
    }

    public void RotateFiringMachineTo(Vector2Int rotationDeltaSide)
    {
        Vector3 stepAngles = new(0f, 5f, 0f);

        if (rotationDeltaSide.x < 0)
            stepAngles *= -1f;

        Vector3 newRotationAngles = _viewController.CurrentEulerAngles + stepAngles;

        _viewController.StartRotationToPoint(newRotationAngles, true);
    }

    public void ChangeExplosiveBlockDistance(bool isNormalDistance, bool isPositive)
    {
        _viewController.ChangeExplosiveBlockDistance(isNormalDistance, isPositive);
    }

    public void ChangeZoomLevel(int deltaZoomLevel)
    {
        int newZoomLevel = _viewController.PoVZoomLevel + deltaZoomLevel;

        _viewController.ChangeZoomLevel(newZoomLevel);
    }

    public void ChangeFocusLevel(int deltaFocusLevel)
    {
        int newFocusLevel = _viewController.PoVFocusLevel + deltaFocusLevel;

        if (newFocusLevel > _focusDataProvider.MaxFocusLevel || newFocusLevel < 0)
            return;

        _viewController.ChangeFocusLevel(newFocusLevel);
    }

    #endregion

    #region Shooting

    public void ChangeFiringModeToggle()
    {
        _shootingController.FiringModeToggle();
    }

    public void Shoot(ShootingType shootingType)
    {
        _shootingController.Shoot(shootingType);
    }

    public void WarningShoot()
    {
        WarningShootServerRpc();
    }

    public void ChangeShootingBlockType(ShootingBlockType shootingBlockType)
    {
        _shootingController.SelectShootingBlock(shootingBlockType);
    }

    public void ChangeAmmoType(ShootingBlockType shootingBlockType, AmmoType ammoType)
    {
        _shootingController.ChangeAmmoType(shootingBlockType, ammoType);
    }

    public void ResetAmmoCount()
    {
        _shootingController.Reload();
    }

    [ServerRpc]
    private void WarningShootServerRpc()
    {
        if (_isFiringMachineEnabled.Value == false)
            return;

        if (_viewController.IsRotatingCurrentlyBlocked)
            return;

        _isWarningShotCompleted = false;
        _isPreparingWarningShot = true;
        _preWarningShotEulerAngles = _viewController.CurrentEulerAngles;

        Vector3 targetWarningShotRotation =
            _preWarningShotEulerAngles + new Vector3(_viewController.MinVerticalAngle, 0f, 0f);

        Debug.Log("Initiated warning shot");

        _viewController.RotationTargetReached += ViewController_OnRotationTargetReached;
        _viewController.StartRotationToPoint(targetWarningShotRotation, true);
    }

    private void ViewController_OnRotationTargetReached()
    {
        if (_isPreparingWarningShot == false)
            return;

        if (_isWarningShotCompleted == false)
        {
            _shootingController.Shoot(ShootingType.Single);
            _isWarningShotCompleted = true;

            _viewController.StartRotationToPoint(_preWarningShotEulerAngles, true);
        }
        else
        {
            _isPreparingWarningShot = false;
            _viewController.RotationTargetReached -= ViewController_OnRotationTargetReached;
        }
    }

    #endregion

    #region Projector

    public void TurnOnProjector()
    {
        if (IsServer == false)
            return;

        TurnOnProjectorClientRpc();
    }

    [ClientRpc]
    private void TurnOnProjectorClientRpc()
    {
        _projectorLightingTransform.gameObject.SetActive(true);
    }

    public void TurnOffProjector()
    {
        if (IsServer == false)
            return;

        TurnOffProjectorClientRpc();
    }

    [ClientRpc]
    private void TurnOffProjectorClientRpc()
    {
        _projectorLightingTransform.gameObject.SetActive(false);
    }

    #endregion

    #region Pre Settings

    public void ChangeToPreSetting(PreSettingSingle preSettingSingle)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        if (preSettingSingle == null)
            return;

        ChangeToPreSettingServerRpc(preSettingSingle);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeToPreSettingServerRpc(PreSettingSingle activatingPreSetting)
    {
        if (_isFiringMachineEnabled.Value == false)
        {
            _isChangingToDifferentPreSetting = true;
            _changingPreSetting = activatingPreSetting;

            return;
        }

        if (_viewController.IsRotatingCurrentlyBlocked)
            return;

        _viewController.StartRotationToPoint(activatingPreSetting.PreSettingEulerAngles, true);
        _viewController.ChangeZoomLevel(activatingPreSetting.PreSettingZoom);
    }

    #endregion

    #region State

    public void SelectActive()
    {
        IsActive = true;
        ActiveChanged?.Invoke();
    }

    public void DeselectActive()
    {
        IsActive = false;
        ActiveChanged?.Invoke();
    }

    public void TurnOn()
    {
        if (IsServer == false)
            return;

        _switchingStateCancellationToken.Cancel();
        _switchingStateCancellationToken = new();

        ChangeFiringMachineStateAsync(true, _firingMachineSwitchStateTime, _switchingStateCancellationToken.Token)
            .Forget();
    }

    public void TurnOff()
    {
        if (IsServer == false)
            return;

        _switchingStateCancellationToken.Cancel();
        _switchingStateCancellationToken = new();

        ChangeFiringMachineStateAsync(false, _firingMachineSwitchStateTime, _switchingStateCancellationToken.Token)
            .Forget();

        TurnOffProjector();
    }

    private async UniTaskVoid ChangeFiringMachineStateAsync(bool newPowerState, float stateSwitchingTime,
        CancellationToken cancellationToken)
    {
        if (stateSwitchingTime > 0f)
            await UniTask.WaitForSeconds(stateSwitchingTime, cancellationToken: cancellationToken);

        _viewController.ResetView();

        _isFiringMachineEnabled.Value = newPowerState;

        if (newPowerState && _isChangingToDifferentPreSetting)
        {
            _isChangingToDifferentPreSetting = true;
            ChangeToPreSetting(_changingPreSetting);
        }

        PovStatusChanged?.Invoke();
    }

    #endregion

    #region Get

    public int GetShootingBlockAmmoCount(ShootingBlockType shootingBlockType)
    {
        return _shootingController.GetShootingBlockAmmoCount(shootingBlockType);
    }

    public int GetShootingBlockMaxAmmoCount(ShootingBlockType shootingBlockType)
    {
        return _shootingController.GetShootingBlockMaxAmmoCount(shootingBlockType);
    }

    public AmmoType GetShootingBlockAmmoType(ShootingBlockType shootingBlockType)
    {
        return _shootingController.GetShootingBlockAmmoType(shootingBlockType);
    }

    public bool IsShootingBlockSelected(ShootingBlockType shootingBlockType)
    {
        return _shootingController.IsShootingBlockSelected(shootingBlockType);
    }

    #endregion

    public void OnSceneReset()
    {
        if (IsServer)
        {
            _isFiringMachineEnabled.Value = false;
            _isDestroyed.Value = false;
            TurnOffProjector();
        }

        _switchingStateCancellationToken.Cancel();
        _switchingStateCancellationToken = new();

        _isPreparingWarningShot = false;
        _isChangingToDifferentPreSetting = false;

        PovStatusChanged?.Invoke();
    }

    public void Dispose()
    {
        _healthController.HealthChanged -= HealthController_OnHealthChanged;
        _healthController.HealthDepleted -= HealthController_OnHealthDepleted;

        _viewController.PoVFocusChanged -= ViewController_OnPoVFocusChanged;
        _viewController.PovAnglesChanged -= ViewController_OnPovAnglesChanged;
        _viewController.PoVZoomChanged -= ViewController_OnPoVZoomChanged;

        _shootingController.ShotInitiated -= ShootingController_OnShotInitiated;
        _shootingController.FiringModeStateChanged -= ShootingController_OnFiringModeStateChanged;
        _shootingController.AmmoCountsUpdated -= ShootingController_OnAmmoCountsUpdated;

        _switchingStateCancellationToken.Cancel();
    }
}