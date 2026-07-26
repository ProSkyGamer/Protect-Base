#region

using System;
using Unity.Netcode;
using UnityEngine;

#endregion

public class OperationMapObjectSingle : NetworkBehaviour
{
    #region Events

    public static event Action<Transform, Vector3, MarkerType> MarkerSpawned;

    #endregion

    #region Variables & References

    [SerializeField] private MarkerType _markerType;
    [SerializeField] private Vector2 _objectOffset;

    #endregion

    #region Initialization

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer == false)
            return;

        MarkerSpawned?.Invoke(transform, _objectOffset, _markerType);
    }

    #endregion
}