#region

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Unity.Netcode;
using UnityEngine;

#endregion

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    #region Events

    public event EventHandler<OnVFXDestroyedEventArgs> OnVFXDestroyed;

    public class OnVFXDestroyedEventArgs : EventArgs
    {
        public int destroyedVFXIndex;
    }

    #endregion

    #region Enums

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public enum VFXType
    {
        LightningV1_0,
        LightningV1_2,
        LightningV2_0,
        LightningV3_2,
        LightningV3_6,
        LightningV4_2,
        BombExplosion,
        DroneExplosion,
        BigDroneExplosion,
        SmallDroneExplosion,
        DroneSmokeExplosion,
        FiringMachineDroneSmokeExplosion,
        BulletSparks
    }

    #endregion

    #region Created Classes

    [Serializable]
    public class VFXSingle
    {
        public VFXType vfxType;
        public Transform vfxTransform;
    }

    private class CreatedVFXSingle
    {
        public Transform createdVFXTransform;
        public float remainingVFXLifetime;
    }

    #endregion

    #region Variables & References

    [SerializeField] private List<VFXSingle> allVFXSingle;
    private readonly List<CreatedVFXSingle> allCreatedVFX = new();

    #endregion

    #region Initialization

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;
    }

    #endregion

    #region Update

    private void Update()
    {
        for (var i = 0; i < allCreatedVFX.Count; i++)
        {
            var createdVFXSingle = allCreatedVFX[i];
            createdVFXSingle.remainingVFXLifetime -= Time.deltaTime;

            if (createdVFXSingle.remainingVFXLifetime > 0f) continue;

            OnVFXDestroyed?.Invoke(this, new OnVFXDestroyedEventArgs
            {
                destroyedVFXIndex = createdVFXSingle.createdVFXTransform.GetInstanceID()
            });

            Destroy(createdVFXSingle.createdVFXTransform.gameObject);
            allCreatedVFX.RemoveAt(i);
            i--;
        }
    }

    #endregion

    #region VFX

    public void CreateVFX(VFXType vfxType, float vfxLifetime, Vector3 vfxSpawningPosition, Vector3 vfxRotation)
    {
        CreateVFXServerRpc(vfxType, vfxLifetime, vfxSpawningPosition, vfxRotation);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CreateVFXServerRpc(VFXType vfxType, float vfxLifetime, Vector3 vfxSpawningPosition,
        Vector3 vfxRotation)
    {
        CreateVFXClientRpc(vfxType, vfxLifetime, vfxSpawningPosition, vfxRotation);
    }

    [ClientRpc]
    private void CreateVFXClientRpc(VFXType vfxType, float vfxLifetime, Vector3 vfxSpawningPosition,
        Vector3 vfxRotation)
    {
        foreach (var vfxSingle in allVFXSingle)
        {
            if (vfxSingle.vfxType != vfxType) continue;

            var newCreatedVFXTransform = Instantiate(vfxSingle.vfxTransform, vfxSpawningPosition,
                Quaternion.identity, transform);

            newCreatedVFXTransform.rotation = vfxRotation != Vector3.zero
                ? Quaternion.Euler(vfxRotation)
                : vfxSingle.vfxTransform.rotation;

            var newCreatedVFXSingle = new CreatedVFXSingle
            {
                createdVFXTransform = newCreatedVFXTransform,
                remainingVFXLifetime = vfxLifetime
            };

            allCreatedVFX.Add(newCreatedVFXSingle);

            return;
        }

        Debug.LogError($"Prefab for {vfxType} has not been found!");
    }

    public void DeleteVFX(int createdVFXIndex)
    {
        foreach (var createdVFXSingle in allCreatedVFX)
        {
            if (createdVFXSingle.createdVFXTransform.GetInstanceID() != createdVFXIndex) continue;

            OnVFXDestroyed?.Invoke(this, new OnVFXDestroyedEventArgs
            {
                destroyedVFXIndex = createdVFXSingle.createdVFXTransform.GetInstanceID()
            });

            Destroy(createdVFXSingle.createdVFXTransform.gameObject);
            allCreatedVFX.Remove(createdVFXSingle);
            break;
        }
    }


    public void DeleteVFX(List<int> createdVFXIndexes)
    {
        for (var i = 0; i < allCreatedVFX.Count; i++)
        {
            var createdVFXSingle = allCreatedVFX[i];
            if (!createdVFXIndexes.Contains(createdVFXSingle.createdVFXTransform.GetInstanceID())) continue;

            OnVFXDestroyed?.Invoke(this, new OnVFXDestroyedEventArgs
            {
                destroyedVFXIndex = createdVFXSingle.createdVFXTransform.GetInstanceID()
            });

            Destroy(createdVFXSingle.createdVFXTransform.gameObject);
            allCreatedVFX.RemoveAt(i);
            i--;
        }
    }

    #endregion
}