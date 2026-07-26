public class AlarmSingle
{
    public int DeviceNumber { get; }

    public int AlarmNumber { get; }

    public int FiringMachineNumber { get; }

    public int PreSettingNumber { get; }

    public FiringMachineEnableType FiringMachineEnableType { get; }

    public AlarmType AlarmType { get; }

    public AlarmSingle(int deviceNumber, int alarmNumber, int firingMachineNumber,
        int preSettingNumber, FiringMachineEnableType firingMachineEnableType, AlarmType alarmType)
    {
        DeviceNumber = deviceNumber;
        AlarmNumber = alarmNumber;
        FiringMachineNumber = firingMachineNumber;
        PreSettingNumber = preSettingNumber;
        FiringMachineEnableType = firingMachineEnableType;
        AlarmType = alarmType;
    }
}

public enum FiringMachineEnableType
{
    Hands,
    Auto
}

public enum AlarmType
{
    Open,
    Closed
}