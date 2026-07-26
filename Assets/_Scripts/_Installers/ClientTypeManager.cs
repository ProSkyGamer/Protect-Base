#region

using Zenject;

#endregion

public class ClientTypeManager : IInitializable
{
    #region Variables & References

    public static ClientType CurrentClientType { get; private set; }

    private bool _isClientTypeSet;

    #endregion

    #region Initialization

    public void SetClientType(ClientType clientType)
    {
        if (_isClientTypeSet) return;

        CurrentClientType = clientType;
        _isClientTypeSet = true;
    }

    public void Initialize()
    {
        /*if (CurrentClientType is ClientType.Game)
            NetworkManager.Singleton.StartHost();
        else
            NetworkManager.Singleton.StartClient();*/
    }

    #endregion
}