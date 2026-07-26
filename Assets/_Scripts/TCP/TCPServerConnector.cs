#region

using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

public class TCPServerConnector : IInitializable, ITickable, IDisposable
{
    #region Events & Event Args

    public event Action TCPServerConnected;

    public event Action<string> TCPMessageReceived;

    #endregion

    #region Variables & References

    private TcpClient _socketConnection;
    private Thread _clientReceiveThread;
    private string _serverMessage = "";
    private string _connectingIP;
    private int _connectingPort;
    private bool _isHasMessageReceived;
    private bool _isConnected;
    private float _reconnectTimer;
    private readonly float _reconnectTime = 2.5f;
    private bool _isSendingConnectionEvent = true;

    #endregion

    #region Initialization [CONNECT]

    public void Initialize()
    {
        if (_isConnected == false)
            ConnectToTcpServer();
    }

    #endregion

    #region Update [RECONNECT; LISTEN FOR MESSAGES]

    public void Tick()
    {
        if (NetworkManager.Singleton.IsConnectedClient == false)
            return;

        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        if (_isConnected && (_clientReceiveThread == null || _clientReceiveThread.IsAlive == false))
        {
            _isConnected = false;
            _isSendingConnectionEvent = true;
        }

        if (_isConnected && _clientReceiveThread is { IsAlive: true } && _isSendingConnectionEvent)
        {
            _isSendingConnectionEvent = false;
            TCPServerConnected?.Invoke();
        }

        if (_isConnected == false)
        {
            _reconnectTimer -= Time.deltaTime;

            if (_reconnectTimer <= 0f)
                ConnectToTcpServer();
        }

        if (_isHasMessageReceived)
        {
            _isHasMessageReceived = false;

            TCPMessageReceived?.Invoke(_serverMessage);
        }
    }

    #endregion

    #region TCP Methods [CONNECT; LISTEN; SEND]

    private void ConnectToTcpServer()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        _reconnectTimer = _reconnectTime;

        if (_connectingIP == "")
            return;

        try
        {
            _clientReceiveThread = new Thread(ListenForData)
            {
                IsBackground = true
            };

            _clientReceiveThread.Start();
            _isConnected = true;
        }
        catch (Exception e)
        {
            Debug.Log("On client connect exception " + e);
        }
    }

    private void ListenForData()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        try
        {
            _socketConnection = new TcpClient(_connectingIP, _connectingPort);
            byte[] bytes = new byte[1024];

            while (true)
                // Get a stream object for reading 				
            {
                if (!_socketConnection.Connected) break;

                using NetworkStream stream = _socketConnection.GetStream();
                int length;

                // Read incoming stream into byte array. 					
                while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)
                {
                    byte[] incomingData = new byte[length];
                    Array.Copy(bytes, 0, incomingData, 0, length);
                    // Convert byte array to string message. 
                    _serverMessage = Encoding.UTF8.GetString(incomingData);
                    _isHasMessageReceived = true;
                }
            }
        }
        catch (SocketException socketException)
        {
            //Debug.Log("Socket exception: " + socketException);
        }
    }

    public void SendMessageByConnection(string clientMessage)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        if (_socketConnection == null)
            return;

        try
        {
            // Get a stream object for writing. 			
            NetworkStream stream = _socketConnection.GetStream();

            if (stream.CanWrite)
            {
                // Convert string message to byte array.                 
                byte[] clientMessageAsByteArray = Encoding.UTF8.GetBytes(clientMessage);
                // Write byte array to socketConnection stream.                 
                stream.Write(clientMessageAsByteArray, 0, clientMessageAsByteArray.Length);

                Debug.Log($"SEND message: {clientMessage}");
            }
        }
        catch (SocketException socketException)
        {
            Debug.Log("Socket exception: " + socketException);
        }
    }

    #endregion

    #region Set

    public void SetConnectionData(string ipAddress, int port)
    {
        _connectingIP = ipAddress;
        _connectingPort = port;
    }

    #endregion

    public void Dispose()
    {
        TCPMessageReceived = null;

        if (_clientReceiveThread != null)
            _clientReceiveThread.Abort();
    }
}