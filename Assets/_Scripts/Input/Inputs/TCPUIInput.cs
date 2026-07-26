#region

using System;
using System.Collections.Generic;
using Zenject;

#endregion

public class TCPUIInput : IUIInput, IInitializable, IDisposable
{
    private readonly TCPServerConnector _tcpServerConnector;
    private const string MAIN_MESSAGE_CATEGORY = "keyboard";

    private readonly List<string> _handlingMessageCategories = new()
    {
        "keypad"
    };

    public TCPUIInput(TCPServerConnector tcpServerConnector)
    {
        _tcpServerConnector = tcpServerConnector;
    }

    #region Events

    public event Action Up;

    public event Action Down;

    public event Action Left;

    public event Action Right;

    public event Action Interact;

    public event Action Input0;

    public event Action Input1;

    public event Action Input2;

    public event Action Input3;

    public event Action Input4;

    public event Action Input5;

    public event Action Input6;

    public event Action Input7;

    public event Action Input8;

    public event Action Input9;

    public event Action Backspace;

    public event Action PreSettingStartInput;

    public event Action PreSettingSaveButton;

    public event Action Minus;

    public event Action Clear;

    public event Action LockToggle;

    public event Action Reset;

    #endregion

    public void Initialize()
    {
        _tcpServerConnector.TCPMessageReceived += TCPServerConnectorTCPMessageReceived;
    }

    private void TCPServerConnectorTCPMessageReceived(string receivedMessage)
    {
        string[] splitTCPMessage = receivedMessage.Split('_');

        if (!IsHandlingThisMessage(splitTCPMessage)) return;

        HandleTCPMessage(splitTCPMessage);
    }

    private void HandleTCPMessage(string[] splitTCPMessage)
    {
        switch (splitTCPMessage[2])
        {
            case "0":
                Input0?.Invoke();

                break;

            case "1":
                Input1?.Invoke();

                break;

            case "2":
                Input2?.Invoke();

                break;

            case "3":
                Input3?.Invoke();

                break;

            case "4":
                Input4?.Invoke();

                break;

            case "5":
                Input5?.Invoke();

                break;

            case "6":
                Input6?.Invoke();

                break;

            case "7":
                Input7?.Invoke();

                break;

            case "8":
                Input8?.Invoke();

                break;

            case "9":
                Input9?.Invoke();

                break;

            case "reset":
                Reset?.Invoke();

                break;

            case "backspace":
                Backspace?.Invoke();

                break;

            case "enter":
                Interact?.Invoke();
                PreSettingSaveButton?.Invoke();

                break;

            case "lock":
                LockToggle?.Invoke();

                break;

            case "presetting":
                PreSettingStartInput?.Invoke();

                break;

            case "minus":
                Minus?.Invoke();

                break;
        }
    }

    private bool IsHandlingThisMessage(string[] splitTCPMessage)
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