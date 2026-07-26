#region

using System;
using System.Collections.Generic;
using Zenject;

#endregion

public class TCPSystemInput : ISystemInput, IInitializable, IDisposable
{
    private readonly TCPServerConnector _tcpServerConnector;

    public TCPSystemInput(TCPServerConnector tcpServerConnector)
    {
        _tcpServerConnector = tcpServerConnector;
    }

    private const string MAIN_MESSAGE_CATEGORY = "keyboard";

    private readonly List<string> _handlingMessageCategories = new()
    {
        "system",
        "network",
        "military"
    };

    #region Events

    public event Action DYMNetworkToggle;

    public event Action DutyModeToggle;

    public event Action<int> SwitchActive;

    public event Action<int, bool> PowerToggle;

    public event Action SwitchNextActiveFiringMachine;

    public event Action SwitchPreviousActiveFiringMachine;

    public event Action ChooseSingleShootingType;

    public event Action ChooseMultiShootingType;

    public event Action ProjectorToggle;

    public event Action InfraredToggle;

    #endregion

    public void Initialize()
    {
        _tcpServerConnector.TCPMessageReceived += TCPServerConnectorTCPMessageReceived;
    }

    private void TCPServerConnectorTCPMessageReceived(string receivedMessage)
    {
        string[] splitTCPMessage = receivedMessage.Split('_');

        if (!IsCanHandleMessage(splitTCPMessage)) return;

        HandleTCPMessage(splitTCPMessage);
    }

    private void HandleTCPMessage(string[] splitTCPMessage)
    {
        switch (splitTCPMessage[2])
        {
            case "projector":
                ProjectorToggle?.Invoke();

                break;

            case "multi":
                ChooseMultiShootingType?.Invoke();

                break;

            case "single":
                ChooseSingleShootingType?.Invoke();

                break;

            case "network":
                DYMNetworkToggle?.Invoke();

                break;

            case "military":
                DutyModeToggle?.Invoke();

                break;

            case "thermal":
                InfraredToggle?.Invoke();

                break;

            case "firingmachine" when splitTCPMessage[3] == "activate":
                int.TryParse(splitTCPMessage[4], out int activatingFiringMachine);
                SwitchActive?.Invoke(activatingFiringMachine);

                break;

            case "firingmachine" when splitTCPMessage[3] == "switch":
                int.TryParse(splitTCPMessage[4], out activatingFiringMachine);
                PowerToggle?.Invoke(activatingFiringMachine, false);

                break;
        }
    }

    private bool IsCanHandleMessage(string[] splitTCPMessage)
    {
        if (splitTCPMessage[0] != MAIN_MESSAGE_CATEGORY) return false;
        if (!_handlingMessageCategories.Contains(splitTCPMessage[1])) return false;

        return true;
    }

    public void Dispose()
    {
        _tcpServerConnector.TCPMessageReceived -= TCPServerConnectorTCPMessageReceived;
    }
}