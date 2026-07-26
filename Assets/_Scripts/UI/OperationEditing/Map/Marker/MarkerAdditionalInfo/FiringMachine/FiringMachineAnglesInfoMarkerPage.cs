#region

using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class FiringMachineAnglesInfoMarkerPage : MarkerPage
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _currentHorizontalAngleText;
    [SerializeField] private TextMeshProUGUI _minHorizontalAngleText;
    [SerializeField] private TextMeshProUGUI _maxHorizontalAngleText;
    [SerializeField] private TextMeshProUGUI _currentVerticalAngleText;
    [SerializeField] private TextMeshProUGUI _minVerticalAngleText;
    [SerializeField] private TextMeshProUGUI _maxVerticalAngleText;

    [SerializeField] private TextMeshProUGUI _currentExplosiveBlockDistanceText;

    private IFiringMachineDataProvider _firingMachineDataProvider;
    private StringFormatsSO _stringFormatsSO;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(StringFormatsSO stringFormatsSO)
    {
        _stringFormatsSO = stringFormatsSO;
    }

    public override void InitializePage(Transform followingObject)
    {
        _firingMachineDataProvider = followingObject.GetComponent<IFiringMachineDataProvider>();

        base.InitializePage(followingObject);
    }

    #endregion

    #region Visuals

    public override void UpdateVisuals()
    {
        Vector3 currentAngles = _firingMachineDataProvider.CurrentEulerAngles;
        Vector3 minAngles = _firingMachineDataProvider.MinEulerAngles;
        Vector3 maxAngles = _firingMachineDataProvider.MaxEulerAngles;

        currentAngles = GetNormalizedAngles(currentAngles);
        minAngles = GetNormalizedAngles(minAngles);
        maxAngles = GetNormalizedAngles(maxAngles);

        Debug.Log($"[FiringMachineAnglesInfoMarkerPage.UpdateVisuals Line 49] {currentAngles}" +
                  $" {minAngles} {maxAngles}");

        string currentFiringMachineHorizontalAnglesString = GetFormattedAnglesString(currentAngles.y);
        string currentFiringMachineVerticalAnglesString = GetFormattedAnglesString(currentAngles.x);

        _currentHorizontalAngleText.text = currentFiringMachineHorizontalAnglesString;
        _currentVerticalAngleText.text = currentFiringMachineVerticalAnglesString;

        string minFiringMachineHorizontalAnglesString = GetFormattedAnglesString(minAngles.y);
        string minFiringMachineVerticalAnglesString = GetFormattedAnglesString(minAngles.x);

        _minHorizontalAngleText.text = minFiringMachineHorizontalAnglesString;
        _minVerticalAngleText.text = minFiringMachineVerticalAnglesString;

        string maxFiringMachineHorizontalAnglesString = GetFormattedAnglesString(maxAngles.y);
        string maxFiringMachineVerticalAnglesString = GetFormattedAnglesString(maxAngles.x);

        _maxHorizontalAngleText.text = maxFiringMachineHorizontalAnglesString;
        _maxVerticalAngleText.text = maxFiringMachineVerticalAnglesString;

        string explosiveBlockDistanceString = string.Format(_stringFormatsSO.DistanceFormatString,
            _firingMachineDataProvider.ExplosiveBlockDistance);

        _currentExplosiveBlockDistanceText.text = explosiveBlockDistanceString;
    }

    #endregion

    #region Get

    private string GetFormattedAnglesString(float angle)
    {
        return (angle >= 0 ? "+" : "") + string.Format(_stringFormatsSO.AnglesFormatString, (int)angle);
    }

    private Vector3 GetNormalizedAngles(Vector3 originalAngles)
    {
        originalAngles.x -= 360 * Mathf.Floor((originalAngles.x + 180) / 360);
        originalAngles.y -= 360 * Mathf.Floor((originalAngles.y + 180) / 360);

        return originalAngles;
    }

    #endregion
}