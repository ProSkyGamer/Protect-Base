#region

using Unity.Netcode.Transports.UTP;
using UnityEngine;

#endregion

public enum WindowType
{
    Fullscreen,
    BorderlessWindow,
    MaximizedWindow,
    Window
}

public class AppSettingsData
{
    public ClientType ClientType;
    public WindowType WindowType;
    public string NetcodeIP;
    public string TCPIP;
    public int TCPPort;
}

public class AppSettingsManager
{
    private readonly IDataSavingManager _dataSavingManager;

    #region Initialization

    public AppSettingsManager(UnityTransport unityTransport, TCPServerConnector tcpServerConnector,
        ClientTypeManager clientTypeManager, IDataSavingManager dataSavingManager)
    {
        _dataSavingManager = dataSavingManager;

        AppSettingsData appSettingsData = dataSavingManager.GetSavedAppSettings();

        clientTypeManager.SetClientType(appSettingsData.ClientType);
        unityTransport.ConnectionData.Address = appSettingsData.NetcodeIP;
        tcpServerConnector.SetConnectionData(appSettingsData.TCPIP, appSettingsData.TCPPort);
    }

    #endregion

    #region Save

    public void SaveData(ClientType clientType, WindowType windowType, string netcodeIP, string tcpIP, int tcpPort)
    {
        AppSettingsData appSettingsData = new()
        {
            ClientType = clientType,
            WindowType = windowType,
            NetcodeIP = netcodeIP,
            TCPIP = tcpIP,
            TCPPort = tcpPort
        };

        Debug.Log(
            $"{appSettingsData.ClientType} {appSettingsData.NetcodeIP} {appSettingsData.TCPIP} {appSettingsData.TCPPort}");

        _dataSavingManager.SaveAppSettings(appSettingsData);
    }

    #endregion
}