#region

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;

#endregion

public class TCPTaskHandler : IInitializable, IDisposable
{
    private readonly TCPServerConnector _tcpServerConnector;

    private readonly List<string> _handlingMessageCategories = new()
    {
        "task"
    };

    private bool _isSendingOperationStatusMessage = true;
    private bool _isSendCurrentOperations;
    private readonly OperationsManager _operationsManager;
    private readonly OperationPresetsManager _operationPresetsManager;

    public TCPTaskHandler(TCPServerConnector tcpServerConnector, OperationsManager operationsManager,
        OperationPresetsManager operationPresetsManager)
    {
        _tcpServerConnector = tcpServerConnector;
        _operationsManager = operationsManager;
        _operationPresetsManager = operationPresetsManager;
    }

    public void Initialize()
    {
        _tcpServerConnector.TCPMessageReceived += TCPServerConnectorTCPMessageReceived;

        _operationPresetsManager.OperationPresetAdded += OperationPresetsManagerOperationPresetAdded;
        _operationPresetsManager.OperationPresetUpdated += OperationPresetsManagerOperationPresetUpdated;
        _operationPresetsManager.OperationPresetRemoved += OperationPresetsManagerOperationPresetRemoved;

        _tcpServerConnector.TCPServerConnected += TCPServerConnectorTCPServerConnected;

        _operationsManager.OperationStarted += OperationsManagerOperationStarted;
        _operationsManager.OperationStopped += OperationsManagerOperationStopped;
    }

    private void TCPServerConnectorTCPServerConnected()
    {
        if (!_isSendCurrentOperations)
        {
            _isSendCurrentOperations = true;
            SendAllOperationPresetsAsync().Forget();
        }
    }

    private void OperationsManagerOperationStarted(ReadonlyOperationData _)
    {
        if (!_isSendingOperationStatusMessage)
        {
            _isSendingOperationStatusMessage = true;

            return;
        }

        _tcpServerConnector.SendMessageByConnection("task_started");
    }

    private void OperationsManagerOperationStopped()
    {
        if (!_isSendingOperationStatusMessage)
        {
            _isSendingOperationStatusMessage = true;

            return;
        }

        _tcpServerConnector.SendMessageByConnection("task_stopped");
    }

    private void OperationPresetsManagerOperationPresetRemoved(int removedOperationIndex)
    {
        SendRemoveOperationPreset(removedOperationIndex);
    }

    private void OperationPresetsManagerOperationPresetUpdated(int operationIndex, string operationName)
    {
        SendUpdateOperationPreset(operationIndex, operationName);
    }

    private void OperationPresetsManagerOperationPresetAdded(int operationIndex, string operationName)
    {
        SendNewOperationPreset(operationIndex, operationName);
    }

    private void TCPServerConnectorTCPMessageReceived(string receivedMessage)
    {
        string[] splitTCPMessage = receivedMessage.Split('_');

        if (!IsHandlingThisMessage(splitTCPMessage)) return;

        HandleTCPMessage(splitTCPMessage);
    }

    private void HandleTCPMessage(string[] splitTCPMessage)
    {
        switch (splitTCPMessage[0])
        {
            case "task" when splitTCPMessage[1] == "start":
                if (_operationsManager.IsOperationActive) return;

                string startingTaskMessage = splitTCPMessage[2];
                startingTaskMessage = startingTaskMessage.Replace("[", "");
                startingTaskMessage = startingTaskMessage.Replace("]", "");

                if (!int.TryParse(startingTaskMessage, out int startingTaskIndex)) return;

                _isSendingOperationStatusMessage = false;

                SavedOperationData startingOperation =
                    _operationPresetsManager.GetOperationSingle(startingTaskIndex);

                _operationsManager.StartOperation(startingOperation.OperationData);

                break;

            case "task" when splitTCPMessage[1] == "stop":
                _isSendingOperationStatusMessage = false;
                _operationsManager.StopOperation();

                break;
        }
    }

    private bool IsHandlingThisMessage(string[] splitTCPMessage)
    {
        if (!_handlingMessageCategories.Contains(splitTCPMessage[0])) return false;

        return true;
    }

    #region Sending Data

    private async UniTaskVoid SendAllOperationPresetsAsync()
    {
        List<SavedOperationData> allOperationsSingle =
            await _operationPresetsManager.GetCurrentSavedOperationsAsync();

        foreach (SavedOperationData savedOperationSingle in allOperationsSingle)
        {
            SendNewOperationPreset(savedOperationSingle.OperationIndex, savedOperationSingle.OperationName);
        }
    }

    private void SendNewOperationPreset(int operationIndex, string operationName)
    {
        string operationMessage = string.Format(GetOperationInfoString(operationIndex, operationName), "add");
        _tcpServerConnector.SendMessageByConnection(operationMessage);
    }

    private void SendUpdateOperationPreset(int operationIndex, string operationName)
    {
        string operationMessage = string.Format(GetOperationInfoString(operationIndex, operationName), "update");
        _tcpServerConnector.SendMessageByConnection(operationMessage);
    }

    private void SendRemoveOperationPreset(int removingOperationIndex)
    {
        string operationMessage = string.Format(GetOperationInfoString(removingOperationIndex, ""), "remove");
        _tcpServerConnector.SendMessageByConnection(operationMessage);
    }

    #endregion

    #region Get

    private string GetOperationInfoString(int operationIndex, string operationName)
    {
        string operationInfoString = $"task_{{0}}_[{operationIndex}]_[{operationName}]";

        return operationInfoString;
    }

    #endregion

    public void Dispose()
    {
        _tcpServerConnector.TCPMessageReceived -= TCPServerConnectorTCPMessageReceived;
        _tcpServerConnector.TCPServerConnected -= TCPServerConnectorTCPServerConnected;

        _operationPresetsManager.OperationPresetAdded -= OperationPresetsManagerOperationPresetAdded;
        _operationPresetsManager.OperationPresetUpdated -= OperationPresetsManagerOperationPresetUpdated;
        _operationPresetsManager.OperationPresetRemoved -= OperationPresetsManagerOperationPresetRemoved;

        _operationsManager.OperationStarted -= OperationsManagerOperationStarted;
        _operationsManager.OperationStopped -= OperationsManagerOperationStopped;
    }
}