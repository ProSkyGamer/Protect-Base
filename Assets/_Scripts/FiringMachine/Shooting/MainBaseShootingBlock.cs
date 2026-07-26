#region

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

public class MainBaseShootingBlock : BaseShootingBlock, ISceneResettable, IDisposable
{
    #region Events

    public override event Action AmmoCountChanged;
    public override event Action<OperationSavingStatType, object> DataChanged;

    #endregion

    #region Variables & References

    [SerializeField] private ShootingBlockType _shootingBlockType;
    [SerializeField] private int _maxAmmoCount = 80;
    [SerializeField] private float _shootCooldown = .2f;
    [SerializeField] private float _bulletDistance = 200f;
    [SerializeField] private Transform _shootingPosition;
    [SerializeField] private LayerMask _damagingLayerMask;
    [SerializeField] private float _dealingDamage;
    [SerializeField] private List<Transform> _vfxSpawningPositions;

    private bool _isShootingQueuesInProcess;
    private CancellationTokenSource _shootingCancellationToken = new();
    private const int ShotsPreRound = 2;
    private const int ShotsCountSingle = 2;
    private const int ShotsCountMulti = 10;

    private readonly NetworkVariable<int> _currentAmmoCount = new();

    private IShootingAnglesProvider _shootingAnglesProvider;

    #endregion

    #region Properties

    public override AmmoType CurrentAmmoType => AmmoType.Patr7N6;

    public override int CurrentAmmoCount => _currentAmmoCount.Value;

    public override int MaxAmmoCount => _maxAmmoCount;

    public override ShootingBlockType ShootingBlockType => _shootingBlockType;

    #endregion

    #region Initiailization

    [Inject]
    public void Construct(IShootingAnglesProvider shootingAnglesProvider)
    {
        _shootingAnglesProvider = shootingAnglesProvider;
    }

    #endregion

    #region Shoot

    public override void Shoot(ShootingType shootingType)
    {
        ShootServerRpc(shootingType);
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootServerRpc(ShootingType shootingType)
    {
        if (_isShootingQueuesInProcess)
            return;

        int totalShotsCount;

        switch (shootingType)
        {
            default:
            case ShootingType.Single:
                totalShotsCount = ShotsCountSingle;

                break;

            case ShootingType.Multi:
                totalShotsCount = ShotsCountMulti;

                break;
        }

        StartShootingQueueAsync(totalShotsCount, _shootingCancellationToken.Token).Forget();
    }

    private async UniTaskVoid StartShootingQueueAsync(int totalShotsCount, CancellationToken cancellationToken)
    {
        _isShootingQueuesInProcess = true;

        while (totalShotsCount > 0)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            for (int i = 0; i < ShotsPreRound; i++)
            {
                if (totalShotsCount == 0)
                    break;

                InitiateShot();
                totalShotsCount--;
            }

            foreach (Transform vfxSpawningPositionTransform in _vfxSpawningPositions)
            {
                VFXManager.Instance.CreateVFX(VFXManager.VFXType.BulletSparks, 2.5f,
                    vfxSpawningPositionTransform.position,
                    transform.eulerAngles);
            }

            AmmoCountChanged?.Invoke();

            await UniTask.WaitForSeconds(_shootCooldown, cancellationToken: cancellationToken);
        }

        _isShootingQueuesInProcess = false;
    }

    private void InitiateShot()
    {
        Vector3 bulletSpawningPosition = _shootingPosition.position;

        Vector3 castPosition = bulletSpawningPosition;
        Vector3 castFullDirection = GetAmmoFinalPoint();
        float castDistance = castFullDirection.magnitude;

        bool isRaycastHit = Physics.Raycast(castPosition, castFullDirection.normalized, out RaycastHit hitInfo,
            castDistance, _damagingLayerMask);

        Debug.DrawLine(transform.position, transform.position + castFullDirection.normalized * 200, Color.red, 10f);

        DataChanged?.Invoke(OperationSavingStatType.FiringMachineRegularShotInitiated, 1);

        if (isRaycastHit && hitInfo.transform.TryGetComponent(out IHaveHealth destructableObject))
        {
            DataChanged?.Invoke(OperationSavingStatType.FiringMachineRegularShotHit, 1);
            destructableObject.TakeDamage(_dealingDamage);
        }

        _currentAmmoCount.Value -= 1;
    }

    #endregion

    #region Ammo Type

    public override void SetAmmoType(AmmoType blockAmmoType)
    {
    }

    #endregion

    #region Ammo Count

    public override void ResetAmmoCount()
    {
        ResetAmmoCountServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetAmmoCountServerRpc()
    {
        _currentAmmoCount.Value = _maxAmmoCount;
    }

    #endregion

    #region Get

    private Vector3 GetAmmoFinalPoint()
    {
        Vector3 ammoFinalPoint = new(0f, 0f, _bulletDistance);
        ammoFinalPoint = _shootingAnglesProvider.GetRotatedPoint(ammoFinalPoint);

        return ammoFinalPoint;
    }

    #endregion

    public void OnSceneReset()
    {
        _shootingCancellationToken.Cancel();
        _shootingCancellationToken = new();
    }

    public void Dispose()
    {
        _shootingCancellationToken.Cancel();
    }
}