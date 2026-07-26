#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

public class ExplosiveBaseShootingBlock : BaseShootingBlock, ISceneResettable, IDisposable
{
    #region Events

    public override event Action AmmoCountChanged;
    public override event Action<OperationSavingStatType, object> DataChanged;

    #endregion

    #region Variables & References

    [SerializeField] private ShootingBlockType _shootingBlockType;
    [SerializeField] private int _maxAmmoCount = 6;
    [SerializeField] private float _shootCooldown = 1.5f;
    [SerializeField] private List<AmmoType> _allowedAmmoTypes;
    [SerializeField] private Transform _shootingPosition;
    [SerializeField] private LayerMask _damagingLayerMask;
    [SerializeField] private float _dealingDamage;
    [SerializeField] private ExplosiveBulletSingle _explosiveBulletPrefab;
    [SerializeField] private int _bulletTrajectoryPointsCount = 10;

    private bool _isShootingQueuesInProcess;
    private CancellationTokenSource _shootingCancellationToken = new();
    private readonly NetworkVariable<int> _currentAmmoCount = new();
    private readonly NetworkVariable<AmmoType> _currentAmmoType = new();

    private IShootingAnglesProvider _shootingAnglesProvider;

    #endregion

    #region Properties

    private bool IsCanFireExplosiveAmmo =>
        _shootingAnglesProvider.IsCanFireExplosiveAmmo && _currentAmmoType.Value != AmmoType.No;

    public override AmmoType CurrentAmmoType => _currentAmmoType.Value;

    public override int CurrentAmmoCount => _currentAmmoCount.Value;

    public override int MaxAmmoCount => _maxAmmoCount;

    public override ShootingBlockType ShootingBlockType => _shootingBlockType;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(IShootingAnglesProvider shootingAnglesProvider)
    {
        _shootingAnglesProvider = shootingAnglesProvider;
    }

    #endregion

    #region Shooting

    public override void Shoot(ShootingType shootingType)
    {
        ShootServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void ShootServerRpc()
    {
        if (_isShootingQueuesInProcess)
            return;

        if (IsCanFireExplosiveAmmo == false)
            return;

        int totalShotsCount = 1;
        StartShootingQueueAsync(totalShotsCount, _shootingCancellationToken.Token).Forget();
    }

    private async UniTaskVoid StartShootingQueueAsync(int totalShotsCount, CancellationToken cancellationToken)
    {
        _isShootingQueuesInProcess = true;

        while (totalShotsCount > 0)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            InitiateShot();
            totalShotsCount--;

            AmmoCountChanged?.Invoke();

            await UniTask.WaitForSeconds(_shootCooldown, cancellationToken: cancellationToken);
        }

        _isShootingQueuesInProcess = false;
    }

    private void InitiateShot()
    {
        if (IsServer == false)
            return;

        float bulletFlightTime = GetBulletFlightTime();
        List<Vector3> bulletTrajectory = GetAmmoTrajectory(bulletFlightTime);
        Vector3 bulletSpawningPosition = _shootingPosition.position;

        for (int j = 0; j < bulletTrajectory.Count; j++)
            bulletTrajectory[j] += bulletSpawningPosition;

        DataChanged?.Invoke(OperationSavingStatType.FiringMachineExplosiveShotInitiated, 1);

        ExplosiveBulletSingle newBulletSingle = Instantiate(_explosiveBulletPrefab, bulletSpawningPosition,
            _explosiveBulletPrefab.transform.rotation);

        NetworkObject newBulletSingleNetworkObject = newBulletSingle.GetComponent<NetworkObject>();
        newBulletSingleNetworkObject.Spawn();

        Debug.Log($"{bulletFlightTime}");

        newBulletSingle.Initialize(bulletFlightTime, bulletFlightTime * 2, bulletTrajectory, _damagingLayerMask,
            _dealingDamage);

        _currentAmmoCount.Value -= 1;
    }

    #endregion

    #region Ammo Type

    public override void SetAmmoType(AmmoType blockAmmoType)
    {
        SetAmmoTypeServerRpc(blockAmmoType);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetAmmoTypeServerRpc(AmmoType blockAmmoType)
    {
        if (_allowedAmmoTypes.Contains(blockAmmoType) == false)
            return;

        _currentAmmoType.Value = blockAmmoType;

        if (blockAmmoType == AmmoType.No)
            _currentAmmoCount.Value = 0;
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

    private float GetBulletFlightTime()
    {
        return BallisticsHelper.CalculateFlightTime(GetTotalFlightDistance()) * 2.5f;
    }

    private float GetTotalFlightDistance()
    {
        return _shootingAnglesProvider.TotalFlightDistance;
    }

    private List<Vector3> GetAmmoTrajectory(float bulletFlightTime)
    {
        return Enumerable.Range(1, _bulletTrajectoryPointsCount)
            .Select(i => GetAmmoTrajectoryPointFromCurrentTime(i * bulletFlightTime / _bulletTrajectoryPointsCount,
                bulletFlightTime, GetTotalFlightDistance())).ToList();
    }

    private Vector3 GetAmmoTrajectoryPointFromCurrentTime(float currentTime, float totalFlightTime,
        float flightDistance)
    {
        Vector3 point = BallisticsHelper.GetTrajectoryPoint(
            currentTime,
            totalFlightTime,
            flightDistance
        );

        Debug.Log(point);

        return _shootingAnglesProvider.GetRotatedPoint(point);
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