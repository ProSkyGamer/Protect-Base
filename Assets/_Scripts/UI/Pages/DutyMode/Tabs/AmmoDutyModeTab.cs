#region

using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class AmmoDutyModeTab : DutyModeTab
{
    #region Variables & References

    [SerializeField] private TMP_FontAsset _chosenAmmoBlockTextFont;
    [SerializeField] private Color _chosenAmmoBlockTextColor;
    private TMP_FontAsset _unChosenAmmoBlockTextFont;
    private Color _unChosenAmmoBlockTextColor;
    [SerializeField] private TextMeshProUGUI _mainBlockAmmoText;
    [SerializeField] private TextMeshProUGUI _explosiveOneBlockAmmoText;
    [SerializeField] private TextMeshProUGUI _explosiveTwoBlockAmmoText;
    [SerializeField] private TextMeshProUGUI _ammoTypeText;

    private EnumTranslationValuesSO _enumTranslationValuesSO;

    public override DutyModeTabType DutyModeTabType => DutyModeTabType.Ammo;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;

        _unChosenAmmoBlockTextColor = _mainBlockAmmoText.color;
        _unChosenAmmoBlockTextFont = _mainBlockAmmoText.font;
    }

    public override void Initialize()
    {
    }

    #endregion

    #region Visuals

    public override void UpdateTabVisual(IFiringMachineDataProvider currentFiringMachineDataProvider)
    {
        bool isFiringMachineActive = currentFiringMachineDataProvider is { CurrentPoVStatus: true };

        UpdateAmmoCount(currentFiringMachineDataProvider, _mainBlockAmmoText, ShootingBlockType.Main, isFiringMachineActive);

        UpdateAmmoCount(currentFiringMachineDataProvider, _explosiveOneBlockAmmoText, ShootingBlockType.ExplosiveOne,
            isFiringMachineActive);

        UpdateAmmoCount(currentFiringMachineDataProvider, _explosiveTwoBlockAmmoText, ShootingBlockType.ExplosiveTwo,
            isFiringMachineActive);

        UpdateAmmoType(currentFiringMachineDataProvider, isFiringMachineActive);
    }

    private void UpdateAmmoType(IFiringMachineDataProvider currentFiringMachineDataProvider, bool isFiringMachineActive)
    {
        string currentAmmoTypeString = "---";

        if (isFiringMachineActive)
        {
            AmmoType currentAmmoType = currentFiringMachineDataProvider.SelectedAmmoType;

            currentAmmoTypeString = _enumTranslationValuesSO.GetFiringMachineAmmoTypeString(currentAmmoType);
        }

        _ammoTypeText.text = currentAmmoTypeString;
    }

    private void UpdateAmmoCount(IFiringMachineDataProvider currentFiringMachineDataProvider, TextMeshProUGUI ammoText, ShootingBlockType blockType,
        bool isFiringMachineActive)
    {
        int blockAmmoCount = currentFiringMachineDataProvider?.GetShootingBlockAmmoCount(blockType) ?? 0;
        bool isShootingBlockSelected = currentFiringMachineDataProvider?.IsShootingBlockSelected(blockType) ?? false;
        int ammoDigits = blockAmmoCount.ToString().Length;

        string ammoCountString = isFiringMachineActive ? FormatStringWithZeros(blockAmmoCount, ammoDigits) : new('-', ammoDigits);

        ammoText.text = ammoCountString;
        ammoText.font = isFiringMachineActive && isShootingBlockSelected ? _chosenAmmoBlockTextFont : _unChosenAmmoBlockTextFont;
        ammoText.color = isFiringMachineActive && isShootingBlockSelected ? _chosenAmmoBlockTextColor : _unChosenAmmoBlockTextColor;
    }

    private string FormatStringWithZeros(float angle, int ammoDigits)
    {
        string currentHorizontalAngleString = ((int)angle).ToString($"D{ammoDigits}");

        return currentHorizontalAngleString;
    }

    #endregion

    public override void Dispose()
    {
    }
}