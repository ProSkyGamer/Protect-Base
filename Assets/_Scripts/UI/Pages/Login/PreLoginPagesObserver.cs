#region

using System;
using Zenject;

#endregion

public class PreLoginPagesObserver : IInitializable, IDisposable, ISceneResettable
{
    #region Variables & References

    private readonly WaitingConnectionUI _waitingConnectionUI;
    private readonly MainPageUI _mainPageUI;
    private readonly RegistrationPageUI _registrationPageUI;
    private readonly ChooseOperatingModePageUI _chooseOperatingModePageUI;
    private readonly ExitPageUI _exitPageUI;
    private readonly NavigationPanelUI _navigationPanelUI;

    private readonly OperatorsLoginManager _operatorsLoginManager;
    private readonly UIManager _uiManager;
    private readonly CustomEventsManager _customEventsManager;
    private readonly DYMNetworkManager _dymNetworkManager;

    #endregion

    #region Initialization

    public PreLoginPagesObserver(WaitingConnectionUI waitingConnectionUI, DYMNetworkManager dymNetworkManager,
        MainPageUI mainPageUI, CustomEventsManager customEventsManager,
        RegistrationPageUI registrationPageUI, ChooseOperatingModePageUI chooseOperatingModePageUI,
        ExitPageUI exitPageUI,
        NavigationPanelUI navigationPanelUI, UIManager uiManager, OperatorsLoginManager operatorsLoginManager)
    {
        _waitingConnectionUI = waitingConnectionUI;
        _dymNetworkManager = dymNetworkManager;
        _mainPageUI = mainPageUI;
        _customEventsManager = customEventsManager;
        _registrationPageUI = registrationPageUI;
        _chooseOperatingModePageUI = chooseOperatingModePageUI;
        _exitPageUI = exitPageUI;
        _navigationPanelUI = navigationPanelUI;
        _uiManager = uiManager;
        _operatorsLoginManager = operatorsLoginManager;
    }

    public void Initialize()
    {
        ResetInterfacesStatesToDefault();

        _dymNetworkManager.OnDYMNetworkStateChanged += DymNetworkManager_OnDYMNetworkStateChanged;
        _mainPageUI.HideRequested += MainPageUI_OnHideRequested;

        _operatorsLoginManager.LoginedSuccessfully += OperatorsLoginManager_OnLoginedSuccessfully;
        _operatorsLoginManager.LoggedOutSuccessfully += OperatorsLoginManager_OnLoggedOutSuccessfully;
    }

    private void OperatorsLoginManager_OnLoggedOutSuccessfully()
    {
        _uiManager.ChangeCurrentInterface(_mainPageUI);

        _navigationPanelUI.Hide();
    }

    private void OperatorsLoginManager_OnLoginedSuccessfully()
    {
        _uiManager.ChangeCurrentInterface(_chooseOperatingModePageUI);
    }

    private void MainPageUI_OnHideRequested()
    {
        _uiManager.ChangeCurrentInterface(_registrationPageUI);

        _customEventsManager.AddEvent("Начало работы");

        _navigationPanelUI.Show(NavigationPanelType.Base);
    }

    private void DymNetworkManager_OnDYMNetworkStateChanged(bool networkNewState)
    {
        if (networkNewState)
            _waitingConnectionUI.Hide();
        else
            _waitingConnectionUI.Show();
    }

    #endregion

    private void ResetInterfacesStatesToDefault()
    {
        _uiManager.ChangeCurrentInterface(_mainPageUI);
        _registrationPageUI.Hide();
        _exitPageUI.Hide();
        _chooseOperatingModePageUI.Hide();
    }

    public void OnSceneReset()
    {
        ResetInterfacesStatesToDefault();
    }

    public void Dispose()
    {
        _dymNetworkManager.OnDYMNetworkStateChanged -= DymNetworkManager_OnDYMNetworkStateChanged;
        _mainPageUI.HideRequested -= MainPageUI_OnHideRequested;

        _operatorsLoginManager.LoginedSuccessfully -= OperatorsLoginManager_OnLoginedSuccessfully;
        _operatorsLoginManager.LoggedOutSuccessfully -= OperatorsLoginManager_OnLoggedOutSuccessfully;
    }
}