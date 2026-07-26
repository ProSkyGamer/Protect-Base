#region

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

public class FiringMachinesSpawner : NetworkBehaviour
{
    #region Events

    public event Action<List<FiringMachineController>> FiringMachinesSpawned;

    #endregion

    #region Variables & References

    [SerializeField] private List<Transform> _allFiringMachineTransforms;

    private FiringMachinesFactory _firingMachinesFactory;
    private DynamicInjector _dynamicInjector;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(FiringMachinesFactory firingMachinesFactory, DynamicInjector dynamicInjector)
    {
        _firingMachinesFactory = firingMachinesFactory;
        _dynamicInjector = dynamicInjector;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer == false)
            return;

        SpawnAllFiringMachines();
    }

    #endregion

    #region Spawn

    private void SpawnAllFiringMachines()
    {
        if (IsServer == false)
            return;

        List<NetworkObject> spawnedFiringMachinesNetworkObjects = new();

        for (int i = 0; i < _allFiringMachineTransforms.Count; i++)
        {
            FiringMachineController createdFiringMachine =
                _firingMachinesFactory.Create(_allFiringMachineTransforms[i], i);

            NetworkObject firingMachineNetworkObject = createdFiringMachine.GetComponent<NetworkObject>();
            firingMachineNetworkObject.Spawn();

            spawnedFiringMachinesNetworkObjects.Add(firingMachineNetworkObject);

            _dynamicInjector.InjectAllInterfacesFrom(createdFiringMachine);
        }

        NetworkObjectReference[] spawnedFiringMachinesNetworkObjectsArray =
            new NetworkObjectReference[spawnedFiringMachinesNetworkObjects.Count];

        for (int i = 0; i < spawnedFiringMachinesNetworkObjectsArray.Length; i++)
            spawnedFiringMachinesNetworkObjectsArray[i] = spawnedFiringMachinesNetworkObjects[i];

        NotifyAllFiringMachinesSpawnedClientRpc(spawnedFiringMachinesNetworkObjectsArray);
    }

    [ClientRpc]
    private void NotifyAllFiringMachinesSpawnedClientRpc(NetworkObjectReference[] networkObjectReferences)
    {
        List<FiringMachineController> allSpawnedFiringMachines =
            networkObjectReferences.Select(networkObjectReference =>
                networkObjectReference.TryGet(out NetworkObject networkObject)
                    ? networkObject.GetComponent<FiringMachineController>()
                    : null).Where(firingMachineController => firingMachineController != null).ToList();

        FiringMachinesSpawned?.Invoke(allSpawnedFiringMachines);
    }

    #endregion
}