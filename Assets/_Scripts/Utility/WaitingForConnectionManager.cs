#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using Zenject;

#endregion

public class WaitingForConnectionManager : NetworkBehaviour, IInitializable, IDisposable
{
    #region Events

    public static event Action ClientConnected;

    public static event Action ClientDisconnected;

    #endregion

    #region Variables & References

    [SerializeField] private float _restartClientAfterDisconnectTime = 12.5f;
    private CancellationTokenSource _restartCancellationToken = new();

    private bool _isServerConnected;

    #endregion

    #region Initialization

    public void Initialize()
    {
        NetworkManager.Singleton.OnConnectionEvent += NetworkManager_OnConnectionEvent;
    }

    private void NetworkManager_OnConnectionEvent(NetworkManager networkManager,
        ConnectionEventData connectionEventData)
    {
        if (connectionEventData.ClientId != networkManager.LocalClientId) return;

        if (connectionEventData.EventType == ConnectionEvent.ClientConnected)
        {
            _restartCancellationToken.Cancel();
            _restartCancellationToken = new();

            _isServerConnected = true;
            ClientConnected?.Invoke();
        }
        else if (connectionEventData.EventType == ConnectionEvent.ClientDisconnected)
        {
            if (!_isServerConnected)
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().name);

                return;
            }

            StartClientRestartCountdown().Forget();

            ClientDisconnected?.Invoke();
        }
    }

    private async UniTask StartClientRestartCountdown()
    {
        await UniTask.WaitForSeconds(_restartClientAfterDisconnectTime);

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    #endregion

    public void Dispose()
    {
        _restartCancellationToken.Cancel();

        ClientConnected = null;
        ClientDisconnected = null;
    }
}