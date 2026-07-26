#region

using Unity.Netcode;
using UnityEngine;

#endregion

public class PreSettingSingle : INetworkCustomSerializable
{
    public Vector3 PreSettingEulerAngles => _preSettingEulerAngles;
    private Vector3 _preSettingEulerAngles;
    public int PreSettingZoom => _preSettingZoom;
    private int _preSettingZoom;

    public PreSettingSingle()
    {
    }

    public PreSettingSingle(Vector3 preSettingEulerAngles, int preSettingZoom)
    {
        _preSettingEulerAngles = preSettingEulerAngles;
        _preSettingZoom = preSettingZoom;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _preSettingEulerAngles);
        serializer.SerializeValue(ref _preSettingZoom);
    }

    public void PackForNetworkTransfer()
    {
    }

    public void UnpackAfterNetworkTransfer()
    {
    }
}

public class SavedPreSetting
{
    public int FiringMachineNumber { get; }
    public int PreSettingNumber { get; }
    public PreSettingSingle PreSettingSingle { get; }

    public SavedPreSetting(int firingMachineNumber, int preSettingNumber, PreSettingSingle preSettingSingle)
    {
        FiringMachineNumber = firingMachineNumber;
        PreSettingNumber = preSettingNumber;
        PreSettingSingle = preSettingSingle;
    }
}