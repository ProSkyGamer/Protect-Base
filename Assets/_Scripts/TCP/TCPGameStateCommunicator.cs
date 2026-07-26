#region

using System;
using System.Collections.Generic;
using Zenject;

#endregion

public class TCPGameStateCommunicator : IInitializable, IDisposable
{
    private readonly TCPServerConnector _tcpServerConnector;
    private readonly AlarmsManager _alarmsManager;

    public TCPGameStateCommunicator(TCPServerConnector tcpServerConnector, AlarmsManager alarmsManager)
    {
        _tcpServerConnector = tcpServerConnector;
        _alarmsManager = alarmsManager;
    }

    public void Initialize()
    {
        _alarmsManager.NewAlarmsTriggered += AlarmsManager_OnNewAlarmsTriggered;
        _alarmsManager.TriggeredAlarmsRemoved += AlarmsManagerTriggeredAlarmsRemoved;
    }

    private void AlarmsManagerTriggeredAlarmsRemoved(
        IReadOnlyCollection<AlarmSingle> removedTriggeredAlarms)
    {
        foreach (AlarmSingle removedTriggeredAlarm in removedTriggeredAlarms)
        {
            _tcpServerConnector.SendMessageByConnection(
                GetAlarmInfoString(removedTriggeredAlarm.FiringMachineNumber + 1, false));
        }
    }

    private void AlarmsManager_OnNewAlarmsTriggered(IReadOnlyCollection<AlarmSingle> newlyTriggeredAlarms)
    {
        foreach (AlarmSingle newlyTriggeredAlarm in newlyTriggeredAlarms)
        {
            _tcpServerConnector.SendMessageByConnection(
                GetAlarmInfoString(newlyTriggeredAlarm.FiringMachineNumber + 1, true));
        }
    }

    private string GetAlarmInfoString(int firingMachineIndex, bool isActive)
    {
        string firingMachineStateInfoString = "alarm_{0}_[{1}]";

        firingMachineStateInfoString = string.Format(firingMachineStateInfoString, isActive ? "start" : "stop",
            firingMachineIndex);

        return firingMachineStateInfoString;
    }

    public void Dispose()
    {
        _alarmsManager.NewAlarmsTriggered -= AlarmsManager_OnNewAlarmsTriggered;
        _alarmsManager.TriggeredAlarmsRemoved -= AlarmsManagerTriggeredAlarmsRemoved;
    }
}