#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

public class PlayerPrefsDataSavingManager : IDataSavingManager
{
    #region Alarms

    private enum AlarmStat
    {
        IsIndexUsed,
        DeviceNumber,
        AlarmNumber,
        FiringMachineNumber,
        PreSettingNumber,
        FiringMachineEnableType,
        AlarmType
    }

    private const string AlarmStatSinglePlayerPrefs = "AlarmStatSinglePlayerPrefs_{0}_{1}";

    public void SaveAlarm(AlarmSingle alarmSingle)
    {
        int savedAlarmIndex = GetAlarmIndex(alarmSingle.DeviceNumber, alarmSingle.AlarmNumber, alarmSingle.FiringMachineNumber);

        PlayerPrefs.SetInt(GetAlarmsPlayerPrefsAccessString(AlarmStat.IsIndexUsed, savedAlarmIndex), 1);

        PlayerPrefs.SetInt(GetAlarmsPlayerPrefsAccessString(AlarmStat.DeviceNumber, savedAlarmIndex),
            alarmSingle.DeviceNumber);

        PlayerPrefs.SetInt(GetAlarmsPlayerPrefsAccessString(AlarmStat.AlarmNumber, savedAlarmIndex),
            alarmSingle.AlarmNumber);

        PlayerPrefs.SetInt(GetAlarmsPlayerPrefsAccessString(AlarmStat.FiringMachineNumber, savedAlarmIndex),
            alarmSingle.FiringMachineNumber);

        PlayerPrefs.SetInt(GetAlarmsPlayerPrefsAccessString(AlarmStat.PreSettingNumber, savedAlarmIndex),
            alarmSingle.PreSettingNumber);

        PlayerPrefs.SetInt(
            GetAlarmsPlayerPrefsAccessString(AlarmStat.FiringMachineEnableType, savedAlarmIndex),
            (int)alarmSingle.FiringMachineEnableType);

        PlayerPrefs.SetInt(GetAlarmsPlayerPrefsAccessString(AlarmStat.AlarmType, savedAlarmIndex),
            (int)alarmSingle.AlarmType);
    }

    private int GetAlarmIndex(int deviceNumber, int alarmNumber, int firingMachineNumber)
    {
        List<AlarmSingle> allSavedAlarms = GetAllSavedAlarms();

        int savedAlarmIndex = allSavedAlarms.FindIndex(savedAlarm =>
            savedAlarm.DeviceNumber == deviceNumber && savedAlarm.AlarmNumber == alarmNumber &&
            savedAlarm.FiringMachineNumber == firingMachineNumber);

        return savedAlarmIndex >= 0 ? savedAlarmIndex : GetFirstUnusedAlarmIndex();
    }

    public void ClearSavedAlarms()
    {
        int currentAlarmIndex = 0;

        while (IsAlarmIndexUsed(currentAlarmIndex))
        {
            PlayerPrefs.SetInt(
                GetAlarmsPlayerPrefsAccessString(AlarmStat.IsIndexUsed, currentAlarmIndex), 0);

            PlayerPrefs.SetInt(
                GetAlarmsPlayerPrefsAccessString(AlarmStat.DeviceNumber, currentAlarmIndex), 0);

            PlayerPrefs.SetInt(
                GetAlarmsPlayerPrefsAccessString(AlarmStat.AlarmNumber, currentAlarmIndex), 0);

            PlayerPrefs.SetInt(
                GetAlarmsPlayerPrefsAccessString(AlarmStat.FiringMachineNumber, currentAlarmIndex), 0);

            PlayerPrefs.SetInt(
                GetAlarmsPlayerPrefsAccessString(AlarmStat.PreSettingNumber, currentAlarmIndex), 0);

            PlayerPrefs.SetInt(
                GetAlarmsPlayerPrefsAccessString(AlarmStat.FiringMachineEnableType, currentAlarmIndex),
                0);

            PlayerPrefs.SetInt(GetAlarmsPlayerPrefsAccessString(AlarmStat.AlarmType, currentAlarmIndex),
                0);

            currentAlarmIndex++;
        }
    }

    public List<AlarmSingle> GetAllSavedAlarms()
    {
        int currentAlarmIndex = 0;
        List<AlarmSingle> allFoundAlarmsSingle = new();

        while (IsAlarmIndexUsed(currentAlarmIndex))
        {
            AlarmSingle currentAlarmSingle = GetAlarmSingleByIndex(currentAlarmIndex);

            currentAlarmIndex++;

            allFoundAlarmsSingle.Add(currentAlarmSingle);
        }

        return allFoundAlarmsSingle;
    }

    private AlarmSingle GetAlarmSingleByIndex(int alarmIndex)
    {
        int deviceNumber = GetAlarmsPlayerPrefsValue(AlarmStat.DeviceNumber, alarmIndex);
        int alarmNumber = GetAlarmsPlayerPrefsValue(AlarmStat.AlarmNumber, alarmIndex);
        AlarmType alarmType = (AlarmType)GetAlarmsPlayerPrefsValue(AlarmStat.AlarmType, alarmIndex);

        FiringMachineEnableType firingMachineEnableType =
            (FiringMachineEnableType)GetAlarmsPlayerPrefsValue(AlarmStat.FiringMachineEnableType, alarmIndex);

        int firingMachineNumber = GetAlarmsPlayerPrefsValue(AlarmStat.FiringMachineNumber, alarmIndex);
        int preSettingNumber = GetAlarmsPlayerPrefsValue(AlarmStat.PreSettingNumber, alarmIndex);

        AlarmSingle alarmSingle = new(deviceNumber, alarmNumber, firingMachineNumber, preSettingNumber,
            firingMachineEnableType, alarmType);

        return alarmSingle;
    }

    private int GetAlarmsPlayerPrefsValue(AlarmStat alarmStat, int alarmIndex)
    {
        return PlayerPrefs.GetInt(GetAlarmsPlayerPrefsAccessString(alarmStat, alarmIndex));
    }

    private string GetAlarmsPlayerPrefsAccessString(AlarmStat alarmStat, int alarmIndex)
    {
        string alarmStatSinglePlayerPrefs =
            string.Format(AlarmStatSinglePlayerPrefs, (int)alarmStat, alarmIndex);

        return alarmStatSinglePlayerPrefs;
    }

    private int GetFirstUnusedAlarmIndex()
    {
        int firstUnusedIndex = 0;

        while (IsAlarmIndexUsed(firstUnusedIndex))
            firstUnusedIndex++;

        return firstUnusedIndex;
    }

    private bool IsAlarmIndexUsed(int alarmIndex)
    {
        return PlayerPrefs.GetInt(GetAlarmsPlayerPrefsAccessString(AlarmStat.IsIndexUsed, alarmIndex)) == 1;
    }

    #endregion

    #region Custom Events

    private enum CustomEventStat
    {
        EventTime,
        EventName,
        EventOperator
    }

    private const string CustomEventStatSinglePlayPrefs = "CustomEventStatSinglePlayerPrefs_{0}_{1}";

    public void SaveCustomEvent(CustomEvent customEvent, bool isRemovingFirst)
    {
        int lastUsedIndex = GetLastUnusedEventIndex();

        if (isRemovingFirst)
            for (int i = 0; i < lastUsedIndex; i++)
            {
                SetEvent(GetEventByIndex(i), i - 1);

                PlayerPrefs.SetString(GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventTime, i), "");
                PlayerPrefs.SetString(GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventName, i), "");

                PlayerPrefs.SetString(GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventOperator, i),
                    "");
            }

        lastUsedIndex = GetLastUnusedEventIndex();

        PlayerPrefs.SetString(GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventTime, lastUsedIndex),
            customEvent.EventTime.ToString());

        PlayerPrefs.SetString(GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventName, lastUsedIndex),
            customEvent.EventName);

        PlayerPrefs.SetString(GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventOperator, lastUsedIndex),
            customEvent.EventOperator);
    }

    private void SetEvent(CustomEvent customEvent, int eventIndex)
    {
        PlayerPrefs.SetString(GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventTime, eventIndex),
            customEvent.EventTime.ToString());

        PlayerPrefs.SetString(GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventName, eventIndex),
            customEvent.EventName);

        PlayerPrefs.SetString(GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventOperator, eventIndex),
            customEvent.EventOperator);
    }

    public List<CustomEvent> GetAllSavedCustomEvents()
    {
        List<CustomEvent> allEventsList = new();
        int lastUnusedIndex = 0;

        while (IsEventIndexUsed(lastUnusedIndex))
        {
            allEventsList.Add(GetEventByIndex(lastUnusedIndex));
            lastUnusedIndex++;
        }

        return allEventsList;
    }

    public int GetSavedCustomEventsCount()
    {
        int lastUnusedIndex = 0;

        while (IsEventIndexUsed(lastUnusedIndex))
            lastUnusedIndex++;

        return lastUnusedIndex;
    }

    private CustomEvent GetEventByIndex(int eventIndex)
    {
        DateTime eventTime = DateTime.Parse(
            PlayerPrefs.GetString(
                GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventTime, eventIndex)));

        string eventOperator = PlayerPrefs.GetString(
            GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventOperator, eventIndex));

        string eventName = PlayerPrefs.GetString(
            GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventName, eventIndex));

        CustomEvent customEvent = new(eventTime, eventName, eventOperator);

        return customEvent;
    }

    private int GetLastUnusedEventIndex()
    {
        int lastUnusedIndex = 0;

        while (IsEventIndexUsed(lastUnusedIndex)) lastUnusedIndex++;

        return lastUnusedIndex;
    }

    private bool IsEventIndexUsed(int eventIndex)
    {
        return PlayerPrefs.GetString(
                   GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventTime, eventIndex)) !=
               "" ||
               PlayerPrefs.GetString(
                   GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventName, eventIndex)) !=
               "" || PlayerPrefs.GetString(
                   GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat.EventOperator, eventIndex)) != "";
    }

    private string GetEventStatsSinglePlayerPrefsAccessString(CustomEventStat eventStat, int eventIndex)
    {
        string eventStatsSinglePlayerPrefsAccessString =
            string.Format(CustomEventStatSinglePlayPrefs, (int)eventStat, eventIndex);

        return eventStatsSinglePlayerPrefsAccessString;
    }

    #endregion

    #region Meteo Conditions

    private const string TemperatureCurrentValuePlayerPrefs = "TemperatureCurrentValuePlayerPrefs";
    private const string PressureCurrentValuePlayerPrefs = "PressureCurrentValuePlayerPrefs";

    public void SaveMeteoConditions(MeteoConditions meteoConditions)
    {
        PlayerPrefs.SetInt(TemperatureCurrentValuePlayerPrefs, meteoConditions.TemperatureValue);
        PlayerPrefs.SetInt(PressureCurrentValuePlayerPrefs, meteoConditions.PressureValue);
    }

    public MeteoConditions GetSavedMeteoConditions()
    {
        int temperatureValue = PlayerPrefs.GetInt(TemperatureCurrentValuePlayerPrefs);
        int pressureValue = PlayerPrefs.GetInt(PressureCurrentValuePlayerPrefs);

        MeteoConditions meteoConditions = new MeteoConditions(temperatureValue, pressureValue);

        return meteoConditions;
    }

    #endregion

    private enum PreSettingStatSingle
    {
        IsPreSettingSet,
        PreSettingEulerAnglesX,
        PreSettingEulerAnglesY,
        PreSettingEulerAnglesZ,
        PreSettingZoom
    }

    private const string PreSettingStatSingleBasePlayerPrefs = "PreSettingStatSingleBasePlayePrefs_{0}_{1}_{2}";

    public void SavePreSetting(SavedPreSetting savedPreSetting)
    {
        PlayerPrefs.SetInt(GetPreSettingStatSinglePlayerPrefsAccessString(PreSettingStatSingle.IsPreSettingSet,
            savedPreSetting.FiringMachineNumber, savedPreSetting.PreSettingNumber), 1);

        PlayerPrefs.SetFloat(GetPreSettingStatSinglePlayerPrefsAccessString(
                PreSettingStatSingle.PreSettingEulerAnglesX, savedPreSetting.FiringMachineNumber, savedPreSetting.PreSettingNumber),
            savedPreSetting.PreSettingSingle.PreSettingEulerAngles.x);

        PlayerPrefs.SetFloat(GetPreSettingStatSinglePlayerPrefsAccessString(
                PreSettingStatSingle.PreSettingEulerAnglesY, savedPreSetting.FiringMachineNumber, savedPreSetting.PreSettingNumber),
            savedPreSetting.PreSettingSingle.PreSettingEulerAngles.y);

        PlayerPrefs.SetFloat(GetPreSettingStatSinglePlayerPrefsAccessString(
                PreSettingStatSingle.PreSettingEulerAnglesZ, savedPreSetting.FiringMachineNumber, savedPreSetting.PreSettingNumber),
            0f);

        PlayerPrefs.SetInt(GetPreSettingStatSinglePlayerPrefsAccessString(
                PreSettingStatSingle.PreSettingZoom, savedPreSetting.FiringMachineNumber, savedPreSetting.PreSettingNumber),
            savedPreSetting.PreSettingSingle.PreSettingZoom);
    }

    public List<SavedPreSetting> GetAllSavedPreSettings()
    {
        throw new NotImplementedException();

        /*bool isPreSettingSet = PlayerPrefs.GetInt(GetPreSettingStatSinglePlayerPrefsAccessString(PreSettingStatSingle.IsPreSettingSet,
            firingMachineNumber, firingMachinePreSettingNumber)) == 1;

        if (isPreSettingSet == false)
            return null;

        Vector3 preSettingEulerAngles = new Vector3(
            PlayerPrefs.GetFloat(GetPreSettingStatSinglePlayerPrefsAccessString(
                PreSettingStatSingle.PreSettingEulerAnglesX, firingMachineNumber, firingMachinePreSettingNumber)),
            PlayerPrefs.GetFloat(GetPreSettingStatSinglePlayerPrefsAccessString(
                PreSettingStatSingle.PreSettingEulerAnglesY, firingMachineNumber, firingMachinePreSettingNumber)),
            PlayerPrefs.GetFloat(GetPreSettingStatSinglePlayerPrefsAccessString(
                PreSettingStatSingle.PreSettingEulerAnglesZ, firingMachineNumber, firingMachinePreSettingNumber)));

        int preSettingZoom = PlayerPrefs.GetInt(GetPreSettingStatSinglePlayerPrefsAccessString(
            PreSettingStatSingle.PreSettingZoom, firingMachineNumber, firingMachinePreSettingNumber));

        PreSettingSingle preSettingSingle = new(preSettingEulerAngles, preSettingZoom);
        
        return null;*/
    }

    private string GetPreSettingStatSinglePlayerPrefsAccessString(PreSettingStatSingle preSettingStatSingle,
        int firingMachineNumber, int preSettingNumber)
    {
        string preSettingStatSinglePlayerPrefsAccessString = string.Format(PreSettingStatSingleBasePlayerPrefs,
            (int)preSettingStatSingle, firingMachineNumber, preSettingNumber);

        return preSettingStatSinglePlayerPrefsAccessString;
    }

    public void SaveFiringMachineAmmoTypes(List<FiringMachineAmmoTypes> allFiringMachineAmmoTypes)
    {
        throw new NotImplementedException();
    }

    public List<FiringMachineAmmoTypes> GetAllSavedFiringMachineAmmoTypes()
    {
        throw new NotImplementedException();
    }

    public void SaveOperationPreset(int overrideOperationIndex, ReadonlyOperationData operationData, string operationName)
    {
        throw new NotImplementedException();
    }

    public void SaveOperationPreset(ReadonlyOperationData operationData, string operationName, out int operationIndex)
    {
        throw new NotImplementedException();
    }

    public void RemoveOperationPreset(int operationIndex)
    {
        throw new NotImplementedException();
    }

    public void SaveOperationPreset(ReadonlyOperationData operationData, string operationName)
    {
        throw new NotImplementedException();
    }

    public List<SavedOperationData> GetAllSavedOperationPresets()
    {
        throw new NotImplementedException();
    }

    public void SaveAppSettings(AppSettingsData appSettingsData)
    {
        throw new NotImplementedException();
    }

    public AppSettingsData GetSavedAppSettings()
    {
        throw new NotImplementedException();
    }

    public void SaveCurrentDateTimeOffset(TimeSpan currentOffset)
    {
        throw new NotImplementedException();
    }

    public TimeSpan GetSavedDateTimeOffset()
    {
        throw new NotImplementedException();
    }

    public void SaveLoginInfo(LoginInfo loginInfo)
    {
        throw new NotImplementedException();
    }

    public List<LoginInfo> GetAllSavedLoginInfos()
    {
        throw new NotImplementedException();
    }
}