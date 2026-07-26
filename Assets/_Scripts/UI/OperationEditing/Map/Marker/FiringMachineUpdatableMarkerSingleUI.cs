#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class FiringMachineUpdatableMarkerSingleUI : BaseUpdatableMarkerSingleUI
{
    #region Variables & References

    [SerializeField] private Image _firingMachineCoveredZoneImage;
    [SerializeField] private Image _currentFiringMachineCoveredZone;

    private IFiringMachineDataProvider _firingMachineDataProvider;
    private bool _isInitialized;

    private bool IsFiringMachineActive =>
        _firingMachineDataProvider.ReadonlyHealthComponent.IsDestroyed == false && _firingMachineDataProvider.CurrentPoVStatus;

    #endregion

    #region Initialize

    public override void Initialize()
    {
        _isInitialized = FollowingObjectTransform.TryGetComponent(out _firingMachineDataProvider);

        if (_isInitialized == false)
        {
            Destroy(gameObject);

            return;
        }

        Vector3 minFiringMachineAngles = _firingMachineDataProvider.MinEulerAngles;
        Vector3 maxFiringMachineAngles = _firingMachineDataProvider.MaxEulerAngles;

        float notCoveredAnglesAmplitude = maxFiringMachineAngles.y > minFiringMachineAngles.y
            ? 360 - Mathf.Abs(maxFiringMachineAngles.y - minFiringMachineAngles.y)
            : 360 - Mathf.Abs(minFiringMachineAngles.y - maxFiringMachineAngles.y);

        float fullCoveredAngle = 360f;
        _firingMachineCoveredZoneImage.fillAmount = (fullCoveredAngle - notCoveredAnglesAmplitude) / fullCoveredAngle;

        Vector3 baseFiringMachineAngle = _firingMachineDataProvider.CurrentEulerAngles;
        float coveredZoneCircleRotation = -baseFiringMachineAngle.y;
        Vector3 coveredZoneCircleEulerAngles = new(0f, 0f, coveredZoneCircleRotation);

        _firingMachineCoveredZoneImage.transform.eulerAngles = coveredZoneCircleEulerAngles;

        _firingMachineCoveredZoneImage.gameObject.SetActive(IsFiringMachineActive);
        _currentFiringMachineCoveredZone.gameObject.SetActive(IsFiringMachineActive);
    }

    #endregion

    #region Visuals

    protected override void UpdateVisuals()
    {
        if (_isInitialized == false)
            return;

        _firingMachineCoveredZoneImage.gameObject.SetActive(IsFiringMachineActive);

        if (IsFiringMachineActive == false)
            return;

        Vector3 currentFiringMachineAngle = _firingMachineDataProvider.CurrentEulerAngles;
        Vector3 coveredZoneCircleEulerAngles = new(0f, 0f, -currentFiringMachineAngle.y);
        _currentFiringMachineCoveredZone.transform.localEulerAngles = coveredZoneCircleEulerAngles;
    }

    #endregion

    public override void OnSceneReset()
    {
        _firingMachineCoveredZoneImage.gameObject.SetActive(false);
        _currentFiringMachineCoveredZone.gameObject.SetActive(false);
    }
}