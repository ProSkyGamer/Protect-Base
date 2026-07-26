#region

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Zenject;

#endregion

[RequireComponent(typeof(VehicleAnimationController))]
[RequireComponent(typeof(NavMeshAgent))]
public class VehicleEnemyController : EnemyController, IHaveHealth, IDisposable
{
    #region Events

    public event Action<float, float> HealthChanged;

    public event Action HealthDepleted;

    public override event Action<EnemyController> EnemySpawned;
    public override event Action<OperationSavingStatType, object> DataChanged;

    #endregion

    #region Variables & References

    [SerializeField] private List<Transform> _allSpawningSoldiersPoints;
    private readonly List<ReadonlyPathPoint> _vehiclePath = new();
    private readonly List<ReadonlyPathPoint> _soldiersPath = new();
    private int _spawningSoldiersCount;
    private ReadonlyEnemyInitializationStats _enemyInitializationStats;

    private readonly float _pointsMagnitudeAccuracy = .5f;
    private VehicleAnimationController _vehicleAnimationController;
    private bool _isEnemySoldiersSpawned;

    private readonly List<ReadonlyPathPoint> _enemyPath = new();
    private readonly CancellationTokenSource _followingPathCancellationToken = new();

    private NavMeshAgent _navMeshAgent;

    public override IHaveHealth HealthComponent => this;

    public override EnemyType EnemyType => EnemyType.Vehicle;

    public override IReadOnlyList<ReadonlyPathPoint> EnemyPath => _enemyPath;

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

        _vehicleAnimationController = GetComponent<VehicleAnimationController>();
        _navMeshAgent = GetComponent<NavMeshAgent>();

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
            _navMeshAgent.speed = _currentSpeed.Value;

            if (_enemyInitializationStats is VehicleInitializationStats vehicleInitializationStats)
                _spawningSoldiersCount = vehicleInitializationStats.SpawningSoldiersCount;
            else
                _spawningSoldiersCount = 2;

            bool isDisembarkPointFound = false;

            _enemyPath.AddRange(_enemyInitializationStats.FullPath);

            foreach (ReadonlyPathPoint enemyPathPoint in _enemyInitializationStats.FullPath)
            {
                if (isDisembarkPointFound == false)
                    isDisembarkPointFound =
                        enemyPathPoint.PathPointType is PathPointType.DisembarkedSoldiersPathPoint;

                if (isDisembarkPointFound == false)
                    _vehiclePath.Add(enemyPathPoint);

                if (isDisembarkPointFound)
                    _soldiersPath.Add(enemyPathPoint);
            }

            FollowPathAsync(_vehiclePath, _followingPathCancellationToken.Token).Forget();
        }
    }

    private async UniTaskVoid FollowPathAsync(List<ReadonlyPathPoint> fullPath,
        CancellationToken cancellationToken)
    {
        int currentPathPointIndex = 0;
        float pointReachedCheckingIntervals = .5f;

        while (currentPathPointIndex < fullPath.Count)
        {
            _navMeshAgent.destination = fullPath[currentPathPointIndex].WorldPoint;

            _vehicleAnimationController.ChangeAnimation(_navMeshAgent.speed > 0f
                ? VehicleAnimations.Move
                : VehicleAnimations.Idle);

            if (IsCanMoveToNextPoint())
            {
                currentPathPointIndex++;

                continue;
            }

            await UniTask.WaitForSeconds(pointReachedCheckingIntervals, cancellationToken: cancellationToken);
        }

        SpawnSoldiers();
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

        SpawnSoldiers();

        HealthDepleted?.Invoke();

        Destroy(gameObject);
    }

    private void SpawnSoldiers()
    {
        if (IsServer == false)
            return;

        if (_isEnemySoldiersSpawned)
            return;

        _isEnemySoldiersSpawned = true;

        for (int i = 0; i < _spawningSoldiersCount && i < _allSpawningSoldiersPoints.Count; i++)
        {
            List<ReadonlyPathPoint> newSpawningEnemyPath = new();

            ReadonlyPathPoint spawningEnemyPoint =
                new(Vector2.zero, Vector2.zero, _allSpawningSoldiersPoints[i].position, PathPointType.SpawnPathPoint);

            newSpawningEnemyPath.Add(spawningEnemyPoint);

            newSpawningEnemyPath.AddRange(_soldiersPath);
            /*OperationsManager.Instance.SpawnEnemy(EnemyType.Soldier, newSpawningEnemyPath,
                EnemiesSharedVariablesManager.Instance.GetEnemyHealthStat(EnemyType.Soldier,
                    EnemyStatSize.Medium),
                EnemiesSharedVariablesManager.Instance.GetEnemyAtkStat(EnemyType.Soldier,
                    EnemyStatSize.Medium),
                EnemiesSharedVariablesManager.Instance.GetEnemySpeedStat(EnemyType.Soldier,
                    EnemyStatSize.Medium), 0);*/

            //TODO FIX THIS
        }
    }

    #endregion

    #region Get

    private bool IsCanMoveToNextPoint()
    {
        return (_navMeshAgent.pathEndPosition - transform.position).magnitude <= _pointsMagnitudeAccuracy;
    }

    #endregion

    public void Dispose()
    {
        _followingPathCancellationToken.Cancel();
    }
}