public interface IAlarmSaver
{
    public void AddAlarmSingle(int deviceNumber, int alarmNumber, int firingMachineNumber, int preSettingNumber,
        FiringMachineEnableType firingMachineEnableType, AlarmType alarmType);

    public void RemoveAllAlarms();
}