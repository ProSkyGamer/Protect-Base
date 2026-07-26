#region

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

#endregion

[Serializable]
public class FenceLayerSingle
{
    public Transform OriginalFenceTransform;
    public Transform UnlockedFenceTransform;
    public Animator FenceAnimator;
}

public class EnemiesShortcutSingle : NetworkBehaviour, IPathShortcut, ISceneResettable
{
    #region Enuns

    private enum FenceAnimationStates
    {
        Idle,
        Falling
    }

    #endregion

    #region Variables & References

    [SerializeField] private OffMeshLink _shortcutOffMeshLink;
    [SerializeField] private int _lockedShortcutOverrideCost = 4;
    [SerializeField] private int _currentlyUnlockingShortcutOverrideCost = 2;
    [SerializeField] private int _unlockedShortcutOverrideCost = 1;
    [SerializeField] private List<FenceLayerSingle> _allFenceLayersSingle = new();
    [SerializeField] private Transform _unlockingPositionTransform;
    [SerializeField] private Transform _waitingPositionTransform;
    [SerializeField] private Transform _explosionVFXPosition;
    [SerializeField] private float _shortcutFullUnlockTime = 7.5f;

    private float _shortcutCurrentUnlockTime;
    private CancellationTokenSource _unlockCancellationToken = new();

    private int _currentFallenFenceIndex;

    private static readonly int State = Animator.StringToHash("State");

    public bool IsCurrentlyBeingUnlocked { get; private set; }

    public bool IsUnlocked => _shortcutCurrentUnlockTime >= _shortcutFullUnlockTime;

    public Vector3 UnlockingPosition => _unlockingPositionTransform.position;

    public Vector3 WaitingPosition => _waitingPositionTransform.position;

    public OffMeshLink OffMeshLink => _shortcutOffMeshLink;

    public float ShortcutUnlockTimeLeft => _shortcutFullUnlockTime - _shortcutCurrentUnlockTime;

    #endregion

    #region Initialization

    private void Awake()
    {
        foreach (FenceLayerSingle fenceLayerSingle in _allFenceLayersSingle)
        {
            fenceLayerSingle.OriginalFenceTransform.gameObject.SetActive(true);
            fenceLayerSingle.UnlockedFenceTransform.gameObject.SetActive(false);
        }
    }

    #endregion

    #region Unlocking Shortcut

    public void StartUnlockingShortcut()
    {
        if (IsServer == false)
            return;

        UnlockShortcutAsync(_unlockCancellationToken.Token).Forget();
    }

    private async UniTaskVoid UnlockShortcutAsync(CancellationToken cancellationToken)
    {
        if (IsUnlocked)
            return;

        IsCurrentlyBeingUnlocked = true;

        _shortcutOffMeshLink.costOverride = _currentlyUnlockingShortcutOverrideCost;

        while (_shortcutCurrentUnlockTime < _shortcutFullUnlockTime)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _shortcutCurrentUnlockTime += Time.deltaTime;

            await UniTask.NextFrame();
        }

        VFXManager.Instance.CreateVFX(VFXManager.VFXType.BombExplosion, 2.5f, _explosionVFXPosition.position,
            Vector3.zero);

        await WaitForAllFencesToFallAsync();

        IsCurrentlyBeingUnlocked = false;

        _shortcutOffMeshLink.costOverride = _unlockedShortcutOverrideCost;
    }

    public void StopUnlockingShortcut()
    {
        if (IsServer == false)
            return;

        IsCurrentlyBeingUnlocked = false;
        _unlockCancellationToken.Cancel();
        _unlockCancellationToken = new();
    }

    #endregion

    #region Animation

    private async UniTask WaitForAllFencesToFallAsync()
    {
        _currentFallenFenceIndex = 0;

        _allFenceLayersSingle[_currentFallenFenceIndex].FenceAnimator
            .SetInteger(State, (int)FenceAnimationStates.Falling);

        await UniTask.WaitUntil(() => _currentFallenFenceIndex >= _allFenceLayersSingle.Count);
    }

    public void OnTriggerNextFenceFall()
    {
        if (IsServer == false)
            return;

        _currentFallenFenceIndex += 1;

        if (_currentFallenFenceIndex < _allFenceLayersSingle.Count)
            _allFenceLayersSingle[_currentFallenFenceIndex].FenceAnimator
                .SetInteger(State, (int)FenceAnimationStates.Falling);
    }

    #endregion

    public void OnSceneReset()
    {
        foreach (FenceLayerSingle fenceLayerSingle in _allFenceLayersSingle)
        {
            fenceLayerSingle.UnlockedFenceTransform.gameObject.SetActive(false);
            fenceLayerSingle.OriginalFenceTransform.gameObject.SetActive(true);

            if (IsServer == false)
                continue;

            fenceLayerSingle.FenceAnimator.SetInteger(State, (int)FenceAnimationStates.Idle);
        }

        if (IsServer == false)
            return;

        _currentFallenFenceIndex = 0;
        IsCurrentlyBeingUnlocked = false;
        _shortcutCurrentUnlockTime = 0f;
        _shortcutOffMeshLink.costOverride = _lockedShortcutOverrideCost;
    }
}