#region

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

public class AllFiringMachinesManager : NetworkBehaviour, IAllFiringMachineInfoProvider,
    ICurrentFiringMachineDataProvider,
    IDutyInterfaceListener, IDevInterfaceListener, IPoVSwapper,
    ISceneResettable, IDisposable
{
    #region Events

    public event Action<IPovProvider> ChangePoV;

    public event Action<IFiringMachineDataProvider> ChangedFiringMachine;

    public event Action<bool> ChangeInfraredState;

    public event Action<bool> ChangeProjectorState;

    #endregion

    #region Created Classes

    [Serializable]
    public class ShootingBlockDefaultAmmoType
    {
        public ShootingBlockType ShootingBlockType;
        public AmmoType AmmoType;
    }

    #endregion

    #region Variables & References

    [SerializeField] private int _defaultFiringMachinePreSettingIndex = 1;
    [SerializeField] private List<ShootingBlockDefaultAmmoType> _allShootingBlockDefaultAmmoTypes;

    private readonly Dictionary<ShootingBlockType, AmmoType> _shootingBlockDefaultAmmoTypesDictionary = new();
    private readonly List<IFiringMachine> _allFiringMachinesSingle = new();
    private bool _isFiringMachinesInitialized;

    private readonly NetworkVariable<bool> _isInfraredEnabled = new();
    private readonly NetworkVariable<bool> _isProjectorEnabled = new();
    private bool _wasProjectorEnabled;

    private IFiringMachine _currentFiringMachine;
    private IPreSettingsProvider _preSettingsProvider;
    private IAlarmsTriggerer _alarmsTriggerer;
    private IPreSettingTriggerer _preSettingTriggerer;

    private bool _isInterfaceActive;
    private bool _isDevInterfaceShowing;
    private ShootingType _currentShootingType;
    private bool _isDutyModeActive;

    private const string IsFiringMachineAmmoTypeInitialized = "IsFiringMachineAmmoTypeInitialed";

    private FiringMachinesSpawner _firingMachinesSpawner;
    private IDataSavingManager _dataSavingManager;

    #endregion

    #region Properties

    private bool IsInterfaceActive => _isInterfaceActive && _isDevInterfaceShowing == false;

    public bool IsInfraredEnabled => _isInfraredEnabled.Value;

    public bool IsProjectorEnabled => _isProjectorEnabled.Value;

    public int CurrentActive => _currentFiringMachine.FiringMachineNumber;

    public bool IsAnySelected => _currentFiringMachine != null;

    public bool IsSelectedActive => _currentFiringMachine != null && _currentFiringMachine.PovProvider.CurrentPoVStatus;

    public Vector3 CurrentEulerAngles =>
        _currentFiringMachine != null
            ? _currentFiringMachine.PovProvider.CurrentPovEulerAngles
            : Vector3.zero;

    public int CurrentZoomLevel =>
        _currentFiringMachine != null
            ? _currentFiringMachine.PovProvider.CurrentPovZoomLevel
            : 0;

    public int TotalCount => _allFiringMachinesSingle.Count;

    public bool IsAnyEnabled
    {
        get { return _allFiringMachinesSingle.Any(firingMachineSingle => firingMachineSingle.PovProvider.CurrentPoVStatus); }
    }

    #endregion

    #region Global Events

    public void DevInterfaceActivated()
    {
        _isDevInterfaceShowing = true;
    }

    public void DevInterfaceDeactivated()
    {
        _isDevInterfaceShowing = false;
    }

    public void DutyInterfaceActivated(FiringMachinesPageType pageType)
    {
        _isInterfaceActive = true;
    }

    public void DutyInterfaceDeactivated()
    {
        _isInterfaceActive = false;
    }

    #endregion

    #region Initialization

    [Inject]
    public void Construct(IPreSettingsProvider preSettingsProvider,
        IAlarmsTriggerer alarmsTriggerer, IPreSettingTriggerer preSettingTriggerer,
        FiringMachinesSpawner firingMachinesSpawner, IDataSavingManager dataSavingManager)
    {
        _preSettingsProvider = preSettingsProvider;
        _alarmsTriggerer = alarmsTriggerer;
        _preSettingTriggerer = preSettingTriggerer;
        _dataSavingManager = dataSavingManager;
        _firingMachinesSpawner = firingMachinesSpawner;
        _firingMachinesSpawner.FiringMachinesSpawned += FiringMachinesSpawner_OnFiringMachinesSpawned;
    }

    private void FiringMachinesSpawner_OnFiringMachinesSpawned(List<FiringMachineController> spawnedFiringMachines)
    {
        _allFiringMachinesSingle.AddRange(spawnedFiringMachines);

        foreach (IFiringMachine firingMachine in _allFiringMachinesSingle)
        {
            firingMachine.DataProvider.HealthDepleted += FiringMachine_OnHealthDepleted;
        }

        foreach (ShootingBlockDefaultAmmoType shootingBlockDefaultAmmoType in _allShootingBlockDefaultAmmoTypes)
        {
            _shootingBlockDefaultAmmoTypesDictionary.TryAdd(shootingBlockDefaultAmmoType.ShootingBlockType,
                shootingBlockDefaultAmmoType.AmmoType);
        }

        bool isAmmoTypeInitialized = false;

        if (PlayerPrefs.GetInt(IsFiringMachineAmmoTypeInitialized, 0) == 0)
            PlayerPrefs.SetInt(IsFiringMachineAmmoTypeInitialized, 1);
        else
            isAmmoTypeInitialized = true;

        if (isAmmoTypeInitialized == false)
        {
            List<FiringMachineAmmoTypes> allDefaultFiringMachineAmmoTypes = _allFiringMachinesSingle.Select(firingMachine =>
                    new FiringMachineAmmoTypes(
                        firingMachine.AllShootingBlockTypes.ToDictionary(shootingBlockType => shootingBlockType,
                            shootingBlockType => _shootingBlockDefaultAmmoTypesDictionary[shootingBlockType]), firingMachine.FiringMachineNumber))
                .ToList();

            _dataSavingManager.SaveFiringMachineAmmoTypes(allDefaultFiringMachineAmmoTypes);
        }

        List<FiringMachineAmmoTypes> savedAmmoTypes = _dataSavingManager.GetAllSavedFiringMachineAmmoTypes();

        foreach (IFiringMachine firingMachine in _allFiringMachinesSingle)
        foreach (ShootingBlockType shootingBlockType in firingMachine.AllShootingBlockTypes)
        {
            AmmoType ammoType = AmmoType.No;

            savedAmmoTypes.Find(firingMachineAmmoType => firingMachineAmmoType.FiringMachineNumber == firingMachine.FiringMachineNumber)
                ?.AllShootingBlocksAmmoTypes.TryGetValue(shootingBlockType, out ammoType);

            ChangeAmmoType(firingMachine.FiringMachineNumber, shootingBlockType, ammoType);
        }

        _isFiringMachinesInitialized = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _isInfraredEnabled.OnValueChanged += IsInfraredEnabled_OnValueChanged;
        _isProjectorEnabled.OnValueChanged += IsProjectorEnabled_OnValueChanged;

        if (IsServer == false)
            return;

        _alarmsTriggerer.NewAlarmsTriggered += AlarmsTriggerer_OnNewAlarmsTriggered;
        _preSettingTriggerer.PreSettingTriggered += PreSettingTriggerer_OnTriggerPreSetting;
    }

    private void PreSettingTriggerer_OnTriggerPreSetting(int preSettingNumber)
    {
        if (_currentFiringMachine == null)
            return;

        PreSettingSingle changingPreSetting =
            _preSettingsProvider.GetPreSettingSingle(_currentFiringMachine.FiringMachineNumber, preSettingNumber);

        if (changingPreSetting == null)
            return;

        _currentFiringMachine.ChangeToPreSetting(changingPreSetting);
    }

    private void AlarmsTriggerer_OnNewAlarmsTriggered(IReadOnlyList<AlarmSingle> newlyTriggeredAlarms)
    {
        if (IsServer == false)
            return;

        foreach (AlarmSingle alarmSingle in newlyTriggeredAlarms)
        {
            if (alarmSingle.FiringMachineEnableType is not FiringMachineEnableType.Auto) continue;

            IFiringMachine enablingFiringMachine = _allFiringMachinesSingle.Find(firingMachine =>
                firingMachine.FiringMachineNumber == alarmSingle.FiringMachineNumber);

            if (!enablingFiringMachine.PovProvider.CurrentPoVStatus)
                enablingFiringMachine.TurnOn();

            PreSettingSingle changingPreSetting = _preSettingsProvider.GetPreSettingSingle(
                enablingFiringMachine.FiringMachineNumber,
                alarmSingle.PreSettingNumber);

            if (changingPreSetting == null)
                return;

            enablingFiringMachine.ChangeToPreSetting(changingPreSetting);
        }
    }

    private void IsProjectorEnabled_OnValueChanged(bool previousValue, bool newValue)
    {
        if (IsServer)
            return;

        if (previousValue == newValue)
            return;

        ChangeProjectorState?.Invoke(newValue);
    }

    private void IsInfraredEnabled_OnValueChanged(bool previousValue, bool newValue)
    {
        if (IsServer)
            return;

        if (previousValue == newValue)
            return;

        ChangeInfraredState?.Invoke(newValue);
    }

    private void FiringMachine_OnHealthDepleted(int firingMachineNumber)
    {
        if (IsServer == false)
            return;

        IFiringMachine destroyedFiringMachine =
            _allFiringMachinesSingle.Find(firingMachine => firingMachine.FiringMachineNumber == firingMachineNumber);

        if (destroyedFiringMachine == null)
            return;

        if (_currentFiringMachine != destroyedFiringMachine)
            return;

        if (_currentFiringMachine != null)
            SwitchCurrentFiringMachine(_currentFiringMachine.FiringMachineNumber, true, true);
    }

    #endregion

    #region Power

    public void PowerToggle()
    {
        if (IsInterfaceActive == false)
            return;

        if (_currentFiringMachine == null)
            return;

        FiringMachineToggleStateServerRpc(_currentFiringMachine.FiringMachineNumber);
    }

    public void PowerToggle(int firingMachineNumber)
    {
        if (IsInterfaceActive == false)
            return;

        FiringMachineToggleStateServerRpc(firingMachineNumber);
    }

    [ServerRpc(RequireOwnership = false)]
    private void FiringMachineToggleStateServerRpc(int firingMachineNumber)
    {
        IFiringMachine changingFiringMachine =
            _allFiringMachinesSingle.Find(firingMachine => firingMachine.FiringMachineNumber == firingMachineNumber);

        if (changingFiringMachine.HealthComponent.IsDestroyed)
            return;

        if (changingFiringMachine.PovProvider.CurrentPoVStatus)
        {
            changingFiringMachine.TurnOff();
        }
        else
        {
            changingFiringMachine.TurnOn();

            int switchingPreSettingIndex = _isInfraredEnabled.Value ? 82 :
                _isProjectorEnabled.Value ? 81 : _defaultFiringMachinePreSettingIndex;

            PreSettingSingle switchingPreSetting =
                _preSettingsProvider.GetPreSettingSingle(changingFiringMachine.FiringMachineNumber,
                    switchingPreSettingIndex);

            if (switchingPreSetting == null)
                return;

            changingFiringMachine.ChangeToPreSetting(switchingPreSetting);
        }
    }

    #endregion

    #region Rotation

    public void StartCurrentFiringMachineRotation(Vector2 normalizedRotationDelta)
    {
        if (IsInterfaceActive == false)
            return;

        _currentFiringMachine?.StartRotation(normalizedRotationDelta);
    }

    public void StopCurrentFiringMachineRotation()
    {
        if (IsInterfaceActive == false)
            return;

        _currentFiringMachine?.StopRotation();
    }

    public void ChangeCurrentFiringBlock(ShootingBlockType selectingShootingBlockType)
    {
        if (IsInterfaceActive == false)
            return;

        _currentFiringMachine?.ChangeShootingBlockType(selectingShootingBlockType);
    }

    public void RotateFiringMachine(bool isPositive)
    {
        if (IsInterfaceActive == false)
            return;

        _currentFiringMachine?.RotateFiringMachineTo(isPositive ? Vector2Int.right : Vector2Int.left);
    }

    public void ChangeExplosiveBlockDistance(bool isNormalDistance, bool isPositive)
    {
        if (IsInterfaceActive == false)
            return;

        _currentFiringMachine?.ChangeExplosiveBlockDistance(isNormalDistance, isPositive);
    }

    #endregion

    #region Ammo & Shooting

    public void DutyModeToggle()
    {
        if (IsInterfaceActive == false)
            return;

        Debug.Log(_isDutyModeActive ? "First firing stage ENABLED" : "First firing stage DISABLED");

        _isDutyModeActive = !_isDutyModeActive;
    }

    public void FiringModeToggle()
    {
        if (_isDutyModeActive == false)
            return;

        _currentFiringMachine?.ChangeFiringModeToggle();
    }

    public void ShootCurrentFiringMachine()
    {
        if (IsInterfaceActive == false)
            return;

        if (_isDutyModeActive == false)
            return;

        _currentFiringMachine?.Shoot(_currentShootingType);
    }

    public void WarningShootCurrentFiringMachine()
    {
        if (IsInterfaceActive == false)
            return;

        if (_isDutyModeActive == false)
            return;

        _currentFiringMachine?.WarningShoot();
    }

    public void ChangeShootingType(ShootingType shootingType)
    {
        if (IsInterfaceActive == false)
            return;

        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        Debug.Log($"Firing mode changed to {shootingType}");

        _currentShootingType = shootingType;
    }

    public void ReloadCurrentFiringMachine()
    {
        if (IsInterfaceActive == false)
            return;

        _currentFiringMachine?.ResetAmmoCount();
    }

    public void ChangeAmmoType(int firingMachineNumber, ShootingBlockType changingBlockType, AmmoType ammoType)
    {
        if (IsServer == false)
            return;

        IFiringMachine changingFiringMachine =
            _allFiringMachinesSingle.Find(firingMachine => firingMachine.FiringMachineNumber == firingMachineNumber);

        changingFiringMachine.ChangeAmmoType(changingBlockType, ammoType);

        List<FiringMachineAmmoTypes> allFiringMachineAmmoTypes = _allFiringMachinesSingle.Select(firingMachine =>
                new FiringMachineAmmoTypes(
                    firingMachine.AllShootingBlockTypes.ToDictionary(shootingBlockType => shootingBlockType,
                        shootingBlockType => firingMachine.DataProvider.GetShootingBlockAmmoType(shootingBlockType)),
                    firingMachine.FiringMachineNumber))
            .ToList();

        _dataSavingManager.SaveFiringMachineAmmoTypes(allFiringMachineAmmoTypes);
    }

    public void ResetAllFiringMachinesAmmoType()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        foreach (IFiringMachine firingMachine in _allFiringMachinesSingle)
        foreach (ShootingBlockType shootingBlockType in firingMachine.AllShootingBlockTypes)
        {
            ChangeAmmoType(firingMachine.FiringMachineNumber, shootingBlockType,
                _shootingBlockDefaultAmmoTypesDictionary.ContainsKey(shootingBlockType)
                    ? _shootingBlockDefaultAmmoTypesDictionary[shootingBlockType]
                    : AmmoType.No);
        }
    }

    #endregion

    #region Focus & Zoom

    public void ChangeFocusLevel(int deltaFocusLevel)
    {
        if (IsInterfaceActive == false)
            return;

        _currentFiringMachine?.ChangeFocusLevel(deltaFocusLevel);
    }

    public void ChangeZoomLevel(int deltaZoomLevel)
    {
        if (IsInterfaceActive == false)
            return;

        _currentFiringMachine?.ChangeZoomLevel(deltaZoomLevel);
    }

    #endregion

    #region Switch Active

    public void SwitchCurrentFiringMachine(int newFiringMachineNumber, bool isLookingForCloseOnes,
        bool isIncreasing = false)
    {
        if (IsInterfaceActive == false)
            return;

        ChangeFiringMachineServerRpc(newFiringMachineNumber, isLookingForCloseOnes, isIncreasing);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeFiringMachineServerRpc(int newFiringMachineNumber, bool isLookingForCloseOnes,
        bool isIncreasing = false)
    {
        IFiringMachine changingFiringMachine =
            _allFiringMachinesSingle.Find(firingMachine => firingMachine.FiringMachineNumber == newFiringMachineNumber);

        if (isLookingForCloseOnes == false &&
            (changingFiringMachine == null || changingFiringMachine.HealthComponent.IsDestroyed)) return;

        if ((changingFiringMachine == null || changingFiringMachine.HealthComponent.IsDestroyed) &&
            TryFindClosestFiringMachine(newFiringMachineNumber, isIncreasing, out changingFiringMachine) == false)
            return;

        newFiringMachineNumber = changingFiringMachine.FiringMachineNumber;

        ChangeFiringMachineClientRpc(newFiringMachineNumber);
    }

    [ClientRpc]
    private void ChangeFiringMachineClientRpc(int newFiringMachineNumber)
    {
        IFiringMachine changingFiringMachine =
            _allFiringMachinesSingle.Find(firingMachine => firingMachine.FiringMachineNumber == newFiringMachineNumber);

        if (changingFiringMachine == null)
            return;

        _currentFiringMachine?.DeselectActive();

        _currentFiringMachine = changingFiringMachine;
        _currentFiringMachine.SelectActive();
        ChangePoV?.Invoke(_currentFiringMachine.PovProvider);
        ChangedFiringMachine?.Invoke(_currentFiringMachine.DataProvider);

        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        _alarmsTriggerer.RemoveTriggeredAlarm(newFiringMachineNumber);
    }

    public void SwitchSelectedToNext()
    {
        if (IsInterfaceActive == false)
            return;

        int nextFiringMachineNumber = _currentFiringMachine?.FiringMachineNumber + 1 ?? 1;

        nextFiringMachineNumber =
            nextFiringMachineNumber > _allFiringMachinesSingle.Count ? 1 : nextFiringMachineNumber;

        SwitchCurrentFiringMachine(nextFiringMachineNumber, true, true);
    }

    public void SwitchSelectedToPrevious()
    {
        if (IsInterfaceActive == false)
            return;

        int previousFiringMachineNumber = _currentFiringMachine?.FiringMachineNumber - 1 ?? 1;

        previousFiringMachineNumber = previousFiringMachineNumber <= 0
            ? _allFiringMachinesSingle.Count
            : previousFiringMachineNumber;

        SwitchCurrentFiringMachine(previousFiringMachineNumber, true);
    }

    #endregion

    #region Projector

    public void ProjectorToggle()
    {
        if (IsInterfaceActive == false)
            return;

        ProjectorToggleServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ProjectorToggleServerRpc()
    {
        if (_currentFiringMachine == null)
            return;

        if (_isInfraredEnabled.Value)
            return;

        ChangeCurrentProjectorState(_isProjectorEnabled.Value == false);

        Debug.Log(_isProjectorEnabled.Value ? "Projector ENABLED" : "Projector DISABLED");

        if (_isProjectorEnabled.Value)
            foreach (IFiringMachine firingMachineSingle in _allFiringMachinesSingle)
            {
                if (firingMachineSingle.PovProvider.CurrentPoVStatus)
                {
                    PreSettingSingle changingPreSetting = _preSettingsProvider.GetPreSettingSingle(firingMachineSingle.FiringMachineNumber, 81);

                    if (changingPreSetting == null)
                        return;

                    firingMachineSingle.ChangeToPreSetting(
                        changingPreSetting);
                }
            }
    }

    private void ChangeCurrentProjectorState(bool newState)
    {
        _isProjectorEnabled.Value = newState;

        ChangeProjectorState?.Invoke(_isProjectorEnabled.Value);

        foreach (IFiringMachine firingMachine in _allFiringMachinesSingle)
        {
            if (newState)
                firingMachine.TurnOnProjector();
            else
                firingMachine.TurnOffProjector();
        }
    }

    #endregion

    #region Infrared

    public void InfraredToggle()
    {
        if (IsInterfaceActive == false)
            return;

        InfraredToggleServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void InfraredToggleServerRpc()
    {
        if (_currentFiringMachine == null)
            return;

        _isInfraredEnabled.Value = !_isInfraredEnabled.Value;

        Debug.Log(_isInfraredEnabled.Value ? "Infrared ENABLED" : "Infrared DISABLED");

        ChangeInfraredState?.Invoke(_isInfraredEnabled.Value);

        if (_isInfraredEnabled.Value && _isProjectorEnabled.Value)
        {
            _wasProjectorEnabled = _isProjectorEnabled.Value;

            ChangeCurrentProjectorState(false);
        }
        else if (_isInfraredEnabled.Value == false && _wasProjectorEnabled)
        {
            _wasProjectorEnabled = false;

            ChangeCurrentProjectorState(true);
        }

        if (_isInfraredEnabled.Value)
            if (_currentFiringMachine.PovProvider.CurrentPoVStatus)
            {
                PreSettingSingle changingPreSetting = _preSettingsProvider.GetPreSettingSingle(_currentFiringMachine.FiringMachineNumber, 82);

                if (changingPreSetting == null)
                    return;

                _currentFiringMachine.ChangeToPreSetting(
                    changingPreSetting);
            }
    }

    #endregion

    #region Get

    private bool TryFindClosestFiringMachine(int newFiringMachineNumber, bool isIncreasing,
        out IFiringMachine foundFiringMachine)
    {
        List<IFiringMachine> otherFiringMachines = isIncreasing
            ? _allFiringMachinesSingle.Where(firingMachine =>
                firingMachine.FiringMachineNumber > newFiringMachineNumber).ToList()
            : _allFiringMachinesSingle.Where(firingMachine =>
                firingMachine.FiringMachineNumber < newFiringMachineNumber).ToList();

        otherFiringMachines = otherFiringMachines.Where(firingMachine => firingMachine.HealthComponent.IsDestroyed == false).ToList();

        if (otherFiringMachines.Any())
        {
            foundFiringMachine = otherFiringMachines.OrderBy(firingMachine =>
                Mathf.Abs(firingMachine.FiringMachineNumber - newFiringMachineNumber)).First();

            return true;
        }

        foundFiringMachine = isIncreasing
            ? _allFiringMachinesSingle.OrderBy(firingMachine => firingMachine.FiringMachineNumber).First()
            : _allFiringMachinesSingle.OrderByDescending(firingMachine => firingMachine.FiringMachineNumber).First();

        return foundFiringMachine != null;
    }

    public async UniTask<List<IFiringMachineDataProvider>> GetAllDataProviders()
    {
        await UniTask.WaitUntil(() => _isFiringMachinesInitialized);

        return _allFiringMachinesSingle.Select(firingMachine => firingMachine.DataProvider).ToList();
    }

    public int GetFiringMachineMinNumber()
    {
        int firingMachineMinNumber = 1;

        return firingMachineMinNumber;
    }

    public async UniTask<int> GetFiringMachineMaxNumber()
    {
        await UniTask.WaitUntil(() => _isFiringMachinesInitialized);

        return _allFiringMachinesSingle.Count;
    }

    #endregion

    public void OnSceneReset()
    {
        if (IsServer)
        {
            foreach (IFiringMachine firingMachineSingle in _allFiringMachinesSingle)
            {
                firingMachineSingle.ResetAmmoCount();

                firingMachineSingle.TurnOff();
            }

            _isInfraredEnabled.Value = false;
            _isProjectorEnabled.Value = false;
        }

        if (ClientTypeManager.CurrentClientType is ClientType.Game)
        {
            _isDutyModeActive = false;
            _currentShootingType = ShootingType.Single;
        }

        _currentFiringMachine?.DeselectActive();
        _currentFiringMachine = null;

        ChangePoV?.Invoke(null);

        ChangeInfraredState?.Invoke(false);
        ChangeProjectorState?.Invoke(false);
    }

    public void Dispose()
    {
        _firingMachinesSpawner.FiringMachinesSpawned -= FiringMachinesSpawner_OnFiringMachinesSpawned;

        _alarmsTriggerer.NewAlarmsTriggered -= AlarmsTriggerer_OnNewAlarmsTriggered;
        _preSettingTriggerer.PreSettingTriggered -= PreSettingTriggerer_OnTriggerPreSetting;
    }
}