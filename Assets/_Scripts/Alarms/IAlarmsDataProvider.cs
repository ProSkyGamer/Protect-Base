#region

using System.Collections.Generic;

#endregion

public interface IAlarmsDataProvider
{
    public IReadOnlyList<AlarmSingle> GetAllAlarmsSingleByData(int deviceNumber, int firingMachineIndex);

    public int MaxDeviceNumber { get; }
    public int MinDeviceNumber { get; }

    public int MaxAlarmNumber { get; }
    public int MinAlarmNumber { get; }
}