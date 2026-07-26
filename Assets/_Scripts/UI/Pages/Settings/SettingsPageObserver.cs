#region

using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

#endregion

public class SettingsPageObserver : IInitializable, IDisposable, ISceneResettable
{
    #region Variables & References

    private readonly SettingsPageUI _settingsPageUI;
    private readonly OperatorsPinsPageUI _operatorsPinsPageUI;
    private readonly DateTimePageUI _dateTimePageUI;
    private readonly Dictionary<FiringMachinesPageType, FiringMachinesPageUI> _allFiringMachinesPages;
    private readonly AlarmsPageUI _alarmsPageUI;
    private readonly AmmoPageUI _ammoPageUI;
    private readonly NavigationPanelUI _navigationPanelUI;

    private readonly SelectedUIItemController _selectedUIItemController;
    private readonly UIManager _uiManager;

    #endregion

    #region Initialization

    public SettingsPageObserver(SettingsPageUI settingsPageUI, OperatorsPinsPageUI operatorsPinsPageUI,
        DateTimePageUI dateTimePageUI, List<FiringMachinesPageUI> allFiringMachinesPages, AlarmsPageUI alarmsPageUI,
        AmmoPageUI ammoPageUI,
        NavigationPanelUI navigationPanelUI, SelectedUIItemController selectedUIItemController, UIManager uiManager)
    {
        _settingsPageUI = settingsPageUI;
        _operatorsPinsPageUI = operatorsPinsPageUI;
        _dateTimePageUI = dateTimePageUI;
        _alarmsPageUI = alarmsPageUI;
        _ammoPageUI = ammoPageUI;
        _navigationPanelUI = navigationPanelUI;
        _selectedUIItemController = selectedUIItemController;
        _uiManager = uiManager;

        _allFiringMachinesPages = allFiringMachinesPages.ToDictionary(firingMachinesPage => firingMachinesPage.PageType);
    }

    public void Initialize()
    {
        ResetInterfacesStateToDefault();

        _settingsPageUI.OperatorPinsButtonPressed += SettingsPageUI_OnOperatorPinsButtonPressed;
        _operatorsPinsPageUI.HideRequested += OperatorsPinsPageUI_OnHideRequested;

        _settingsPageUI.DateTimeButtonPressed += SettingsPageUI_OnDateTimeButtonPressed;
        _dateTimePageUI.HideRequested += DateTimePageUI_OnHideRequested;

        _settingsPageUI.PreSettingsButtonPressed += SettingsPageUI_OnPreSettingsButtonPressed;

        _allFiringMachinesPages[FiringMachinesPageType.PreSettingsMode].HideRequested +=
            FiringMachinesPageUI_OnHideRequested;

        _settingsPageUI.AlarmsButtonPressed += SettingsPageUI_OnAlarmsButtonPressed;
        _alarmsPageUI.HideRequested += AlarmsPageUI_OnHideRequested;

        _settingsPageUI.AmmoButtonPressed += SettingsPageUI_OnAmmoButtonPressed;
        _ammoPageUI.HideRequested += AmmoPageUI_OnHideRequested;
    }

    private void SettingsPageUI_OnOperatorPinsButtonPressed()
    {
        _uiManager.ChangeCurrentInterface(_operatorsPinsPageUI);
    }

    private void OperatorsPinsPageUI_OnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_settingsPageUI);
    }

    private void SettingsPageUI_OnDateTimeButtonPressed()
    {
        _uiManager.ChangeCurrentInterface(_dateTimePageUI);
    }

    private void DateTimePageUI_OnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_settingsPageUI);
    }

    private void SettingsPageUI_OnPreSettingsButtonPressed()
    {
        _uiManager.ChangeCurrentInterface(_allFiringMachinesPages[FiringMachinesPageType.PreSettingsMode]);

        _selectedUIItemController.BlockInteraction();
        _navigationPanelUI.Hide();
    }

    private void FiringMachinesPageUI_OnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_settingsPageUI);

        _selectedUIItemController.UnlockInteraction();
        _navigationPanelUI.Show(NavigationPanelType.Base);
    }

    private void SettingsPageUI_OnAlarmsButtonPressed()
    {
        _uiManager.ChangeCurrentInterface(_alarmsPageUI);

        _navigationPanelUI.Hide();
    }

    private void AlarmsPageUI_OnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_settingsPageUI);

        _navigationPanelUI.Show(NavigationPanelType.Base);
    }

    private void SettingsPageUI_OnAmmoButtonPressed()
    {
        _uiManager.ChangeCurrentInterface(_ammoPageUI);
    }

    private void AmmoPageUI_OnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_settingsPageUI);
    }

    #endregion

    private void ResetInterfacesStateToDefault()
    {
        _settingsPageUI.Hide();
        _operatorsPinsPageUI.Hide();
        _dateTimePageUI.Hide();
        _alarmsPageUI.Hide();
        _ammoPageUI.Hide();

        _selectedUIItemController.UnlockInteraction();
    }

    public void OnSceneReset()
    {
        ResetInterfacesStateToDefault();
    }

    public void Dispose()
    {
        _settingsPageUI.OperatorPinsButtonPressed -= SettingsPageUI_OnOperatorPinsButtonPressed;
        _operatorsPinsPageUI.HideRequested -= OperatorsPinsPageUI_OnHideRequested;

        _settingsPageUI.DateTimeButtonPressed -= SettingsPageUI_OnDateTimeButtonPressed;
        _dateTimePageUI.HideRequested -= DateTimePageUI_OnHideRequested;

        _settingsPageUI.PreSettingsButtonPressed -= SettingsPageUI_OnPreSettingsButtonPressed;

        _allFiringMachinesPages[FiringMachinesPageType.PreSettingsMode].HideRequested -=
            FiringMachinesPageUI_OnHideRequested;

        _settingsPageUI.AlarmsButtonPressed -= SettingsPageUI_OnAlarmsButtonPressed;
        _alarmsPageUI.HideRequested -= AlarmsPageUI_OnHideRequested;

        _settingsPageUI.AmmoButtonPressed -= SettingsPageUI_OnAmmoButtonPressed;
        _ammoPageUI.HideRequested -= AmmoPageUI_OnHideRequested;
    }
}