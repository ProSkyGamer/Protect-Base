#region

using System;
using System.Collections.Generic;

#endregion

public interface IDataSavingManager
{
    public void SaveAlarm(AlarmSingle alarmSingle);
    public void ClearSavedAlarms();
    public List<AlarmSingle> GetAllSavedAlarms();

    public void SaveCustomEvent(CustomEvent customEvent, bool isRemovingFirst);
    public List<CustomEvent> GetAllSavedCustomEvents();
    public int GetSavedCustomEventsCount();

    public void SaveMeteoConditions(MeteoConditions meteoConditions);
    public MeteoConditions GetSavedMeteoConditions();

    public void SavePreSetting(SavedPreSetting savedPreSetting);
    public List<SavedPreSetting> GetAllSavedPreSettings();

    public void SaveFiringMachineAmmoTypes(List<FiringMachineAmmoTypes> allFiringMachineAmmoTypes);
    public List<FiringMachineAmmoTypes> GetAllSavedFiringMachineAmmoTypes();

    public void SaveOperationPreset(int overrideOperationIndex, ReadonlyOperationData operationData, string operationName);
    public void SaveOperationPreset(ReadonlyOperationData operationData, string operationName, out int operationIndex);
    public void RemoveOperationPreset(int operationIndex);
    public List<SavedOperationData> GetAllSavedOperationPresets();

    public void SaveAppSettings(AppSettingsData appSettingsData);
    public AppSettingsData GetSavedAppSettings();

    // date time offset & passwords
    public void SaveCurrentDateTimeOffset(TimeSpan currentOffset);
    public TimeSpan GetSavedDateTimeOffset();

    public void SaveLoginInfo(LoginInfo loginInfo);
    public List<LoginInfo> GetAllSavedLoginInfos();
}