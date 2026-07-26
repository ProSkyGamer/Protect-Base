#region

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class FiringMachineMainInfoMarkerPage : MarkerPage
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _firingMachineNameText;

    [SerializeField] private Image _firingMachinePowerStatusIcon;
    [SerializeField] private Image _firingMachineProjectorStatusIcon;
    [SerializeField] private Image _firingMachineInfraredStatusIcon;
    [SerializeField] private Color _inactiveIconColor;
    [SerializeField] private Color _activeIconColor;
    [SerializeField] private Image _healthBarImage;
    [SerializeField] private TextMeshProUGUI _currentHealthText;

    private IFiringMachineDataProvider _followingFiringMachine;
    private IPoVSwapper _poVSwapper;
    private StringFormatsSO _stringFormatsSO;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(IPoVSwapper poVSwapper, StringFormatsSO stringFormatsSO)
    {
        _poVSwapper = poVSwapper;
        _stringFormatsSO = stringFormatsSO;
    }

    public override void InitializePage(Transform followingObject)
    {
        _followingFiringMachine = followingObject.GetComponent<IFiringMachineDataProvider>();

        base.InitializePage(followingObject);
    }

    #endregion

    #region Visuals

    public override void UpdateVisuals()
    {
        string currentFiringMachineNameString = string.Format(_stringFormatsSO.FiringMachineNameFormatString,
            _followingFiringMachine.FiringMachineNumber);

        _firingMachineNameText.text = currentFiringMachineNameString;

        _firingMachinePowerStatusIcon.color = _followingFiringMachine.CurrentPoVStatus
            ? _activeIconColor
            : _inactiveIconColor;

        _firingMachineProjectorStatusIcon.color = _poVSwapper.IsProjectorEnabled ? _activeIconColor : _inactiveIconColor;

        _firingMachineInfraredStatusIcon.color =
            _poVSwapper.IsInfraredEnabled ? _activeIconColor : _inactiveIconColor;

        float currentHeath = _followingFiringMachine.ReadonlyHealthComponent.CurrentHealth;
        float maxHealth = _followingFiringMachine.ReadonlyHealthComponent.MaxHealth;

        _healthBarImage.fillAmount = currentHeath / maxHealth;

        string currentFiringMachineHealthString = string.Format(_stringFormatsSO.CurrentHealthFormatString,
            (int)currentHeath, (int)maxHealth);

        _currentHealthText.text = currentFiringMachineHealthString;
    }

    #endregion
}