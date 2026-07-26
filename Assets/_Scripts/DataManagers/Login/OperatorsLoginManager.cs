#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class LoginInfo
{
    public int Login { get; }
    public string Password { get; }

    public LoginInfo(int login, string password)
    {
        Login = login;
        Password = password;
    }
}

public class OperatorsLoginManager : ILoginDataProvider, IInitializable, ISceneResettable
{
    #region Events

    public event Action LoginedSuccessfully;

    public event Action LoginFailed;

    public event Action LoggedOutSuccessfully;

    #endregion

    #region Created Classes

    private class LoginedUserInfo
    {
        public int CurrentLoginedUserIndex { get; }
        public DateTime LoginTime { get; }
        public int EventsCounts { get; private set; }
        public int AlarmsCount { get; private set; }

        public LoginedUserInfo(int userIndex, DateTime loginTime)
        {
            CurrentLoginedUserIndex = userIndex;
            LoginTime = loginTime;
            EventsCounts = 0;
            AlarmsCount = 0;
        }

        public void IncreaseEventCounts(int addingCount)
        {
            if (addingCount <= 0)
                return;

            EventsCounts += addingCount;
        }

        public void IncreaseAlarmsCount(int addingCount)
        {
            if (addingCount <= 0)
                return;

            AlarmsCount += addingCount;
        }
    }

    #endregion

    #region Variables & References

    private readonly int _maxUnsuccessfullLoginAttempts = 3;
    private int _attemptsLeft;
    private bool _isNotificationSend;

    private LoginedUserInfo _loginedUserInfo;
    private readonly List<LoginInfo> _allLoginsInfo = new();
    private int _previousAlarmsListCount;

    private readonly ICurrentDateTimeProvider _dateTimeProvider;
    private readonly IDataSavingManager _dataSavingManager;

    public ReadonlyLoginedUser LoginedUser =>
        new()
        {
            AlarmsCount = _loginedUserInfo.AlarmsCount,
            CurrentLoginedUserIndex = _loginedUserInfo.CurrentLoginedUserIndex,
            EventsCounts = _loginedUserInfo.EventsCounts,
            LoginTime = _loginedUserInfo.LoginTime
        };

    public int MaxUserLoginIndex { get; } = 99;

    public int MinUserLoginIndex => 0;

    #endregion

    #region Initializaion

    public OperatorsLoginManager(ICurrentDateTimeProvider dateTimeProvider, IDataSavingManager dataSavingManager)
    {
        _dateTimeProvider = dateTimeProvider;
        _dataSavingManager = dataSavingManager;
    }

    public void Initialize()
    {
        _allLoginsInfo.AddRange(_dataSavingManager.GetAllSavedLoginInfos());

        _loginedUserInfo = new LoginedUserInfo(0, _dateTimeProvider.CurrentDateTime);

        _attemptsLeft = _maxUnsuccessfullLoginAttempts;
        _isNotificationSend = false;
    }

    public void AddEventsTriggerCount(int addingCount)
    {
        _loginedUserInfo.IncreaseEventCounts(addingCount);
    }

    public void AddAlarmsTriggerCount(int addingCount)
    {
        _loginedUserInfo.IncreaseAlarmsCount(addingCount);
    }

    #endregion

    #region Login

    public bool WouldLoginSuccessful(int login, string password)
    {
        if (ClientTypeManager.CurrentClientType != ClientType.Game)
            return false;

        return IsLoginSuccessful(login, password);
    }

    public void Login(int login, string password)
    {
        if (IsLoginSuccessful(login, password))
        {
            _loginedUserInfo = new(login, _dateTimeProvider.CurrentDateTime);

            Debug.Log($"Operator login: {login}");

            LoginedSuccessfully?.Invoke();
        }

        _attemptsLeft--;

        if (_attemptsLeft <= 0 && !_isNotificationSend)
        {
            _isNotificationSend = true;
            LoginFailed?.Invoke();
        }
    }

    public void LogOut(string password)
    {
        LoginInfo currentUserLoginData = _allLoginsInfo.Find(loginInfo => loginInfo.Login == _loginedUserInfo.CurrentLoginedUserIndex);

        if (currentUserLoginData.Password != password)
            return;

        _attemptsLeft = _maxUnsuccessfullLoginAttempts;
        _isNotificationSend = false;

        LoggedOutSuccessfully?.Invoke();
    }

    public bool WouldLoggedOutSuccessful(string password)
    {
        LoginInfo currentUserLoginData = _allLoginsInfo.Find(loginInfo => loginInfo.Login == _loginedUserInfo.CurrentLoginedUserIndex);

        return currentUserLoginData?.Password == password;
    }

    public void ChangeOperatorPassword(int login, string newPassword)
    {
        if (ClientTypeManager.CurrentClientType != ClientType.Game)
            return;

        int loginIndex = _allLoginsInfo.FindIndex(loginInfo => loginInfo.Login == login);

        if (loginIndex <= 0)
            return;

        _allLoginsInfo[loginIndex] = new LoginInfo(login, newPassword);

        _dataSavingManager.SaveLoginInfo(_allLoginsInfo[loginIndex]);
    }

    #endregion

    #region Get

    private bool IsLoginSuccessful(int login, string password)
    {
        LoginInfo currentUserLoginData = _allLoginsInfo.Find(loginInfo => loginInfo.Login == login);

        return currentUserLoginData?.Password == password;
    }

    #endregion

    public void OnSceneReset()
    {
        _loginedUserInfo = new LoginedUserInfo(0, _dateTimeProvider.CurrentDateTime);

        _attemptsLeft = _maxUnsuccessfullLoginAttempts;
        _isNotificationSend = false;
    }
}