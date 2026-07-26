#region

using System;
using UnityEngine;
using UnityEngine.Rendering;
using Zenject;

#endregion

public class CameraViewManager : MonoBehaviour, ICameraStatsProvider, IFocusDataProvider, IInitializable,
    ISceneResettable, IDisposable
{
    #region Events

    public event Action OnCameraAnglesChanged;

    #endregion

    #region Variables

    [SerializeField] private Camera _mainCamera;
    [SerializeField] private Camera _infraredCamera;
    private CameraStatsSO _cameraStatsSO;

    private Volume _postProcessVolume;
    private IPoVSwapper _poVSwapper;
    private IPovProvider _currentPoVProvider;

    #endregion

    #region Properties

    public int MaxFocusLevel => _cameraStatsSO.AllFocusProfiles.Count - 1;

    public int CameraHorizontalAngle => _currentPoVProvider == null ? 0 : (int)_currentPoVProvider.CurrentPovEulerAngles.y;

    public int CameraVerticalAngle => _currentPoVProvider == null ? 0 : (int)_currentPoVProvider.CurrentPovEulerAngles.x;

    public int CurrentZoomLevel => _currentPoVProvider?.CurrentPovZoomLevel ?? 0;

    public float TargetDistance
    {
        get
        {
            Ray ray = new(_mainCamera.transform.position, _mainCamera.transform.forward);

            Physics.Raycast(ray, out RaycastHit raycastHit, _cameraStatsSO.MaxTargetDistance);

            return raycastHit.distance;
        }
    }

    #endregion

    #region Initialization

    [Inject]
    public void Construct(IPoVSwapper poVSwapper, CameraStatsSO cameraStatsSO)
    {
        _poVSwapper = poVSwapper;
        _cameraStatsSO = cameraStatsSO;
    }

    public void Initialize()
    {
        _postProcessVolume = _mainCamera.GetComponent<Volume>();
        _postProcessVolume.enabled = false;

        _poVSwapper.ChangePoV += PoVSwapperOnChangePoV;
        _poVSwapper.ChangeInfraredState += PoVSwapperOnChangeInfraredState;
    }

    private void PoVSwapperOnChangePoV(IPovProvider newPovProvider)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        if (_currentPoVProvider != null)
        {
            _currentPoVProvider.PovAnglesChanged -= CurrentPoVProvider_OnPovAnglesChanged;
            _currentPoVProvider.PoVFocusChanged -= CurrentPoVProvider_OnPoVFocusChanged;
            _currentPoVProvider.PoVZoomChanged -= CurrentPoVProvider_OnPoVZoomChanged;
            _currentPoVProvider.PovStatusChanged -= CurrentPoVProvider_OnPovStatusChanged;
        }

        _currentPoVProvider = newPovProvider;

        if (_currentPoVProvider == null) return;

        _postProcessVolume.enabled = _currentPoVProvider.CurrentPoVStatus;

        _currentPoVProvider.PovAnglesChanged += CurrentPoVProvider_OnPovAnglesChanged;
        _currentPoVProvider.PoVFocusChanged += CurrentPoVProvider_OnPoVFocusChanged;
        _currentPoVProvider.PoVZoomChanged += CurrentPoVProvider_OnPoVZoomChanged;
        _currentPoVProvider.PovStatusChanged += CurrentPoVProvider_OnPovStatusChanged;

        UpdateCamera();
        UpdateCameraFocus();
    }

    private void CurrentPoVProvider_OnPovAnglesChanged()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        UpdateCamera();
    }

    private void CurrentPoVProvider_OnPoVFocusChanged()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        UpdateCameraFocus();
    }

    private void CurrentPoVProvider_OnPoVZoomChanged()
    {
        float currentFiringMachineZoomFieldOfView = _currentPoVProvider.CurrentPovZoomValue;
        _mainCamera.fieldOfView = currentFiringMachineZoomFieldOfView;
        _infraredCamera.fieldOfView = currentFiringMachineZoomFieldOfView;
    }

    private void CurrentPoVProvider_OnPovStatusChanged()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        _postProcessVolume.enabled = _currentPoVProvider.CurrentPoVStatus;

        UpdateCameraFocus();
    }

    private void PoVSwapperOnChangeInfraredState(bool newInfraredState)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        _infraredCamera.gameObject.SetActive(newInfraredState);
    }

    private void UpdateCameraFocus()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        if (_currentPoVProvider == null)
            return;

        int poVFocusLevel = _currentPoVProvider.CurrentPovFocusLevel;
        _postProcessVolume.profile = _cameraStatsSO.AllFocusProfiles[poVFocusLevel];
    }

    private void UpdateCamera()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game) return;
        if (_currentPoVProvider == null) return;

        Vector3 currentPovCameraPosition = _currentPoVProvider.CurrentPovCameraPosition;
        Vector3 currentPovAngles = _currentPoVProvider.CurrentPovEulerAngles;

        _mainCamera.transform.position = currentPovCameraPosition;
        _mainCamera.transform.eulerAngles = currentPovAngles;

        OnCameraAnglesChanged?.Invoke();
    }

    #endregion

    public void OnSceneReset()
    {
        _infraredCamera.gameObject.SetActive(false);
        _postProcessVolume.enabled = false;
        _currentPoVProvider = null;
    }

    public void Dispose()
    {
        OnCameraAnglesChanged = null;
    }
}