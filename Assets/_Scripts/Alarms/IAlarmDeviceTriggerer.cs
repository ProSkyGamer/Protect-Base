#region

using System;

#endregion

public interface IAlarmDeviceTriggerer
{
    public event Action<int> OnAlarmDeviceTriggered;
}