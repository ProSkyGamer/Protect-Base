#region

using System;
using Unity.Netcode;
using Zenject;

#endregion

public class TCPSceneResetManager : NetworkBehaviour, IInitializable, IDisposable
{
    #region Events

    public event Action SceneReset;

    #endregion

    #region Variables & References

    private TCPServerConnector _tcpServerConnector;
    private const string ResetCommand = "resetscene";

    #endregion

    #region Initialization

    [Inject]
    public void Construct(TCPServerConnector tcpServerConnector)
    {
        _tcpServerConnector = tcpServerConnector;
    }

    public void Initialize()
    {
        _tcpServerConnector.TCPMessageReceived += TCPServerConnectorTCPMessageReceived;
    }

    private void TCPServerConnectorTCPMessageReceived(string receivedMessage)
    {
        if (receivedMessage != ResetCommand)
            return;

        ResetCurrentScene();
    }

    #endregion

    #region Reset

    private void ResetCurrentScene()
    {
        if (IsServer == false)
            return;

        ResetCurrentSceneClientRpc();
    }

    [ClientRpc]
    private void ResetCurrentSceneClientRpc()
    {
        SceneReset?.Invoke();
    }

    #endregion

    public void Dispose()
    {
        _tcpServerConnector.TCPMessageReceived -= TCPServerConnectorTCPMessageReceived;
    }
}