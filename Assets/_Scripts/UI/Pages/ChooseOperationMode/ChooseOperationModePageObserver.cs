#region

using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

#endregion

public class ChooseOperationModePageObserver : IInitializable, ISceneResettable, IDisposable
{
    #region Variables & References

    private readonly ChooseOperatingModePageUI _chooseOperatingModePageUI;
    private readonly MeteoConditionsPageUI _meteoConditionsManager;
    private readonly EventsListPageUI _eventsListPageUI;
    private readonly SettingsPageUI _settingsPageUI;
    private readonly ExitPageUI _exitPageUI;
    private readonly NavigationPanelUI _navigationPanelUI;
    private readonly Dictionary<FiringMachinesPageType, FiringMachinesPageUI> _allFiringMachinePages;

    private readonly CustomEventsManager _customEventsManager;
    private readonly SelectedUIItemController _selectedUIItemController;
    private readonly UIManager _uiManager;

    #endregion

    #region Initialization

    public ChooseOperationModePageObserver(ChooseOperatingModePageUI chooseOperatingModePageUI,
        MeteoConditionsPageUI meteoConditionsPageUI, List<FiringMachinesPageUI> allFiringMachinePages,
        EventsListPageUI eventsListPageUI,
        SettingsPageUI settingsPageUI, ExitPageUI exitPageUI, CustomEventsManager customEventsManager,
        SelectedUIItemController selectedUIItemController, NavigationPanelUI navigationPanelUI, UIManager uiManager)
    {
        _chooseOperatingModePageUI = chooseOperatingModePageUI;
        _meteoConditionsManager = meteoConditionsPageUI;
        _eventsListPageUI = eventsListPageUI;
        _settingsPageUI = settingsPageUI;
        _exitPageUI = exitPageUI;
        _customEventsManager = customEventsManager;
        _selectedUIItemController = selectedUIItemController;
        _navigationPanelUI = navigationPanelUI;
        _uiManager = uiManager;

        _allFiringMachinePages = allFiringMachinePages.ToDictionary(firingMachinePage => firingMachinePage.PageType);
    }

    public void Initialize()
    {
        ResetInterfacesStatesToDefault();

        _chooseOperatingModePageUI.MeteoConditionsButtonPressed +=
            ChooseOperatingModePageUIOnMeteoConditionsButtonPressed;

        _meteoConditionsManager.HideRequested += MeteoConditionsManager_OnHideRequested;

        _chooseOperatingModePageUI.DutyModeButtonPressed += ChooseOperatingModePageUIOnDutyModeButtonPressed;
        _allFiringMachinePages[FiringMachinesPageType.DutyMode].HideRequested += DutyModePageUI_OnHideRequested;

        _chooseOperatingModePageUI.EventsListButtonPressed += ChooseOperatingModePageUIOnEventsListButtonPressed;
        _eventsListPageUI.HideRequested += EventsListPageUI_OnHideRequested;

        _chooseOperatingModePageUI.SettingsButtonPressed += ChooseOperatingModePageUIOnSettingsButtonPressed;
        _settingsPageUI.HideRequested += SettingsPageUI_OnHideRequested;

        _chooseOperatingModePageUI.HideRequested += ChooseOperatingModePageUIOnHideRequested;
        _exitPageUI.HideRequested += ExitPageUI_OnHideRequested;

        _chooseOperatingModePageUI.PageShown += ChooseOperatingModePageUI_OnPageShown;
    }

    private void ChooseOperatingModePageUI_OnPageShown()
    {
        _chooseOperatingModePageUI.UpdateVisual();
    }

    private void ChooseOperatingModePageUIOnMeteoConditionsButtonPressed()
    {
        _uiManager.ChangeCurrentInterface(_meteoConditionsManager);
    }

    private void MeteoConditionsManager_OnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_chooseOperatingModePageUI);
    }

    private void ChooseOperatingModePageUIOnDutyModeButtonPressed()
    {
        _uiManager.ChangeCurrentInterface(_allFiringMachinePages[FiringMachinesPageType.DutyMode]);

        _selectedUIItemController.BlockInteraction();
        _navigationPanelUI.Hide();
    }

    private void DutyModePageUI_OnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_chooseOperatingModePageUI);

        _selectedUIItemController.UnlockInteraction();
        _navigationPanelUI.Show(NavigationPanelType.Base);
    }

    private void ChooseOperatingModePageUIOnEventsListButtonPressed()
    {
        _uiManager.ChangeCurrentInterface(_eventsListPageUI);

        _navigationPanelUI.Show(NavigationPanelType.EventsList);
    }

    private void EventsListPageUI_OnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_chooseOperatingModePageUI);

        _navigationPanelUI.Show(NavigationPanelType.Base);
    }

    private void ChooseOperatingModePageUIOnSettingsButtonPressed()
    {
        _uiManager.ChangeCurrentInterface(_settingsPageUI);

        _customEventsManager.AddEvent("Настройка");
    }

    private void SettingsPageUI_OnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_chooseOperatingModePageUI);
    }

    private void ChooseOperatingModePageUIOnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_exitPageUI);
    }

    private void ExitPageUI_OnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_chooseOperatingModePageUI);
    }

    #endregion

    private void ResetInterfacesStatesToDefault()
    {
        _meteoConditionsManager.Hide();
        _allFiringMachinePages[FiringMachinesPageType.DutyMode].Hide();
        _eventsListPageUI.Hide();
        _settingsPageUI.Hide();
        _exitPageUI.Hide();
        _chooseOperatingModePageUI.Hide();

        _navigationPanelUI.Hide();
    }

    public void OnSceneReset()
    {
        ResetInterfacesStatesToDefault();

        _selectedUIItemController.UnlockInteraction();
    }

    public void Dispose()
    {
        _chooseOperatingModePageUI.MeteoConditionsButtonPressed -=
            ChooseOperatingModePageUIOnMeteoConditionsButtonPressed;

        _meteoConditionsManager.HideRequested -= MeteoConditionsManager_OnHideRequested;

        _chooseOperatingModePageUI.DutyModeButtonPressed -= ChooseOperatingModePageUIOnDutyModeButtonPressed;
        _allFiringMachinePages[FiringMachinesPageType.DutyMode].HideRequested -= DutyModePageUI_OnHideRequested;

        _chooseOperatingModePageUI.EventsListButtonPressed -= ChooseOperatingModePageUIOnEventsListButtonPressed;
        _eventsListPageUI.HideRequested -= EventsListPageUI_OnHideRequested;

        _chooseOperatingModePageUI.SettingsButtonPressed -= ChooseOperatingModePageUIOnSettingsButtonPressed;
        _settingsPageUI.HideRequested -= SettingsPageUI_OnHideRequested;

        _chooseOperatingModePageUI.HideRequested -= ChooseOperatingModePageUIOnHideRequested;
        _exitPageUI.HideRequested -= ExitPageUI_OnHideRequested;

        _chooseOperatingModePageUI.PageShown -= ChooseOperatingModePageUI_OnPageShown;
    }
}