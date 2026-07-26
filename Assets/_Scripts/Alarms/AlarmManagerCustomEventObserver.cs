#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class AlarmManagerCustomEventObserver : IInitializable, IDisposable
{
    private readonly AlarmsManager _alarmsManager;
    private readonly CustomEventsManager _customEventsManager;
    private readonly StringFormatsSO _stringFormatsSO;

    public AlarmManagerCustomEventObserver(AlarmsManager alarmsManager, CustomEventsManager customEventsManager, StringFormatsSO stringFormatsSO)
    {
        _alarmsManager = alarmsManager;
        _customEventsManager = customEventsManager;
        _stringFormatsSO = stringFormatsSO;
    }

    public void Initialize()
    {
        _alarmsManager.NewAlarmsTriggered += AlarmsManagerOnNewAlarmsTriggered;
    }

    private void AlarmsManagerOnNewAlarmsTriggered(IReadOnlyCollection<AlarmSingle> newlyTriggeredAlarms)
    {
        foreach (AlarmSingle newlyTriggeredAlarm in newlyTriggeredAlarms)
        {
            string eventText = string.Format(_stringFormatsSO.EventTextFormatString, newlyTriggeredAlarm.DeviceNumber,
                newlyTriggeredAlarm.AlarmNumber, newlyTriggeredAlarm.FiringMachineNumber);

            Debug.Log($"Triggered alarm from device {newlyTriggeredAlarm.DeviceNumber} from alarm " +
                      $"{newlyTriggeredAlarm.AlarmNumber} on firing machine {newlyTriggeredAlarm.FiringMachineNumber}");

            _customEventsManager.AddEvent(eventText);
        }
    }

    public void Dispose()
    {
        _alarmsManager.NewAlarmsTriggered -= AlarmsManagerOnNewAlarmsTriggered;
    }
}