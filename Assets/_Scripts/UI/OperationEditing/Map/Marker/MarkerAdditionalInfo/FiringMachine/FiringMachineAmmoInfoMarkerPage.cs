#region

using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class FiringMachineAmmoInfoMarkerPage : MarkerPage
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _ammoCountText;
    [SerializeField] private TextMeshProUGUI _explosiveAmmoFirstBlockCountText;
    [SerializeField] private TextMeshProUGUI _explosiveFirstBlockAmmoTypeText;
    [SerializeField] private TextMeshProUGUI _explosiveAmmoSecondBlockCountText;
    [SerializeField] private TextMeshProUGUI _explosiveSecondBlockAmmoTypeText;

    private IFiringMachineDataProvider _followingFiringMachine;
    private EnumTranslationValuesSO _enumTranslationValuesSO;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;
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
        int currentAmmoCount = _followingFiringMachine.GetShootingBlockAmmoCount(ShootingBlockType.Main);
        _ammoCountText.text = currentAmmoCount.ToString();

        int explosiveAmmoCountFirstBlock =
            _followingFiringMachine.GetShootingBlockAmmoCount(ShootingBlockType.ExplosiveOne);

        _explosiveAmmoFirstBlockCountText.text = explosiveAmmoCountFirstBlock.ToString();

        int explosiveAmmoCountSecondBlock =
            _followingFiringMachine.GetShootingBlockAmmoCount(ShootingBlockType.ExplosiveTwo);

        _explosiveAmmoSecondBlockCountText.text = explosiveAmmoCountSecondBlock.ToString();

        AmmoType explosiveFirstBlockAmmoType =
            _followingFiringMachine.GetShootingBlockAmmoType(ShootingBlockType.ExplosiveOne);

        _explosiveFirstBlockAmmoTypeText.text =
            _enumTranslationValuesSO.GetFiringMachineAmmoTypeString(explosiveFirstBlockAmmoType);

        AmmoType explosiveSecondBlockAmmoType =
            _followingFiringMachine.GetShootingBlockAmmoType(ShootingBlockType.ExplosiveTwo);

        _explosiveSecondBlockAmmoTypeText.text =
            _enumTranslationValuesSO.GetFiringMachineAmmoTypeString(explosiveSecondBlockAmmoType);
    }

    #endregion
}