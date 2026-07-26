#region

using System;
using Zenject;

#endregion

public class AmmoPageObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly AmmoPageUI _ammoPageUI;
    private readonly AllFiringMachinesManager _allFiringMachinesManager;

    #endregion

    #region Initialization

    public AmmoPageObserver(AllFiringMachinesManager allFiringMachinesManager, AmmoPageUI ammoPageUI)
    {
        _allFiringMachinesManager = allFiringMachinesManager;
        _ammoPageUI = ammoPageUI;
    }

    public void Initialize()
    {
        _ammoPageUI.AmmoTypeChanged += AmmoPageUI_OnAmmoTypeChanged;
        _ammoPageUI.AllAmmoTypesReset += AmmoPageUI_OnAllAmmoTypesReset;

        _ammoPageUI.PageShown += AmmoPageUI_OnPageShown;
    }

    private void AmmoPageUI_OnPageShown()
    {
        _ammoPageUI.UpdateVisual().Forget();
    }

    private void AmmoPageUI_OnAmmoTypeChanged(int firingMachineNumber, ShootingBlockType shootingBlockType,
        AmmoType newAmmoType)
    {
        _allFiringMachinesManager.ChangeAmmoType(firingMachineNumber, shootingBlockType, newAmmoType);

        _ammoPageUI.UpdateVisual().Forget();
    }

    private void AmmoPageUI_OnAllAmmoTypesReset()
    {
        _allFiringMachinesManager.ResetAllFiringMachinesAmmoType();

        _ammoPageUI.UpdateVisual().Forget();
    }

    #endregion

    public void Dispose()
    {
        _ammoPageUI.AmmoTypeChanged -= AmmoPageUI_OnAmmoTypeChanged;
        _ammoPageUI.AllAmmoTypesReset -= AmmoPageUI_OnAllAmmoTypesReset;

        _ammoPageUI.PageShown -= AmmoPageUI_OnPageShown;
    }
}