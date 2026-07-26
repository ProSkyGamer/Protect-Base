#region

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

#endregion

public class JSONDataSavingManager : IDataSavingManager
{
    #region Variables & References

    private string BaseFilePath => $"{Application.dataPath}/Resources/Saves";
    private string AlarmsFilePath => $"{BaseFilePath}/Alarms.json";
    private string CustomEventsFilePath => $"{BaseFilePath}/CustomEvents.json";
    private string MeteoConditionsFilePath => $"{BaseFilePath}/MeteoConditions.json";
    private string PreSettingsFilePath => $"{BaseFilePath}/PreSettings.json";
    private string FiringMachineAmmoTypesFilePath => $"{BaseFilePath}/FiringMachinesAmmoTypes.json";
    private string OperationPresetsFilePath => $"{BaseFilePath}/OperationPresets.json";
    private string AppSettingsFilePath => $"{BaseFilePath}/AppSettings.json";
    private string DateTimeOffsetFilePath => $"{BaseFilePath}/DateTimeOffset.json";
    private string LoginsFilePath => $"{BaseFilePath}/Logins.json";

    private readonly JsonSerializerSettings _jsonSerializerSettings = new()
    {
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
    };

    #endregion

    #region Alarms

    public void SaveAlarm(AlarmSingle alarmSingle)
    {
        List<AlarmSingle> allSavedAlarms = GetAllSavedAlarms();

        int savedAlarmIndex = allSavedAlarms.FindIndex(savedAlarm =>
            savedAlarm.DeviceNumber == alarmSingle.DeviceNumber && savedAlarm.AlarmNumber == alarmSingle.AlarmNumber &&
            savedAlarm.FiringMachineNumber == alarmSingle.FiringMachineNumber);

        if (savedAlarmIndex >= 0)
            allSavedAlarms[savedAlarmIndex] = alarmSingle;
        else
            allSavedAlarms.Add(alarmSingle);

        SaveAllAlarms(allSavedAlarms);
    }

    private void SaveAllAlarms(List<AlarmSingle> allSavedAlarms)
    {
        List<SerializableAlarm> allSavedSerializableAlarms = allSavedAlarms.Select(alarmSingle => new SerializableAlarm(alarmSingle)).ToList();

        string allSavedAlarmsJsonString = JsonConvert.SerializeObject(allSavedSerializableAlarms, _jsonSerializerSettings);

        File.WriteAllText(AlarmsFilePath, allSavedAlarmsJsonString);
    }

    public void ClearSavedAlarms()
    {
        SaveAllAlarms(new());
    }

    public List<AlarmSingle> GetAllSavedAlarms()
    {
        if (IsFileExist(AlarmsFilePath) == false)
            SaveAllAlarms(new());

        string allAlarmsJsonString = File.ReadAllText(AlarmsFilePath);

        List<AlarmSingle> allSavedAlarms = JsonConvert.DeserializeObject<List<SerializableAlarm>>(allAlarmsJsonString, _jsonSerializerSettings)
            .Select(serializableAlarm => serializableAlarm.GetAlarmSingle()).ToList();

        return allSavedAlarms;
    }

    #endregion

    #region Custom Events

    public void SaveCustomEvent(CustomEvent customEvent, bool isRemovingFirst)
    {
        List<CustomEvent> allSavedCustomEvents = GetAllSavedCustomEvents();

        if (isRemovingFirst)
            allSavedCustomEvents.RemoveAt(0);

        allSavedCustomEvents.Add(customEvent);

        SaveAllCustomEvents(allSavedCustomEvents);
    }

    private void SaveAllCustomEvents(List<CustomEvent> allSavedCustomEvents)
    {
        List<SerializableCustomEvent> allSerializableCustomEvent =
            allSavedCustomEvents.Select(customEvent => new SerializableCustomEvent(customEvent)).ToList();

        string allCustomEventsJSONString = JsonConvert.SerializeObject(allSerializableCustomEvent, _jsonSerializerSettings);

        File.WriteAllText(CustomEventsFilePath, allCustomEventsJSONString);
    }

    public List<CustomEvent> GetAllSavedCustomEvents()
    {
        if (IsFileExist(CustomEventsFilePath) == false)
            SaveAllCustomEvents(new());

        string allCustomEventsJSONString = File.ReadAllText(CustomEventsFilePath);

        List<CustomEvent> allSavedCustomEvents = JsonConvert
            .DeserializeObject<List<SerializableCustomEvent>>(allCustomEventsJSONString, _jsonSerializerSettings)
            .Select(serializableCustomEvent => serializableCustomEvent.GetCustomEvent()).ToList();

        return allSavedCustomEvents;
    }

    public int GetSavedCustomEventsCount()
    {
        List<CustomEvent> allSavedCustomEvents = GetAllSavedCustomEvents();

        return allSavedCustomEvents.Count;
    }

    #endregion

    #region Meteo Conditions

    private readonly MeteoConditions _defaultMeteoConditions = new MeteoConditions(25, 760);

    public void SaveMeteoConditions(MeteoConditions meteoConditions)
    {
        string meteoConditionsJSONString = JsonConvert.SerializeObject(meteoConditions, _jsonSerializerSettings);

        File.WriteAllText(MeteoConditionsFilePath, meteoConditionsJSONString);
    }

    public MeteoConditions GetSavedMeteoConditions()
    {
        if (IsFileExist(MeteoConditionsFilePath) == false)
            SaveMeteoConditions(_defaultMeteoConditions);

        string savedMeteoConditionsJSONString = File.ReadAllText(MeteoConditionsFilePath);

        MeteoConditions savedMeteoConditions =
            JsonConvert.DeserializeObject<MeteoConditions>(savedMeteoConditionsJSONString, _jsonSerializerSettings);

        return savedMeteoConditions;
    }

    #endregion

    #region PreSettings

    public void SavePreSetting(SavedPreSetting savedPreSetting)
    {
        List<SavedPreSetting> allSavedPreSettings = GetAllSavedPreSettings();

        int savedPreSettingsIndex = allSavedPreSettings.FindIndex(preSetting =>
            preSetting.FiringMachineNumber == savedPreSetting.FiringMachineNumber && preSetting.PreSettingNumber == savedPreSetting.PreSettingNumber);

        if (savedPreSettingsIndex >= 0)
            allSavedPreSettings[savedPreSettingsIndex] = savedPreSetting;
        else
            allSavedPreSettings.Add(savedPreSetting);

        SaveAllPreSettings(allSavedPreSettings);
    }

    private void SaveAllPreSettings(List<SavedPreSetting> allSavedPreSettings)
    {
        List<SerializableSavedPreSetting> allSerializablePreSettings =
            allSavedPreSettings.Select(savedPreSetting => new SerializableSavedPreSetting(savedPreSetting)).ToList();

        string allSavedPreSettingJSONString = JsonConvert.SerializeObject(allSerializablePreSettings, _jsonSerializerSettings);

        File.WriteAllText(PreSettingsFilePath, allSavedPreSettingJSONString);
    }

    public List<SavedPreSetting> GetAllSavedPreSettings()
    {
        if (IsFileExist(PreSettingsFilePath) == false)
            SaveAllPreSettings(new());

        string allSavedPreSettingsJSONString = File.ReadAllText(PreSettingsFilePath);

        List<SavedPreSetting> allSavedPreSettings =
            JsonConvert.DeserializeObject<List<SerializableSavedPreSetting>>(allSavedPreSettingsJSONString, _jsonSerializerSettings)
                .Where(serializablePreSetting => serializablePreSetting.PreSettingNumber >= 0)
                .Select(serializablePreSetting => serializablePreSetting.GetSavedPreSetting()).ToList();

        return allSavedPreSettings;
    }

    #endregion

    #region Ammo Types

    public void SaveFiringMachineAmmoTypes(List<FiringMachineAmmoTypes> allFiringMachineAmmoTypes)
    {
        List<SerializableFiringMachineAmmoTypes> allSerializableFiringMachineAmmoTypes = allFiringMachineAmmoTypes
            .Select(firingMachineAmmo => new SerializableFiringMachineAmmoTypes(firingMachineAmmo)).ToList();

        string allFiringMachinesAmmoTypesJSONString = JsonConvert.SerializeObject(allSerializableFiringMachineAmmoTypes, _jsonSerializerSettings);

        File.WriteAllText(FiringMachineAmmoTypesFilePath, allFiringMachinesAmmoTypesJSONString);
    }

    public List<FiringMachineAmmoTypes> GetAllSavedFiringMachineAmmoTypes()
    {
        if (IsFileExist(FiringMachineAmmoTypesFilePath) == false)
            SaveFiringMachineAmmoTypes(new());

        string allSavedFiringMachinesAmmoTypesJSONString = File.ReadAllText(FiringMachineAmmoTypesFilePath);

        List<FiringMachineAmmoTypes> allSavedPreSettings =
            JsonConvert
                .DeserializeObject<List<SerializableFiringMachineAmmoTypes>>(allSavedFiringMachinesAmmoTypesJSONString, _jsonSerializerSettings)
                .Select(firingMachineAmmoType => firingMachineAmmoType.GetFiringMachineAmmoTypes()).ToList();

        return allSavedPreSettings;
    }

    #endregion

    #region Operation Presets

    public void SaveOperationPreset(int overrideIndex, ReadonlyOperationData operationData, string operationName)
    {
        List<SavedOperationData> allSavedOperationPresets = GetAllSavedOperationPresets();

        SavedOperationData savedOperation = new SavedOperationData(overrideIndex, operationData, operationName);

        int savedOperationIndex = allSavedOperationPresets.FindIndex(operationPreset =>
            operationPreset.OperationIndex == overrideIndex);

        if (overrideIndex >= 0)
            if (allSavedOperationPresets.Count > savedOperationIndex)
                allSavedOperationPresets[savedOperationIndex] = savedOperation;
            else
                allSavedOperationPresets.Add(savedOperation);

        SaveAllOperationPresets(allSavedOperationPresets);
    }

    private void SaveAllOperationPresets(List<SavedOperationData> allSavedOperationPresets)
    {
        List<SerializableSavedOperationData> allSerializableOperationPresets =
            allSavedOperationPresets.Select(operationPreset => new SerializableSavedOperationData(operationPreset)).ToList();

        string allSavedOperationsJSONString = JsonConvert.SerializeObject(allSerializableOperationPresets, _jsonSerializerSettings);

        File.WriteAllText(OperationPresetsFilePath, allSavedOperationsJSONString);
    }

    public void SaveOperationPreset(ReadonlyOperationData operationData, string operationName, out int operationIndex)
    {
        List<SavedOperationData> allSavedOperationPresets = GetAllSavedOperationPresets();

        operationIndex = allSavedOperationPresets.Count == 0 ? 0 : allSavedOperationPresets.Max(operationPreset => operationPreset.OperationIndex);
        operationIndex += 1;

        SavedOperationData savedOperationData = new(operationIndex, operationData, operationName);
        allSavedOperationPresets.Add(savedOperationData);

        SaveAllOperationPresets(allSavedOperationPresets);
    }

    public void RemoveOperationPreset(int operationIndex)
    {
        List<SavedOperationData> allSavedOperationPresets = GetAllSavedOperationPresets();

        SavedOperationData removingOperation = allSavedOperationPresets.Find(operationPreset => operationPreset.OperationIndex == operationIndex);

        if (removingOperation == null)
            return;

        allSavedOperationPresets.Remove(removingOperation);

        SaveAllOperationPresets(allSavedOperationPresets);
    }

    public List<SavedOperationData> GetAllSavedOperationPresets()
    {
        if (IsFileExist(OperationPresetsFilePath) == false)
            SaveAllOperationPresets(new());

        string allSavedOperationPresetsJSONString = File.ReadAllText(OperationPresetsFilePath);

        List<SavedOperationData> allSavedOperationPresets =
            JsonConvert.DeserializeObject<List<SerializableSavedOperationData>>(allSavedOperationPresetsJSONString, _jsonSerializerSettings)
                .Where(operationPreset => operationPreset.OperationIndex >= 0).Select(operationPreset => operationPreset.GetOperationData()).ToList();

        return allSavedOperationPresets;
    }

    #endregion

    #region App Settings

    private readonly AppSettingsData _defaultAppSettings = new()
    {
        ClientType = ClientType.Game,
        WindowType = WindowType.Fullscreen,
        NetcodeIP = "127.0.0.1",
        TCPIP = "10.1.4.54",
        TCPPort = 12345
    };

    public void SaveAppSettings(AppSettingsData appSettingsData)
    {
        string appSettingsJSONString = JsonConvert.SerializeObject(appSettingsData, _jsonSerializerSettings);

        File.WriteAllText(AppSettingsFilePath, appSettingsJSONString);
    }

    public AppSettingsData GetSavedAppSettings()
    {
        if (IsFileExist(AppSettingsFilePath) == false)
            SaveAppSettings(_defaultAppSettings);

        string appSettingsJSONString = File.ReadAllText(AppSettingsFilePath);
        AppSettingsData savedAppSettings = JsonConvert.DeserializeObject<AppSettingsData>(appSettingsJSONString, _jsonSerializerSettings);

        return savedAppSettings;
    }

    #endregion

    #region DateTime Offset

    public void SaveCurrentDateTimeOffset(TimeSpan currentOffset)
    {
        string dateTimeOffsetJSONString = JsonConvert.SerializeObject(currentOffset, _jsonSerializerSettings);

        File.WriteAllText(DateTimeOffsetFilePath, dateTimeOffsetJSONString);
    }

    public TimeSpan GetSavedDateTimeOffset()
    {
        if (IsFileExist(DateTimeOffsetFilePath) == false)
            SaveCurrentDateTimeOffset(new TimeSpan());

        string dateTimeOffsetJSONString = File.ReadAllText(DateTimeOffsetFilePath);
        TimeSpan dateTimeOffset = JsonConvert.DeserializeObject<TimeSpan>(dateTimeOffsetJSONString, _jsonSerializerSettings);

        return dateTimeOffset;
    }

    #endregion

    #region Login

    private readonly List<LoginInfo> _defaultLogins = new() { new LoginInfo(0, 0.ToString()) };

    public void SaveLoginInfo(LoginInfo loginInfo)
    {
        List<LoginInfo> allSavedPreSettings = GetAllSavedLoginInfos();

        int savedLoginInfoIndex = allSavedPreSettings.FindIndex(login => login.Login == loginInfo.Login);

        if (savedLoginInfoIndex >= 0)
            allSavedPreSettings[savedLoginInfoIndex] = loginInfo;
        else
            allSavedPreSettings.Add(loginInfo);

        SaveAllLoginsInfo(allSavedPreSettings);
    }

    private void SaveAllLoginsInfo(List<LoginInfo> allLogins)
    {
        List<SerializableLoginInfo> allSerializableLogins = allLogins.Select(login => new SerializableLoginInfo(login)).ToList();

        string allSavedLoginInfoJSONString = JsonConvert.SerializeObject(allSerializableLogins, _jsonSerializerSettings);

        File.WriteAllText(LoginsFilePath, allSavedLoginInfoJSONString);
    }

    public List<LoginInfo> GetAllSavedLoginInfos()
    {
        if (IsFileExist(LoginsFilePath) == false)
            SaveAllLoginsInfo(_defaultLogins);

        string allLoginsInfoJSONString = File.ReadAllText(LoginsFilePath);

        List<SerializableLoginInfo> allLoginsInfo =
            JsonConvert.DeserializeObject<List<SerializableLoginInfo>>(allLoginsInfoJSONString, _jsonSerializerSettings);

        List<LoginInfo> newww = allLoginsInfo.Select(login => login.GetLoginInfo()).ToList();

        return newww;
    }

    #endregion

    private bool IsFileExist(string filePath)
    {
        return File.Exists(filePath);
    }
}