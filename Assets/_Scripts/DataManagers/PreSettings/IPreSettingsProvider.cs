public interface IPreSettingsProvider
{
    public PreSettingSingle GetPreSettingSingle(int firingMachineNumber, int firingMachinePreSettingNumber);

    public int PreSettingMaxIndex { get; }
    public int PreSettingMinIndex { get; }
}