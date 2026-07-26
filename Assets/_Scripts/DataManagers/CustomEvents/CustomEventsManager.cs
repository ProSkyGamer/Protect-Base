#region

using System;
using System.Collections.Generic;
using Zenject;

#endregion

public class CustomEventsManager : ICustomEventsProvider
{
    #region Events

    public event Action ListUpdated;

    #endregion

    #region Variables & References

    private readonly int _savingLastEventsCount = 105;

    private OperatorsLoginManager _operatorsLoginManager;
    private IDataSavingManager _dataSavingManager;

    #endregion

    #region Inialization

    [Inject]
    public void Construct(OperatorsLoginManager operatorsLoginManager, IDataSavingManager dataSavingManager)
    {
        _operatorsLoginManager = operatorsLoginManager;
        _dataSavingManager = dataSavingManager;
    }

    #endregion

    #region Custom Events

    public void AddEvent(string eventText)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game) return;

        DateTime eventTime = DateTime.Now;
        string operatorText = _operatorsLoginManager.LoginedUser.CurrentLoginedUserIndex.ToString();

        CustomEvent newEvent = new(eventTime, eventText, operatorText);

        _dataSavingManager.SaveCustomEvent(newEvent, IsAllIndexesUsed());

        ListUpdated?.Invoke();
    }

    #endregion

    #region Get

    public List<CustomEvent> GetAllEventsList()
    {
        return _dataSavingManager.GetAllSavedCustomEvents();
    }

    private bool IsAllIndexesUsed()
    {
        int currentEventCount = _dataSavingManager.GetSavedCustomEventsCount();

        return currentEventCount > _savingLastEventsCount;
    }

    #endregion
}