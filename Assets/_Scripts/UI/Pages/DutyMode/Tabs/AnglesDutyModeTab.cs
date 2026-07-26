#region

using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class AnglesDutyModeTab : DutyModeTab
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _currentDistanceText;
    [SerializeField] private int _maxDisplayingDistance = 400;

    [SerializeField] private TextMeshProUGUI _currentZoomText;
    [SerializeField] private TextMeshProUGUI _currentHorizontalAngleText;
    [SerializeField] private TextMeshProUGUI _currentVerticalAngleText;
    [SerializeField] private TextMeshProUGUI _currentExplosiveBlockAscensionAngleText;

    private ICameraStatsProvider _cameraStatsProvider;
    private StringFormatsSO _stringFormatsSO;

    public override DutyModeTabType DutyModeTabType => DutyModeTabType.Angles;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ICameraStatsProvider cameraStatsProvider, StringFormatsSO stringFormatsSO)
    {
        _cameraStatsProvider = cameraStatsProvider;
        _stringFormatsSO = stringFormatsSO;
    }

    public override void Initialize()
    {
    }

    #endregion

    #region Visuals

    public override void UpdateTabVisual(IFiringMachineDataProvider currentFiringMachineDataProvider)
    {
        bool isFiringMachineActive = currentFiringMachineDataProvider is { CurrentPoVStatus: true };

        UpdateZoom(isFiringMachineActive);

        UpdateAngle(_cameraStatsProvider.CameraHorizontalAngle, _currentHorizontalAngleText, isFiringMachineActive);
        UpdateAngle(_cameraStatsProvider.CameraVerticalAngle, _currentVerticalAngleText, isFiringMachineActive);

        int explosiveBlockDistance = currentFiringMachineDataProvider?.ExplosiveBlockDistance ?? -1;
        int maxExplosiveBlockDistance = currentFiringMachineDataProvider?.MaxExplosiveBlockDistance ?? -1;

        UpdateExplosiveBlockDistance(explosiveBlockDistance, maxExplosiveBlockDistance, isFiringMachineActive);

        UpdateCameraDistance(isFiringMachineActive);
    }

    private void UpdateCameraDistance(bool isFiringMachineActive)
    {
        float currentDistance = _cameraStatsProvider.TargetDistance;

        string currentDistanceString = isFiringMachineActive
            ? string.Format(_stringFormatsSO.DistanceFormatString,
                currentDistance < _maxDisplayingDistance ? (int)currentDistance : $">{_maxDisplayingDistance}")
            : "-";

        _currentDistanceText.text = currentDistanceString;
    }

    private void UpdateExplosiveBlockDistance(int explosiveBlockDistance, int maxExplosiveBlockDistance, bool isFiringMachineActive)
    {
        int currentExplosiveBlockAscensionAngle = explosiveBlockDistance / maxExplosiveBlockDistance;
        string currentExplosiveBlockAscensionAnglesString = FormatStringWithZeros(currentExplosiveBlockAscensionAngle);

        string currentHorizontalAnglesString =
            isFiringMachineActive ? $"+{string.Format(_stringFormatsSO.AnglesFormatString, currentExplosiveBlockAscensionAnglesString)}" : "-";

        _currentExplosiveBlockAscensionAngleText.text = currentHorizontalAnglesString;
    }

    private void UpdateAngle(float angle, TextMeshProUGUI textUI, bool isFiringMachineActive)
    {
        angle = (angle % 360 + 360) % 360;

        if (angle > 180)
            angle -= 360;

        string currentHorizontalAngleString = FormatStringWithZeros(angle);

        string currentHorizontalAnglesString =
            isFiringMachineActive ? string.Format(_stringFormatsSO.AnglesFormatString, currentHorizontalAngleString) : "-";

        textUI.text = angle >= 0 && isFiringMachineActive ? $"+{currentHorizontalAnglesString}" : currentHorizontalAnglesString;
    }

    private string FormatStringWithZeros(float angle)
    {
        string currentHorizontalAngleString = angle switch
        {
            < 10 and >= 0 => $"0{angle}",
            < 0 and > -10 => $"-0{Mathf.Abs(angle)}",
            var _ => $"{angle}"
        };

        return currentHorizontalAngleString;
    }

    private void UpdateZoom(bool isFiringMachineActive)
    {
        string currentZoomString = isFiringMachineActive ? (_cameraStatsProvider.CurrentZoomLevel + 1).ToString() : "-";

        _currentZoomText.text = currentZoomString;
    }

    #endregion

    public override void Dispose()
    {
    }
}