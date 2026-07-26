#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

#endregion

public class AlarmsManager : IAlarmsTriggerer, IAlarmSaver, IAlarmsDataProvider, IInitializable, IOperationsStatusListener, ISceneResettable
{
    #region Events

    public event Action<IReadOnlyList<AlarmSingle>> NewAlarmsTriggered;

    public event Action<IReadOnlyList<AlarmSingle>> TriggeredAlarmsRemoved;

    #endregion

    #region Enums

    #endregion

    #region Variables & References

    private readonly List<IAlarmDeviceTriggerer> _allAlarmTriggers = new();

    private readonly List<AlarmSingle> _currentActiveAlarmsDictionary = new();

    private List<AlarmSingle> _currentSavedAlarms = new();
    private IDataSavingManager _dataSavingManager;

    #endregion

    #region Properties

    public int MaxDeviceNumber { get; } = 4;

    public int MinDeviceNumber => 0;

    public int MaxAlarmNumber { get; } = 255;

    public int MinAlarmNumber => 0;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(List<IAlarmDeviceTriggerer> allAlarmTriggers, IDataSavingManager dataSavingManager)
    {
        _allAlarmTriggers.AddRange(allAlarmTriggers);
        _dataSavingManager = dataSavingManager;
    }

    public void OperationStarted()
    {
        _currentSavedAlarms = _dataSavingManager.GetAllSavedAlarms();
    }

    public void OperationEnded()
    {
        _currentSavedAlarms.Clear();
    }

    public void Initialize()
    {
        foreach (IAlarmDeviceTriggerer alarmDeviceZoneTrigger in _allAlarmTriggers)
        {
            alarmDeviceZoneTrigger.OnAlarmDeviceTriggered += AlarmDeviceTriggerZoneSingle_OnAlarmDeviceTriggered;
        }
    }

    private void AlarmDeviceTriggerZoneSingle_OnAlarmDeviceTriggered(int triggeredAlarmDeviceIndex)
    {
        TriggerAlarm(triggeredAlarmDeviceIndex);
    }

    #endregion

    #region Set

    public void RemoveAllAlarms()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        _dataSavingManager.ClearSavedAlarms();

        Debug.Log("Cleared all alarms!");
    }

    public void AddAlarmSingle(int deviceNumber, int alarmNumber, int firingMachineNumber, int preSettingNumber,
        FiringMachineEnableType firingMachineEnableType, AlarmType alarmType)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        AlarmSingle alarmSingle = new(deviceNumber, alarmNumber, firingMachineNumber, preSettingNumber, firingMachineEnableType, alarmType);

        _dataSavingManager.SaveAlarm(alarmSingle);

        Debug.Log($"Added/changed alarm");
    }

    #endregion

    #region Alarms Trigger

    private void TriggerAlarm(int deviceNumber)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        List<AlarmSingle> allTriggeringAlarms = _currentSavedAlarms.Where(savedAlarm => savedAlarm.DeviceNumber == deviceNumber).ToList();

        if (allTriggeringAlarms.Count == 0)
            return;

        List<AlarmSingle> newlyTriggeredAlarms = new();

        foreach (AlarmSingle triggeringAlarm in allTriggeringAlarms)
        {
            if (_currentActiveAlarmsDictionary.Contains(triggeringAlarm) == false)
            {
                _currentActiveAlarmsDictionary.Add(triggeringAlarm);
                newlyTriggeredAlarms.Add(triggeringAlarm);
            }
        }

        if (newlyTriggeredAlarms.Count != 0)
            NewAlarmsTriggered?.Invoke(newlyTriggeredAlarms);
    }

    public void RemoveTriggeredAlarm(int firingMachineNumber)
    {
        List<AlarmSingle> allAlarmsTriggeredWithFiringMachineNumber = _currentActiveAlarmsDictionary
            .Where(activeAlarm => activeAlarm.FiringMachineNumber == firingMachineNumber).ToList();

        List<AlarmSingle> removingActiveAlarms = allAlarmsTriggeredWithFiringMachineNumber
            .Select(activeAlarmPair => activeAlarmPair).ToList();

        foreach (AlarmSingle removingAlarm in removingActiveAlarms)
        {
            _currentActiveAlarmsDictionary.Remove(removingAlarm);
        }

        if (removingActiveAlarms.Count != 0) TriggeredAlarmsRemoved?.Invoke(removingActiveAlarms);
    }

    public IReadOnlyList<AlarmSingle> GetCurrentActiveAlarms()
    {
        return _currentActiveAlarmsDictionary;
    }

    #endregion

    #region Get

    private IReadOnlyList<AlarmSingle> GetAllAlarmsSingleByData(int deviceNumber)
    {
        List<AlarmSingle> allSavedAlarms =
            _dataSavingManager.GetAllSavedAlarms().Where(alarmSingle => alarmSingle.DeviceNumber == deviceNumber).ToList();

        return allSavedAlarms;
    }

    public IReadOnlyList<AlarmSingle> GetAllAlarmsSingleByData(int deviceNumber, int firingMachineIndex)
    {
        List<AlarmSingle> allSavedAlarms = _dataSavingManager.GetAllSavedAlarms().Where(alarmSingle =>
            alarmSingle.DeviceNumber == deviceNumber && alarmSingle.FiringMachineNumber == firingMachineIndex).ToList();

        return allSavedAlarms;
    }

    #endregion

    public void OnSceneReset()
    {
        _currentActiveAlarmsDictionary.Clear();
    }
}