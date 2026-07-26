#region

using System;
using System.Collections.Generic;
using Zenject;

#endregion

public class OperatorsLoginObserver : IInitializable, IDisposable
{
    private readonly OperatorsLoginManager _loginManager;
    private readonly IAlarmsTriggerer _alarmsTriggerer;
    private readonly CustomEventsManager _customEventsManager;

    public OperatorsLoginObserver(OperatorsLoginManager loginManager, IAlarmsTriggerer alarmsTriggerer,
        CustomEventsManager customEventsManager)
    {
        _loginManager = loginManager;
        _alarmsTriggerer = alarmsTriggerer;
        _customEventsManager = customEventsManager;
    }

    public void Initialize()
    {
        _loginManager.LoginedSuccessfully += LoginManagerLoginedSuccessfully;
        _loginManager.LoginFailed += LoginManagerLoginFailed;
        _loginManager.LoggedOutSuccessfully += LoginManagerLoggedOutSuccessfully;

        _alarmsTriggerer.NewAlarmsTriggered += AlarmsTriggererNewAlarmsTriggered;
        _customEventsManager.ListUpdated += CustomEventsManagerListUpdated;
    }

    private void CustomEventsManagerListUpdated()
    {
        _loginManager.AddEventsTriggerCount(1);
    }

    private void AlarmsTriggererNewAlarmsTriggered(IReadOnlyList<AlarmSingle> triggeredAlarms)
    {
        _loginManager.AddAlarmsTriggerCount(triggeredAlarms.Count);
    }

    private void LoginManagerLoggedOutSuccessfully()
    {
        _customEventsManager.AddEvent("Регистрация");
    }

    private void LoginManagerLoginFailed()
    {
        _customEventsManager.AddEvent("Подбор ПИН");
    }

    private void LoginManagerLoginedSuccessfully()
    {
        _customEventsManager.AddEvent("Завершение работы");
    }

    public void Dispose()
    {
        _loginManager.LoginedSuccessfully -= LoginManagerLoginedSuccessfully;
        _loginManager.LoginFailed -= LoginManagerLoginFailed;
        _loginManager.LoggedOutSuccessfully -= LoginManagerLoggedOutSuccessfully;

        _alarmsTriggerer.NewAlarmsTriggered -= AlarmsTriggererNewAlarmsTriggered;
        _customEventsManager.ListUpdated -= CustomEventsManagerListUpdated;
    }
}