#region

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using Zenject;

#endregion

public class OperationPresetsManager : NetworkBehaviour, IOperationPresetsProvider
{
    #region Events

    public event Action<int, string> OperationPresetAdded;

    public event Action<int> OperationPresetRemoved;

    public event Action<int, string> OperationPresetUpdated;

    #endregion

    #region Variables & References

    private bool _isSavedOperationsReceived;
    private readonly List<SavedOperationData> _requestedSavedOperationsSingle = new();
    private IDataSavingManager _dataSavingManager;

    #endregion

    #region Set

    [Inject]
    public void Construct(IDataSavingManager dataSavingManager)
    {
        _dataSavingManager = dataSavingManager;
    }

    public void AddOperationSingle(ReadonlyOperationData addingOperationSingle, string operationName)
    {
        addingOperationSingle.PackForNetworkTransfer();

        AddOperationSingleServerRpc(addingOperationSingle, operationName);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddOperationSingleServerRpc(ReadonlyOperationData addingOperationSingle,
        string operationName)
    {
        addingOperationSingle.UnpackAfterNetworkTransfer();

        _dataSavingManager.SaveOperationPreset(addingOperationSingle, operationName, out int operationIndex);

        AddOperationSingleClientRpc(operationIndex, operationName);
    }

    [ClientRpc]
    private void AddOperationSingleClientRpc(int operationIndex, string operationName)
    {
        OperationPresetAdded?.Invoke(operationIndex, operationName);
    }

    public void EditOperationSingle(ReadonlyOperationData editedOperationSingle, int editedOperationIndex,
        string operationName)
    {
        editedOperationSingle.PackForNetworkTransfer();

        EditOperationSingleServerRpc(editedOperationSingle, editedOperationIndex, operationName);
    }

    [ServerRpc(RequireOwnership = false)]
    private void EditOperationSingleServerRpc(ReadonlyOperationData editedOperationSingle,
        int editedOperationIndex, string operationName)
    {
        editedOperationSingle.UnpackAfterNetworkTransfer();

        _dataSavingManager.SaveOperationPreset(editedOperationIndex, editedOperationSingle, operationName);

        EditOperationSingleClientRpc(editedOperationIndex, operationName);
    }

    [ClientRpc]
    private void EditOperationSingleClientRpc(int operationIndex, string operationName)
    {
        OperationPresetUpdated?.Invoke(operationIndex, operationName);
    }

    public void RemoveOperationSingle(int removingOperationIndex)
    {
        RemoveOperationSingleServerRpc(removingOperationIndex);
    }

    [ServerRpc(RequireOwnership = false)]
    private void RemoveOperationSingleServerRpc(int removingOperationIndex)
    {
        _dataSavingManager.RemoveOperationPreset(removingOperationIndex);

        RemoveOperationSingleClientRpc(removingOperationIndex);
    }

    [ClientRpc]
    private void RemoveOperationSingleClientRpc(int removingOperationIndex)
    {
        OperationPresetRemoved?.Invoke(removingOperationIndex);
    }

    #endregion

    #region Get

    public SavedOperationData GetOperationSingle(int operationIndex)
    {
        if (IsServer == false)
            return null;

        SavedOperationData savedOperation = _dataSavingManager.GetAllSavedOperationPresets()
            .Find(savedOperationData => savedOperationData.OperationIndex == operationIndex);

        return savedOperation;
    }

    public async UniTask<List<SavedOperationData>> GetCurrentSavedOperationsAsync()
    {
        _requestedSavedOperationsSingle.Clear();
        _isSavedOperationsReceived = false;

        SendAllSavedOperationsServerRpc();

        await UniTask.WaitUntil(() => _isSavedOperationsReceived);

        return _requestedSavedOperationsSingle;
    }

    [ServerRpc(RequireOwnership = false)]
    private void SendAllSavedOperationsServerRpc()
    {
        List<SavedOperationData> allSavedOperations = _dataSavingManager.GetAllSavedOperationPresets();

        //Debug.Log($"[OperationPresetsManager.SendAllSavedOperationsServerRpc Line 139] {allSavedOperations[0].OperationName}");

        SavedOperationData[] allSavedOperationsArray = new SavedOperationData[allSavedOperations.Count];
        allSavedOperations.CopyTo(allSavedOperationsArray);

        foreach (SavedOperationData savedOperationData in allSavedOperationsArray)
        {
            savedOperationData.PackForNetworkTransfer();
        }

        SendAllSavedOperationsClientRpc(allSavedOperationsArray);
    }

    [ClientRpc]
    private void SendAllSavedOperationsClientRpc(SavedOperationData[] savedOperationData)
    {
        _requestedSavedOperationsSingle.Clear();
        _requestedSavedOperationsSingle.AddRange(savedOperationData);

        foreach (SavedOperationData savedOperation in _requestedSavedOperationsSingle)
        {
            savedOperation.UnpackAfterNetworkTransfer();
        }

        _isSavedOperationsReceived = true;
    }

    #endregion
}