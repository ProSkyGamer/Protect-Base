#region

using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

public class CreatedMarkerSingle
{
    public CreatedMarkerSingle(Transform worldObject, Vector2 offset, MarkerType markerType)
    {
        WorldObject = worldObject;
        MapMarkerOffset = offset;
        MarkerType = markerType;
    }

    public Transform WorldObject { get; }
    public Vector2 MapMarkerOffset { get; }
    public MarkerType MarkerType { get; }
}

public class MarkersManager : NetworkBehaviour, IInitializable, IDisposable
{
    #region Events

    public event Action<Vector2, MarkerType, Transform> MarkerAdded;
    public event Action<Transform> MarkerRemoved;

    #endregion

    #region Variables & References

    private readonly List<CreatedMarkerSingle> _allCreatedMarkers = new();
    private OperationTerritoryManager _operationTerritoryManager;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(OperationTerritoryManager operationTerritoryManager)
    {
        _operationTerritoryManager = operationTerritoryManager;
    }

    public void Initialize()
    {
        OperationMapObjectSingle.MarkerSpawned += OperationMapObjectSingle_OnMarkerSpawned;
    }

    private void OperationMapObjectSingle_OnMarkerSpawned(Transform mapObjectTransform, Vector3 offset,
        MarkerType markerType)
    {
        AddMapMarker(mapObjectTransform, offset, markerType);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (!IsServer) return;

        NetworkManager.Singleton.OnConnectionEvent += NetworkManager_OnConnectionEvent;
    }

    private void NetworkManager_OnConnectionEvent(NetworkManager networkManager,
        ConnectionEventData connectionEventData)
    {
        if (!IsServer) return;
        if (connectionEventData.EventType is not ConnectionEvent.ClientConnected) return;

        foreach (CreatedMarkerSingle createdMarkerSingle in _allCreatedMarkers)
        {
            AddMapMarker(createdMarkerSingle.WorldObject, createdMarkerSingle.MapMarkerOffset,
                createdMarkerSingle.MarkerType);
        }
    }

    #endregion

    #region Markers

    public void AddMapMarker(Transform worldObject, Vector2 mapMarkerOffset,
        MarkerType markerType)
    {
        if (!IsServer) return;

        NetworkObject networkWorldObject = worldObject.GetComponent<NetworkObject>();

        if (networkWorldObject == null) return;

        AddMapMarkerClientRpc(networkWorldObject, mapMarkerOffset, markerType);
    }

    [ClientRpc]
    private void AddMapMarkerClientRpc(NetworkObjectReference networkWorldObjectReference, Vector2 mapMarkerOffset,
        MarkerType markerType)
    {
        if (ClientTypeManager.CurrentClientType is not (ClientType.OperationSettings
            or ClientType.Game)) return;

        networkWorldObjectReference.TryGet(out NetworkObject networkWorldObject);
        Transform worldObject = networkWorldObject.transform;

        Vector2 mapMarkerPosition = _operationTerritoryManager.GetMapPointFromWorldPoint(worldObject.position) +
                                    mapMarkerOffset;

        foreach (CreatedMarkerSingle createdMarkerSingle in _allCreatedMarkers)
        {
            if (createdMarkerSingle.WorldObject == worldObject)
                return;
        }

        _allCreatedMarkers.Add(new CreatedMarkerSingle(worldObject, mapMarkerOffset, markerType));

        MarkerAdded?.Invoke(mapMarkerPosition, markerType, worldObject);
    }

    public void RemoveMarker(Transform worldObject)
    {
        if (!IsServer) return;

        NetworkObject networkWorldObject = worldObject.GetComponent<NetworkObject>();

        if (networkWorldObject == null) return;

        RemoveMarkerClientRpc(networkWorldObject);
    }

    [ClientRpc]
    private void RemoveMarkerClientRpc(NetworkObjectReference networkWorldObjectReference)
    {
        if (ClientTypeManager.CurrentClientType is not (ClientType.OperationSettings
            or ClientType.Game)) return;

        networkWorldObjectReference.TryGet(out NetworkObject networkWorldObject);
        Transform worldObject = networkWorldObject.transform;

        foreach (CreatedMarkerSingle createdMarkerSingle in _allCreatedMarkers)
        {
            if (createdMarkerSingle.WorldObject != worldObject) continue;

            _allCreatedMarkers.Remove(createdMarkerSingle);

            break;
        }

        MarkerRemoved?.Invoke(worldObject);
    }

    #endregion

    public void Dispose()
    {
        OperationMapObjectSingle.MarkerSpawned -= OperationMapObjectSingle_OnMarkerSpawned;
    }
}