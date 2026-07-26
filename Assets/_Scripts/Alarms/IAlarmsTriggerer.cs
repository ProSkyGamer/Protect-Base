#region

using System;
using System.Collections.Generic;

#endregion

public interface IAlarmsTriggerer
{
    public event Action<IReadOnlyList<AlarmSingle>> NewAlarmsTriggered;

    public event Action<IReadOnlyList<AlarmSingle>> TriggeredAlarmsRemoved;

    public void RemoveTriggeredAlarm(int firingMachineNumber);

    public IReadOnlyList<AlarmSingle> GetCurrentActiveAlarms();
}