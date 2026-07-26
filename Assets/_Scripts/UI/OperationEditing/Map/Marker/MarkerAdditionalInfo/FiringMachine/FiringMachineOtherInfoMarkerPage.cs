#region

using TMPro;
using UnityEngine;

#endregion

public class FiringMachineOtherInfoMarkerPage : MarkerPage
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _focusLevelText;
    [SerializeField] private TextMeshProUGUI _zoomLevelText;

    private IFiringMachineDataProvider _firingMachineDataProvider;

    #endregion

    #region Initialization

    public override void InitializePage(Transform followingObject)
    {
        _firingMachineDataProvider = followingObject.GetComponent<IFiringMachineDataProvider>();

        base.InitializePage(followingObject);
    }

    #endregion

    #region Visuals

    public override void UpdateVisuals()
    {
        _focusLevelText.text = _firingMachineDataProvider.FocusLevel.ToString();
        _zoomLevelText.text = _firingMachineDataProvider.ZoomLevel.ToString();
    }

    #endregion
}