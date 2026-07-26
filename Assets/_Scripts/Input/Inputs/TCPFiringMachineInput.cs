#region

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using Zenject;

#endregion

public class TCPFiringMachineInput : IFiringMachineInput, IInitializable, IDisposable
{
    private readonly TCPServerConnector _tcpServerConnector;

    private const string MAIN_MESSAGE_CATEGORY = "keyboard";

    private readonly List<string> _handlingMessageCategories = new()
    {
        "firing",
        "joystick",
        "camera"
    };

    private readonly float _holdButtonInterval = .5f;
    private readonly Dictionary<string, CancellationTokenSource> _currentActiveCancellationTokens = new();

    public TCPFiringMachineInput(TCPServerConnector tcpServerConnector)
    {
        _tcpServerConnector = tcpServerConnector;
    }

    #region Events

    public event Action HideCursor;

    public event Action ShowCursor;

    public event Action<Vector2> Rotation;

    public event Action PowerToggle;

    public event Action ChooseMainFiringBlock;

    public event Action ChooseFirstExplosiveBlock;

    public event Action ChooseSecondExplosiveBlock;

    public event Action FiringModeToggle;

    public event Action Shoot;

    public event Action WarningShot;

    public event Action Reload;

    public event Action RangeRight;

    public event Action RangeLeft;

    public event Action RangeUp;

    public event Action RangeDown;

    public event Action RangeUpDouble;

    public event Action RangeDownDouble;

    public event Action ZoomIn;

    public event Action ZoomOut;

    public event Action FocusPlus;

    public event Action FocusMinus;

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
        string messageCommand = splitTCPMessage[2]; // основной запрос

        string
            messageCommandClarification =
                splitTCPMessage[3]; // уточнение к запросу (Н. при вращении - вращение в какую сторону)

        string
            messageCommandNewStatus =
                splitTCPMessage[4]; // если у запроса есть статус (Н. начать/остановить удержание кнопки)

        switch (messageCommand)
        {
            case "ammo":
                ChooseMainFiringBlock?.Invoke();

                break;

            case "explosive1":
                ChooseFirstExplosiveBlock?.Invoke();

                break;

            case "explosive2":
                ChooseSecondExplosiveBlock?.Invoke();

                break;

            case "warning":
                WarningShot?.Invoke();

                break;

            case "zoom" when messageCommandClarification == "plus":
                ZoomIn?.Invoke();

                break;

            case "zoom" when messageCommandClarification == "minus":
                ZoomOut?.Invoke();

                break;

            case "focus" when messageCommandClarification == "plus":
                FocusPlus?.Invoke();

                break;

            case "focus" when messageCommandClarification == "minus":
                FocusMinus?.Invoke();

                break;

            case "range" when messageCommandClarification == "right" && messageCommandNewStatus == "start":
                if (_currentActiveCancellationTokens[messageCommandClarification] != null) return;

                _currentActiveCancellationTokens[messageCommandClarification] = new CancellationTokenSource();

                StartButtonHoldAsync(()=>RangeRight?.Invoke(), _holdButtonInterval,
                    _currentActiveCancellationTokens[messageCommandClarification].Token).Forget();

                break;

            case "range" when messageCommandClarification == "left" && messageCommandNewStatus == "start":
                if (_currentActiveCancellationTokens[messageCommandClarification] != null) return;

                _currentActiveCancellationTokens[messageCommandClarification] = new CancellationTokenSource();

                StartButtonHoldAsync(()=>RangeLeft?.Invoke(), _holdButtonInterval,
                    _currentActiveCancellationTokens[messageCommandClarification].Token).Forget();

                break;

            case "range" when messageCommandClarification == "up" && messageCommandNewStatus == "start":
                if (_currentActiveCancellationTokens[messageCommandClarification] != null) return;

                _currentActiveCancellationTokens[messageCommandClarification] = new CancellationTokenSource();

                StartButtonHoldAsync(()=>RangeUp?.Invoke(), _holdButtonInterval,
                    _currentActiveCancellationTokens[messageCommandClarification].Token).Forget();

                break;

            case "range" when messageCommandClarification == "up" && messageCommandNewStatus == "double":
                RangeUpDouble?.Invoke();

                break;

            case "range" when messageCommandClarification == "down" && messageCommandNewStatus == "start":
                if (_currentActiveCancellationTokens[messageCommandClarification] != null) return;

                _currentActiveCancellationTokens[messageCommandClarification] = new CancellationTokenSource();

                StartButtonHoldAsync(()=>RangeDown?.Invoke(), _holdButtonInterval,
                    _currentActiveCancellationTokens[messageCommandClarification].Token).Forget();

                break;

            case "range" when messageCommandClarification == "down" && messageCommandNewStatus == "double":
                RangeDown?.Invoke();

                break;

            case "range" when (messageCommandClarification == "right" || messageCommandClarification == "left" ||
                               messageCommandClarification == "up" || messageCommandClarification == "down") &&
                              messageCommandNewStatus == "stop":
                if (_currentActiveCancellationTokens[messageCommandClarification] != null)
                    _currentActiveCancellationTokens[messageCommandClarification].Cancel();

                break;

            case "unlock" when splitTCPMessage[1] == "unlock":
                FiringModeToggle?.Invoke();

                break;

            case "value" when splitTCPMessage[1] == "joystick":
                int.TryParse(messageCommandClarification, out int xJoystickValue);
                int.TryParse(messageCommandNewStatus, out int yJoystickValue);

                Vector2 joystickInput = new(xJoystickValue, yJoystickValue);

                if (joystickInput == Vector2.zero)
                {
                    if (_currentActiveCancellationTokens[splitTCPMessage[1]] != null)
                    {
                        _currentActiveCancellationTokens[splitTCPMessage[1]].Cancel();
                        _currentActiveCancellationTokens[splitTCPMessage[1]] = null;
                        ShowCursor?.Invoke();
                    }
                }
                else
                {
                    if (_currentActiveCancellationTokens[splitTCPMessage[1]] == null)
                    {
                        _currentActiveCancellationTokens[splitTCPMessage[1]] = new CancellationTokenSource();

                        StartJoystickHoldAsync(joystickInput,
                            _currentActiveCancellationTokens[splitTCPMessage[1]].Token).Forget();
                    }
                }

                break;

            case "fire":
                Shoot?.Invoke();

                break;
        }
    }

    private async UniTaskVoid StartJoystickHoldAsync(Vector2 joystickInput, CancellationToken cancellationToken)
    {
        HideCursor?.Invoke();

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            Rotation?.Invoke(joystickInput);
            await UniTask.NextFrame();
        }
    }

    private async UniTaskVoid StartButtonHoldAsync(Action activationAction, float activationIntervals,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            activationAction();

            await UniTask.WaitForSeconds(activationIntervals, cancellationToken: cancellationToken);
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