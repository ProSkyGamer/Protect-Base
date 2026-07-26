#region

using UnityEngine;

#endregion

public class SerializableSavedPreSetting
{
    public int FiringMachineNumber;
    public int PreSettingNumber;
    public SerializablePreSetting PreSettingSingle;

    public SerializableSavedPreSetting()
    {
    }

    public SerializableSavedPreSetting(SavedPreSetting savedPreSetting)
    {
        FiringMachineNumber = savedPreSetting.FiringMachineNumber;
        PreSettingNumber = savedPreSetting.PreSettingNumber;
        PreSettingSingle = new(savedPreSetting.PreSettingSingle);
    }

    public SavedPreSetting GetSavedPreSetting()
    {
        SavedPreSetting savedPreSetting = new SavedPreSetting(FiringMachineNumber, PreSettingNumber, PreSettingSingle.GetPreSetting());

        return savedPreSetting;
    }
}

public class SerializablePreSetting
{
    public Vector3 PreSettingEulerAngles { get; }
    public int PreSettingZoom { get; }

    public SerializablePreSetting(PreSettingSingle preSettingSingle)
    {
        PreSettingEulerAngles = preSettingSingle.PreSettingEulerAngles;
        PreSettingZoom = preSettingSingle.PreSettingZoom;
    }

    public PreSettingSingle GetPreSetting()
    {
        PreSettingSingle preSettingSingle = new PreSettingSingle(PreSettingEulerAngles, PreSettingZoom);

        return preSettingSingle;
    }
}