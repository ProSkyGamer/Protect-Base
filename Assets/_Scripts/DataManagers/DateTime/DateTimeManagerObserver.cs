#region

using System;
using Zenject;

#endregion

public class DateTimeManagerObserver : IDisposable, IInitializable
{
    private readonly CurrentDateTimeManager _dateTimeManager;
    private readonly CustomEventsManager _customEventsManager;

    public DateTimeManagerObserver(CurrentDateTimeManager dateTimeManager, CustomEventsManager customEventsManager)
    {
        _dateTimeManager = dateTimeManager;
        _customEventsManager = customEventsManager;
    }

    public void Initialize()
    {
        _dateTimeManager.NewDayReached += DateTimeManager_OnNewDayReached;
    }

    private void DateTimeManager_OnNewDayReached()
    {
        DateTime currentDateTime = _dateTimeManager.CurrentDateTime;
        _customEventsManager.AddEvent($"{currentDateTime.Day}.{currentDateTime.Month}.{currentDateTime.Year}");
    }

    public void Dispose()
    {
        _dateTimeManager.NewDayReached -= DateTimeManager_OnNewDayReached;
    }
}