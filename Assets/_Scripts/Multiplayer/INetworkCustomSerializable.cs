#region

using Unity.Netcode;

#endregion

public interface INetworkCustomSerializable : INetworkSerializable
{
    public void PackForNetworkTransfer();
    public void UnpackAfterNetworkTransfer();
}