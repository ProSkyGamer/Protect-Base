#region

using System;
using Zenject;

#endregion

public class ClientConnectionObserver : IInitializable, IDisposable
{
    private readonly ClientConnectionUI _clientConnectionUI;
    private bool _isFirstlyConnected;

    public ClientConnectionObserver(ClientConnectionUI clientConnectionUI)
    {
        _clientConnectionUI = clientConnectionUI;
    }

    public void Initialize()
    {
        WaitingForConnectionManager.ClientConnected += WaitingForConnectionManagerClientConnected;
        WaitingForConnectionManager.ClientDisconnected += WaitingForConnectionManagerClientDisconnected;

        _clientConnectionUI.ChangeReconnectPageState(false);
    }

    private void WaitingForConnectionManagerClientDisconnected()
    {
        if (_isFirstlyConnected)
            _clientConnectionUI.ChangeReconnectPageState(true);
    }

    private void WaitingForConnectionManagerClientConnected()
    {
        if (_isFirstlyConnected == false)
        {
            _isFirstlyConnected = true;
            _clientConnectionUI.ChangeConnectingPageState(false);
        }
        else
        {
            _clientConnectionUI.ChangeReconnectPageState(false);
        }
    }

    public void Dispose()
    {
        WaitingForConnectionManager.ClientConnected -= WaitingForConnectionManagerClientConnected;
        WaitingForConnectionManager.ClientDisconnected -= WaitingForConnectionManagerClientDisconnected;
    }
}