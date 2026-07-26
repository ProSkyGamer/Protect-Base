#region

using UnityEngine;
using UnityEngine.AI;

#endregion

public interface IPathShortcut
{
    public void StartUnlockingShortcut();
    public void StopUnlockingShortcut();

    public bool IsCurrentlyBeingUnlocked { get; }

    public bool IsUnlocked { get; }

    public Vector3 UnlockingPosition { get; }

    public Vector3 WaitingPosition { get; }

    public OffMeshLink OffMeshLink { get; }

    public float ShortcutUnlockTimeLeft { get; }
}