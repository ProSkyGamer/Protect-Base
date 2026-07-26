#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

public class FiringMachineViewController : NetworkBehaviour, IInitializable, ISceneResettable, IShootingAnglesProvider, IDisposable
{
    #region Events

    public event Action PovAnglesChanged;

    public event Action PoVFocusChanged;

    public event Action PoVZoomChanged;

    public event Action RotationTargetReached;

    public event Action ExplBlockDistanceChanged;

    #endregion

    #region Variables & References

    [SerializeField] private Transform _firingMachineBaseCameraPositionTransform;
    [SerializeField] private Transform _firingMachineRotationTransform;

    private readonly NetworkVariable<float> _explBlockCurrentDistance = new();

    private FiringMachineStatsSO _firingMachineStatsSO;

    private Vector3 _firingMachineBaseCameraEulerAngles;
    private float _fullVerticalAngle;

    private readonly NetworkVariable<int> _currentFocusLevel = new();

    private readonly NetworkVariable<Vector3> _currentFiringMachineEulerAngles = new();
    private bool _isRotating;
    private bool _isRotationCurrentlyBlocked;
    private CancellationTokenSource _rotationCancellationToken = new();

    private readonly float _deltaFieldOfView = 5f;
    private readonly NetworkVariable<int> _currentZoomLevel = new();
    private float _currentFieldOfView;
    private bool _isZooming;
    private readonly CancellationTokenSource _zoomCancellationToken = new();

    #endregion

    #region Properties

    public bool IsRotatingCurrentlyBlocked => _isRotating && _isRotationCurrentlyBlocked;

    public Vector3 BasePoVPosition => _firingMachineBaseCameraPositionTransform.position;

    public Vector3 BasePoVEulerAngles => _firingMachineBaseCameraEulerAngles;

    public Vector3 CurrentEulerAngles => _currentFiringMachineEulerAngles.Value + _firingMachineBaseCameraEulerAngles;

    public Vector3 CurrentEulerAnglesWithoutBase => _currentFiringMachineEulerAngles.Value;

    public float AdditionalHorizontalAngle => _firingMachineStatsSO.HorizontalAdditionalAngle;

    public int PoVZoomLevel => _currentZoomLevel.Value;

    public float CurrentPoVZoomValue
    {
        get
        {
            if (ClientTypeManager.CurrentClientType is not ClientType.Game)
                return _firingMachineStatsSO.AllCameraZoomLevelFieldOfViews[_firingMachineStatsSO.BaseCameraZoomLevel];

            return _currentFieldOfView;
        }
    }

    public int CurrentExplosiveBlockAdditionalDistance => (int)_explBlockCurrentDistance.Value;
    public int MinExplosiveBlockDistance => (int)_firingMachineStatsSO.ExplBlockMinDistance;
    public int MaxExplosiveBlockDistance => (int)_firingMachineStatsSO.ExplBlockMaxDistance;

    public int PoVFocusLevel => _currentFocusLevel.Value;

    public float MinVerticalAngle { get; private set; }

    public float MaxVerticalAngle { get; private set; }

    public float TotalFlightDistance => _firingMachineStatsSO.MinExplosiveBlockAnglesAdditionalDistance + _explBlockCurrentDistance.Value;

    public bool IsCanFireExplosiveAmmo =>
        _currentFiringMachineEulerAngles.Value.x > 180f || _explBlockCurrentDistance.Value > 0f;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(FiringMachineStatsSO firingMachineStatsSO)
    {
        _firingMachineStatsSO = firingMachineStatsSO;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _currentFiringMachineEulerAngles.OnValueChanged += CurrentFiringMachineEulerAngles_OnValueChanged;
        _currentFocusLevel.OnValueChanged += CurrentFocusLevel_OnValueChanged;
        _currentZoomLevel.OnValueChanged += CurrentFiringMachineZoomLevel_OnValueChanged;
        _explBlockCurrentDistance.OnValueChanged += ExplBlockCurrentDistance_OnValueChanged;
    }

    public void Initialize()
    {
        if (_firingMachineBaseCameraPositionTransform == null)
            _firingMachineBaseCameraPositionTransform = transform;
        else
            _firingMachineBaseCameraPositionTransform.localEulerAngles = transform.localEulerAngles;

        _firingMachineBaseCameraEulerAngles = _firingMachineBaseCameraPositionTransform.localEulerAngles;

        MinVerticalAngle = _firingMachineStatsSO.VerticalNegativeAdditionalAngle;
        MaxVerticalAngle = _firingMachineStatsSO.VerticalPositiveAdditionalAngle;

        _fullVerticalAngle = (MinVerticalAngle, MaxVerticalAngle) switch
        {
            (< 0, > 0) => -MinVerticalAngle + MaxVerticalAngle,
            (> 0, > 0) => MinVerticalAngle > MaxVerticalAngle
                ? 360 - (MinVerticalAngle - MaxVerticalAngle)
                : MaxVerticalAngle - MinVerticalAngle,
            (< 0, < 0) => MinVerticalAngle > MaxVerticalAngle
                ? 360 - (MinVerticalAngle - MaxVerticalAngle)
                : MaxVerticalAngle - MinVerticalAngle,
            var _ => _fullVerticalAngle - MinVerticalAngle + MaxVerticalAngle
        };

        MinVerticalAngle += MinVerticalAngle switch
        {
            < 0 => 360f,
            var _ => -360f
        };

        MaxVerticalAngle += MaxVerticalAngle switch
        {
            < 0 => 360f,
            var _ => -360f
        };
    }

    private void CurrentFiringMachineEulerAngles_OnValueChanged(Vector3 previousValue, Vector3 newValue)
    {
        if (previousValue == newValue)
            return;

        PovAnglesChanged?.Invoke();
    }

    private void CurrentFocusLevel_OnValueChanged(int previousValue, int newValue)
    {
        if (previousValue == newValue)
            return;

        PoVFocusChanged?.Invoke();
    }

    private void CurrentFiringMachineZoomLevel_OnValueChanged(int previousValue, int newValue)
    {
        if (previousValue == newValue)
            return;

        _currentFieldOfView = _firingMachineStatsSO.AllCameraZoomLevelFieldOfViews[newValue];

        PoVZoomChanged?.Invoke();
    }

    private void ExplBlockCurrentDistance_OnValueChanged(float previousValue, float newValue)
    {
        if (Math.Abs(previousValue - newValue) < .1f)
            return;

        ExplBlockDistanceChanged?.Invoke();
    }

    #endregion

    #region Focus

    public void ChangeFocusLevel(int newFocusLevel)
    {
        ChangeFocusLevelServerRpc(newFocusLevel);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeFocusLevelServerRpc(int newFocusLevel)
    {
        _currentFocusLevel.Value = newFocusLevel;
    }

    #endregion

    #region Explosive Block Distance

    public void ChangeExplosiveBlockDistance(bool isNormalDistance, bool isPositive)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        float deltaX = isNormalDistance ? _firingMachineStatsSO.ExplBlockStepDistance : _firingMachineStatsSO.ExplBlockBigStepDistance;

        if (isPositive)
            deltaX = -deltaX;

        ChangeExplosiveBlockDistanceServerRpc(deltaX);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeExplosiveBlockDistanceServerRpc(float deltaX)
    {
        float newExplosiveBlockAdditionalDistance = _explBlockCurrentDistance.Value;

        newExplosiveBlockAdditionalDistance += deltaX;

        newExplosiveBlockAdditionalDistance =
            newExplosiveBlockAdditionalDistance > _firingMachineStatsSO.ExplBlockMaxDistance
                ? _firingMachineStatsSO.ExplBlockMaxDistance
                : newExplosiveBlockAdditionalDistance < _firingMachineStatsSO.ExplBlockMinDistance
                    ? _firingMachineStatsSO.ExplBlockMinDistance
                    : newExplosiveBlockAdditionalDistance;

        _explBlockCurrentDistance.Value = newExplosiveBlockAdditionalDistance;
    }

    #endregion

    #region Zoom

    public void ChangeZoomLevel(int newZoomLevel)
    {
        ChangeZoomLevelServerRpc(newZoomLevel);
    }

    public void ResetCurrentZoom()
    {
        SetZoomLevel(_firingMachineStatsSO.BaseCameraZoomLevel);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeZoomLevelServerRpc(int newZoomLevel)
    {
        if (_isZooming)
            return;

        newZoomLevel = Mathf.Clamp(newZoomLevel, 0, _firingMachineStatsSO.AllCameraZoomLevelFieldOfViews.Count - 1);

        if (newZoomLevel == _currentZoomLevel.Value)
            return;

        ChangeZoomLevelClientRpc(newZoomLevel);
    }

    [ClientRpc]
    private void ChangeZoomLevelClientRpc(int newZoomLevel)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        if (_isZooming)
            return;

        int endingZoomLevel = newZoomLevel;
        float targetFieldOfView = _firingMachineStatsSO.AllCameraZoomLevelFieldOfViews[newZoomLevel];

        float currentDeltaFieldOfView = 0f;

        if (targetFieldOfView - _currentFieldOfView > _deltaFieldOfView)
            currentDeltaFieldOfView = _deltaFieldOfView;
        else if (targetFieldOfView - _currentFieldOfView < -_deltaFieldOfView)
            currentDeltaFieldOfView = -_deltaFieldOfView;

        if (currentDeltaFieldOfView == 0f)
            return;

        ZoomToLevelAsync(currentDeltaFieldOfView, targetFieldOfView, endingZoomLevel, _zoomCancellationToken.Token)
            .Forget();
    }

    private async UniTaskVoid ZoomToLevelAsync(float fieldOfViewDelta, float targetFieldOfView, int endingZoomLevel,
        CancellationToken cancellationToken)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        _isZooming = true;

        Debug.Log(
            $"{_currentFieldOfView} {targetFieldOfView} {endingZoomLevel} {IsFieldOfViewTargetReached(_currentFieldOfView, targetFieldOfView)}" +
            $"{cancellationToken.IsCancellationRequested}");

        while (IsFieldOfViewTargetReached(_currentFieldOfView, targetFieldOfView) == false)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            float currentFieldOfViewDelta = fieldOfViewDelta * Time.deltaTime;

            _currentFieldOfView += currentFieldOfViewDelta;

            PoVZoomChanged?.Invoke();

            await UniTask.NextFrame(cancellationToken);
        }

        _isZooming = false;
        _currentZoomLevel.Value = endingZoomLevel;
    }

    private void SetZoomLevel(int newZoomLevel)
    {
        SetZoomLevelServerRpc(newZoomLevel);
    }

    [ServerRpc]
    private void SetZoomLevelServerRpc(int newZoomLevel)
    {
        _currentZoomLevel.Value = newZoomLevel;
    }

    #endregion

    #region Rotation

    public void StartRotation(Vector2 normalizedRotationVector)
    {
        if (IsRotatingCurrentlyBlocked)
            return;

        if (normalizedRotationVector.magnitude > 1)
            normalizedRotationVector = normalizedRotationVector.normalized;

        _rotationCancellationToken.Cancel();
        _rotationCancellationToken = new();
        RotateAsync(normalizedRotationVector, _rotationCancellationToken.Token).Forget();
    }

    public void StartRotationToPoint(Vector3 targetEulerAngles, bool isBlockingRotation)
    {
        if (IsRotatingCurrentlyBlocked)
            return;

        Vector3 currentRealAngle = _currentFiringMachineEulerAngles.Value;
        currentRealAngle = GetNormalizedRotationVector(currentRealAngle);

        targetEulerAngles.z = _firingMachineBaseCameraEulerAngles.z;
        targetEulerAngles = GetEulerAnglesWithinLimits(targetEulerAngles);
        targetEulerAngles = GetNormalizedRotationVector(targetEulerAngles);

        if (currentRealAngle == targetEulerAngles)
            return;

        Vector3 fullRotatingAngle = targetEulerAngles - currentRealAngle;
        fullRotatingAngle = GetNormalizedRotationVector(fullRotatingAngle);

        Vector3 rotationDelta =
            new(_firingMachineStatsSO.MaxVerticalRotationSpeedPerSecond * (fullRotatingAngle.x < 0 ? -1f : 1f),
                _firingMachineStatsSO.MaxHorizontalRotationSpeedPerSecond * (fullRotatingAngle.y < 0 ? -1f : 1f), 0f);

        float horizontalRotatingTime = Mathf.Abs(fullRotatingAngle.y / rotationDelta.y);
        float verticalRotatingTime = Mathf.Abs(fullRotatingAngle.x / rotationDelta.x);

        float totalRotationTime;

        if (horizontalRotatingTime > verticalRotatingTime || rotationDelta.x == 0f)
            totalRotationTime = horizontalRotatingTime;
        else
            totalRotationTime = verticalRotatingTime;

        rotationDelta = new Vector3(
            fullRotatingAngle.x / totalRotationTime,
            fullRotatingAngle.y / totalRotationTime);

        _isRotationCurrentlyBlocked = isBlockingRotation;

        _rotationCancellationToken.Cancel();
        _rotationCancellationToken = new();
        RotateToTargetAsync(rotationDelta, targetEulerAngles, _rotationCancellationToken.Token).Forget();
    }

    public void StopRotation()
    {
        if (IsRotatingCurrentlyBlocked)
            return;

        _isRotating = false;
        _rotationCancellationToken.Cancel();
        _rotationCancellationToken = new();
    }

    private async UniTaskVoid RotateAsync(Vector2 rotationDelta, CancellationToken cancellationToken)
    {
        if (IsServer == false)
            return;

        _isRotating = true;

        while (true)
        {
            float horizontalRotationDelta = rotationDelta.x * _firingMachineStatsSO.MaxHorizontalRotationSpeedPerSecond * Time.fixedDeltaTime;
            float verticalRotationDelta = -rotationDelta.y * _firingMachineStatsSO.MaxVerticalRotationSpeedPerSecond * Time.fixedDeltaTime;

            Vector3 cameraEulerAngles = _currentFiringMachineEulerAngles.Value;
            cameraEulerAngles += new Vector3(verticalRotationDelta, horizontalRotationDelta);

            Vector3 fixedCameraEulerAngles = GetEulerAnglesWithinLimits(cameraEulerAngles);

            _currentFiringMachineEulerAngles.Value = fixedCameraEulerAngles;

            await UniTask.WaitForFixedUpdate(cancellationToken);
        }
    }

    private async UniTaskVoid RotateToTargetAsync(Vector2 rotationDelta, Vector3 targetRotation,
        CancellationToken cancellationToken)
    {
        if (IsServer == false)
            return;

        _isRotating = true;

        while (IsRotationTargetReached(_currentFiringMachineEulerAngles.Value, targetRotation))
        {
            float horizontalRotationDelta = rotationDelta.x * Time.fixedDeltaTime * _firingMachineStatsSO.MaxHorizontalRotationSpeedPerSecond;
            float verticalRotationDelta = rotationDelta.y * Time.fixedDeltaTime * _firingMachineStatsSO.MaxVerticalRotationSpeedPerSecond;

            Vector3 cameraEulerAngles = _currentFiringMachineEulerAngles.Value;
            cameraEulerAngles += new Vector3(verticalRotationDelta, horizontalRotationDelta);

            Vector3 fixedCameraEulerAngles = GetEulerAnglesWithinLimits(cameraEulerAngles);

            _currentFiringMachineEulerAngles.Value = fixedCameraEulerAngles;

            await UniTask.WaitForFixedUpdate(cancellationToken);
        }

        _currentFiringMachineEulerAngles.Value = targetRotation;
        _isRotating = false;
        RotationTargetReached?.Invoke();
    }

    #endregion

    #region Reset

    public void ResetView()
    {
        _currentZoomLevel.Value = _firingMachineStatsSO.BaseCameraZoomLevel;
        _currentFieldOfView = _firingMachineStatsSO.AllCameraZoomLevelFieldOfViews[_currentZoomLevel.Value];

        PovAnglesChanged?.Invoke();
    }

    #endregion

    #region Get

    private bool IsRotationTargetReached(Vector3 currentRotation, Vector3 targetRotation)
    {
        return (currentRotation - targetRotation).magnitude < 1.5f;
    }

    private Vector2 GetNormalizedRotationVector(Vector2 eulerAngles)
    {
        eulerAngles.x = (eulerAngles.x % 360 + 360) % 360;
        eulerAngles.y = (eulerAngles.y % 360 + 360) % 360;

        return eulerAngles;
    }

    private bool IsFieldOfViewTargetReached(float startingFieldOfView, float targetFieldOfView)
    {
        return Mathf.Abs(targetFieldOfView - startingFieldOfView) < 1f;
    }

    private Vector3 GetEulerAnglesWithinLimits(Vector3 cameraEulerAngles)
    {
        cameraEulerAngles.x = ClampAngle(cameraEulerAngles.x, _firingMachineStatsSO.VerticalNegativeAdditionalAngle,
            _firingMachineStatsSO.VerticalPositiveAdditionalAngle);

        cameraEulerAngles.y = ClampAngle(cameraEulerAngles.y, -_firingMachineStatsSO.HorizontalAdditionalAngle,
            _firingMachineStatsSO.HorizontalAdditionalAngle);

        return cameraEulerAngles;
    }

    private float ClampAngle(float angle, float min, float max)
    {
        angle = (angle % 360 + 360) % 360;
        min = (min % 360 + 360) % 360;
        max = (max % 360 + 360) % 360;

        if (min > max) // Диапазон через 0 градусов
        {
            if (angle >= min || angle <= max)
                return angle;

            // Возвращаем ближайшую границу
            float clampAngle = Mathf.Abs(min - angle) < Mathf.Abs(max - angle) ? min : max;

            return clampAngle;
        }

        return Mathf.Clamp(angle, min, max);
    }

    public Vector3 GetRotatedPoint(Vector3 rotatingPoint)
    {
        return _firingMachineRotationTransform.TransformDirection(rotatingPoint);
    }

    #endregion

    public void OnSceneReset()
    {
        if (IsServer)
        {
            _currentFiringMachineEulerAngles.Value = Vector3.zero;
            _currentFocusLevel.Value = 0;
            _currentZoomLevel.Value = _firingMachineStatsSO.BaseCameraZoomLevel;
            _explBlockCurrentDistance.Value = _firingMachineStatsSO.ExplBlockMinDistance;
        }

        _currentFieldOfView = _firingMachineStatsSO.AllCameraZoomLevelFieldOfViews[_firingMachineStatsSO.BaseCameraZoomLevel];

        PovAnglesChanged?.Invoke();

        _rotationCancellationToken.Cancel();
        _zoomCancellationToken.Cancel();
    }

    public void Dispose()
    {
        _rotationCancellationToken.Cancel();
        _zoomCancellationToken.Cancel();
    }
}