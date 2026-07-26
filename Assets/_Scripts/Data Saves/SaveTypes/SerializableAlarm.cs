public class SerializableAlarm
{
    public int DeviceNumber;
    public int AlarmNumber;
    public int FiringMachineNumber;
    public int PreSettingNumber;
    public FiringMachineEnableType FiringMachineEnableType;
    public AlarmType AlarmType;

    public SerializableAlarm()
    {
    }

    public SerializableAlarm(AlarmSingle alarmSingle)
    {
        DeviceNumber = alarmSingle.DeviceNumber;
        AlarmNumber = alarmSingle.AlarmNumber;
        FiringMachineNumber = alarmSingle.FiringMachineNumber;
        PreSettingNumber = alarmSingle.PreSettingNumber;
        FiringMachineEnableType = alarmSingle.FiringMachineEnableType;
        AlarmType = alarmSingle.AlarmType;
    }

    public AlarmSingle GetAlarmSingle()
    {
        AlarmSingle alarmSingle =
            new AlarmSingle(DeviceNumber, AlarmNumber, FiringMachineNumber, PreSettingNumber, FiringMachineEnableType, AlarmType);

        return alarmSingle;
    }
}