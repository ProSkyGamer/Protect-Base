#region

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

public class EnemyDroneController : EnemyController, IHaveHealth
{
    #region Events

    public event Action<float, float> HealthChanged;

    public event Action HealthDepleted;

    public override event Action<EnemyController> EnemySpawned;
    public override event Action<OperationSavingStatType, object> DataChanged;

    #endregion

    #region Variables & References

    [SerializeField] private ExplosiveBulletSingle _bombPrefab;
    [SerializeField] protected LayerMask _damagingLayers;
    private readonly float _pointsMagnitudeAccuracy = .5f;
    private readonly CancellationTokenSource _followingPathCancellationToken = new();
    private readonly List<ReadonlyPathPoint> _dronePathPoints = new();

    private ReadonlyEnemyInitializationStats _enemyInitializationStats;
    private Vector3 _pathPointDestination;
    private bool _isBombDropped;
    private bool _isFinalPointReached;

    #endregion

    #region Properties

    private bool IsCanMoveToNextPoint => (_pathPointDestination - transform.position).magnitude <= _pointsMagnitudeAccuracy;

    public override IHaveHealth HealthComponent => this;

    public override EnemyType EnemyType => EnemyType.Drone;

    public override IReadOnlyList<ReadonlyPathPoint> EnemyPath => _dronePathPoints;

    public EntityTeam EntityTeam => EntityTeam.Enemy;

    #endregion

    #region Initialize

    [Inject]
    public void Construct(ReadonlyEnemyInitializationStats readonlyEnemyInitializationStats)
    {
        if (readonlyEnemyInitializationStats.MaxHealth <= 0f || readonlyEnemyInitializationStats.BaseAtk <= 0f ||
            readonlyEnemyInitializationStats.BaseSpeed <= 0f || readonlyEnemyInitializationStats.FullPath.Count <= 0)
        {
            Debug.Log("Неверные данные инициализации! Объект уничтожен!");
            Destroy(gameObject);
        }

        _enemyInitializationStats = readonlyEnemyInitializationStats;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        EnemySpawned?.Invoke(this);

        if (IsServer)
        {
            _maxHealth.Value = _enemyInitializationStats.MaxHealth;
            _currentHealth.Value = _maxHealth.Value;
            _currentAtk.Value = _enemyInitializationStats.BaseAtk;
            _currentSpeed.Value = _enemyInitializationStats.BaseSpeed;

            _dronePathPoints.AddRange(_enemyInitializationStats.FullPath);

            FollowPathAsync(_dronePathPoints, _followingPathCancellationToken.Token).Forget();
        }

        DataChanged?.Invoke(OperationSavingStatType.EnemiesMaxHealth, _maxHealth.Value);
    }

    private async UniTaskVoid FollowPathAsync(List<ReadonlyPathPoint> fullPath,
        CancellationToken cancellationToken)
    {
        int currentPathPointIndex = 0;
        Vector3 dronePathPointDirection = GetPathPointDirection(fullPath, currentPathPointIndex);

        foreach (ReadonlyPathPoint pathPoint in fullPath)
        {
            Debug.Log(pathPoint.WorldPoint);
        }

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            transform.position += dronePathPointDirection * (Time.deltaTime * CurrentSpeed);

            if (IsCanMoveToNextPoint)
            {
                currentPathPointIndex++;

                if (currentPathPointIndex >= fullPath.Count)
                    break;

                dronePathPointDirection = GetPathPointDirection(fullPath, currentPathPointIndex);

                continue;
            }

            await UniTask.NextFrame();
        }

        _isFinalPointReached = true;
        DropBomb(out _);
        TakeDamage(CurrentHealth);
    }

    private Vector3 GetPathPointDirection(List<ReadonlyPathPoint> fullPath, int currentPathPointIndex)
    {
        Vector3 castPosition = transform.position;

        Vector3 castDirection =
            (fullPath[currentPathPointIndex].WorldPoint - transform.position)
            .normalized;

        float castDistance = (fullPath[currentPathPointIndex].WorldPoint - transform.position)
            .magnitude;

        bool dronePathPointRaycast =
            Physics.Raycast(castPosition, castDirection, out RaycastHit dronePathCastInfo, castDistance);

        bool isReachingLastPoint = !dronePathPointRaycast;
        Vector3 dronePathPointDirection = castDirection;

        _pathPointDestination = isReachingLastPoint
            ? fullPath[currentPathPointIndex].WorldPoint
            : dronePathCastInfo.point;

        return dronePathPointDirection;
    }

    #endregion

    #region Health

    public void TakeDamage(float damage)
    {
        if (IsServer == false)
            return;

        if (_isDead.Value)
            return;

        _currentHealth.Value -= damage;

        DataChanged?.Invoke(OperationSavingStatType.EnemiesDamageTaken, damage);

        HealthChanged?.Invoke(_currentHealth.Value, damage);

        Debug.Log($"{gameObject.name} took {damage} damage! New health: {_currentHealth}");

        if (_currentHealth.Value <= 0f)
            Die();

        OperationUpdateManager.RequestUpdate();
    }

    private void Die()
    {
        if (IsServer == false)
            return;

        _followingPathCancellationToken.Cancel();

        _isDead.Value = true;

        if (_isBombDropped == false)
        {
            DropBomb(out ExplosiveBulletSingle droppedBomb);
            Debug.Log(droppedBomb);
            droppedBomb.ChangeExplosiveBulletSmokeVFXType(VFXManager.VFXType.DroneSmokeExplosion);
            droppedBomb.ExplodeBullet();
            VFXManager.Instance.CreateVFX(VFXManager.VFXType.BombExplosion, 2.5f, transform.position, Vector3.zero);
        }

        HealthDepleted?.Invoke();
        Destroy(gameObject);
    }

    private void DropBomb(out ExplosiveBulletSingle explosiveBulletSingle)
    {
        explosiveBulletSingle = null;

        if (IsServer == false)
            return;

        if (_isBombDropped)
            return;

        _isBombDropped = true;

        explosiveBulletSingle = Instantiate(_bombPrefab, transform.position, Quaternion.identity);

        List<Vector3> bombTrajectory = new()
        {
            new Vector3(transform.position.x,
                0f,
                transform.position.z)
        };

        Vector3 bombVFXSpawningPosition = _isFinalPointReached
            ? _pathPointDestination - new Vector3(0f, transform.position.y, 0f)
            : Vector3.zero;

        explosiveBulletSingle.Initialize(1.5f, 4f, bombTrajectory, _damagingLayers, _currentAtk.Value,
            bombVFXSpawningPosition);

        explosiveBulletSingle.GetComponent<NetworkObject>().Spawn();
    }

    #endregion

    public override void OnDestroy()
    {
        _followingPathCancellationToken.Cancel();
    }
}