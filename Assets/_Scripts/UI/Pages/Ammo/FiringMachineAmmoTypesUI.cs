#region

using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class FiringMachineAmmoTypesUI : BaseSelectedItemSingleUI
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _firingMachineNumberText;
    [SerializeField] private TextMeshProUGUI _firingMachineBlockOneAmmoTypeText;
    [SerializeField] private TextMeshProUGUI _firingMachineBlockTwoAmmoTypeText;

    private EnumTranslationValuesSO _enumTranslationValuesSO;
    private IFiringMachineDataProvider _dataProvider;

    public int LinkedFiringMachineNumber => _dataProvider.FiringMachineNumber;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO,
        IFiringMachineDataProvider dataProvider)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;
        _dataProvider = dataProvider;

        Initialize();
    }

    public override void Initialize()
    {
        base.Initialize();

        _firingMachineNumberText.text = _dataProvider.FiringMachineNumber.ToString();

        Debug.Log(_dataProvider);
        Debug.Log(_enumTranslationValuesSO);
        Debug.Log(_dataProvider.GetShootingBlockAmmoType(ShootingBlockType.ExplosiveOne));

        _firingMachineBlockOneAmmoTypeText.text =
            _enumTranslationValuesSO.GetFiringMachineAmmoTypeString(
                _dataProvider.GetShootingBlockAmmoType(ShootingBlockType.ExplosiveOne));

        _firingMachineBlockTwoAmmoTypeText.text =
            _enumTranslationValuesSO.GetFiringMachineAmmoTypeString(
                _dataProvider.GetShootingBlockAmmoType(ShootingBlockType.ExplosiveTwo));
    }

    #endregion
}