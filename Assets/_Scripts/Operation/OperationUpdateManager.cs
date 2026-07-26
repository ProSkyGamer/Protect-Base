#region

using System;
using Unity.Netcode;
using Zenject;

#endregion

public class OperationUpdateManager : NetworkBehaviour, ILateTickable
{
    #region Events

    public event Action ActiveOperationUpdated;

    #endregion

    #region Variables & References

    private static bool _isUpdateRequested;

    #endregion

    #region Update

    public void LateTick()
    {
        if (IsServer == false)
            return;

        if (_isUpdateRequested)
        {
            _isUpdateRequested = false;

            UpdateActiveOperationsDataClientRpc();
        }
    }

    [ClientRpc]
    private void UpdateActiveOperationsDataClientRpc()
    {
        ActiveOperationUpdated?.Invoke();
    }

    #endregion

    #region Request Data Update

    public static void RequestUpdate()
    {
        if (NetworkManager.Singleton.IsServer == false)
            return;

        if (_isUpdateRequested)
            return;

        _isUpdateRequested = true;
    }

    #endregion
}