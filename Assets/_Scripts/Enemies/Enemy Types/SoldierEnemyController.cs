#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using Zenject;
using Random = UnityEngine.Random;

#endregion

[RequireComponent(typeof(SoldierAnimatorController))]
[RequireComponent(typeof(NavMeshAgent))]
public class SoldierEnemyController : EnemyController, IHaveHealth, IDisposable
{
    #region Events

    public override event Action<OperationSavingStatType, object> DataChanged;

    public event Action<float, float> HealthChanged;

    public event Action HealthDepleted;

    public override event Action<EnemyController> EnemySpawned;

    #endregion

    #region Variables & References

    [SerializeField] private Transform _enemySoldierShootingPointTransform;
    [SerializeField] private float _enemySoldierSpeedBaseAnimation;
    [SerializeField] protected LayerMask _damagingLayers;
    [SerializeField] private float _disappearTimeAfterDeath = 10f;

    private bool _isShooting;
    private float _shootingTime;
    private float _ammoShootingInterval;
    private float _shootingTimeCooldown;
    private bool _isShootingCooldownUp;
    private float _bulletAccuracy;
    private float _maxBulletDirectionDeviation;
    private float _findingFiringMachineRadius;

    private bool _isSettingAnimationSpeed;
    private bool _isHit;
    private float _onHitPauseTime;

    private LayerMask _breakableFenceLayerMask;

    private SoldierAnimatorController _soldierAnimatorController;
    [SerializeField] private Transform _enemySoldierRendererTransform;
    [SerializeField] private Transform _enemySoldierThermalRendererTransform;

    private readonly float _pointsMagnitudeAccuracy = 1.5f;

    private readonly CancellationTokenSource _followingPathCancellationToken = new();
    private readonly CancellationTokenSource _shortcutCancellationToken = new();
    private readonly CancellationTokenSource _shootingCancellationToken = new();
    private readonly CancellationTokenSource _corpseDisappearingCancellationToken = new();

    private IPathShortcut _unlockingEnemyShortcut;
    private bool _isUnlockingShortcut;
    private readonly List<ReadonlyPathPoint> _enemyPath = new();
    private NavMeshAgent _navMeshAgent;
    private IPoVSwapper _poVSwapper;
    private Collider _rigidbody;
    private ReadonlyEnemyInitializationStats _readonlyEnemyInitializationStats;
    private EnemyBaseStatsSO _enemyBaseStatsSO;

    #endregion

    #region Properties

    public override IHaveHealth HealthComponent => this;

    public override EnemyType EnemyType => EnemyType.Soldier;

    public override IReadOnlyList<ReadonlyPathPoint> EnemyPath => _enemyPath;

    public EntityTeam EntityTeam => EntityTeam.Enemy;

    #endregion

    #region Initialize

    [Inject]
    public void Construct(ReadonlyEnemyInitializationStats readonlyEnemyInitializationStats, IPoVSwapper poVSwapper,
        EnemyBaseStatsSO enemyBaseStatsSO)
    {
        _poVSwapper = poVSwapper;
        _navMeshAgent = GetComponent<NavMeshAgent>();
        _soldierAnimatorController = GetComponent<SoldierAnimatorController>();

        Debug.Log($"{readonlyEnemyInitializationStats.MaxHealth} {readonlyEnemyInitializationStats.BaseAtk}" +
                  $"{readonlyEnemyInitializationStats.BaseSpeed} {readonlyEnemyInitializationStats.FullPath.Count}");

        if (readonlyEnemyInitializationStats.MaxHealth <= 0f || readonlyEnemyInitializationStats.BaseAtk <= 0f ||
            readonlyEnemyInitializationStats.BaseSpeed <= 0f || readonlyEnemyInitializationStats.FullPath.Count <= 0)
        {
            Debug.Log("Неверные данные инициализации! Объект уничтожен!");
            Destroy(gameObject);
        }

        _enemyBaseStatsSO = enemyBaseStatsSO;
        _readonlyEnemyInitializationStats = readonlyEnemyInitializationStats;

        Debug.Log(readonlyEnemyInitializationStats.FullPath.Count);
    }

    private async UniTaskVoid FollowPathAsync(List<ReadonlyPathPoint> fullPath,
        CancellationToken cancellationToken)
    {
        int currentPathPointIndex = 0;
        float pointReachedCheckingIntervals = .5f;

        while (currentPathPointIndex < fullPath.Count)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (_isHit)
            {
                ChangeSoldierAnimation();
                await UniTask.NextFrame();

                continue;
            }

            _navMeshAgent.destination = fullPath[currentPathPointIndex].WorldPoint;

            ChangeSoldierAnimation();

            if (IsCanMoveToNextPoint())
            {
                currentPathPointIndex++;

                await StartShootingQueueAsync(_shootingTime, _ammoShootingInterval, _bulletAccuracy,
                    _maxBulletDirectionDeviation, _findingFiringMachineRadius, _shootingCancellationToken.Token);

                continue;
            }

            if (TryFindShortcut(out _unlockingEnemyShortcut))
                if (_unlockingEnemyShortcut.IsUnlocked == false)
                    await GoToShortcutAndUnlockAsync(_unlockingEnemyShortcut, _shortcutCancellationToken.Token);

            await UniTask.WaitForSeconds(pointReachedCheckingIntervals, cancellationToken: cancellationToken);
        }

        StartRepeatingShootingQueue(_shootingCancellationToken.Token).Forget();
    }

    private async UniTaskVoid StartRepeatingShootingQueue(CancellationToken cancellationToken)
    {
        while (true)
        {
            Debug.Log(_isShootingCooldownUp);

            ChangeSoldierAnimation();

            await UniTask.WaitUntil(() => _isShootingCooldownUp, cancellationToken: cancellationToken);

            await StartShootingQueueAsync(_shootingTime, _ammoShootingInterval, _bulletAccuracy,
                _maxBulletDirectionDeviation, _findingFiringMachineRadius, _shootingCancellationToken.Token);
        }
    }

    private async UniTask GoToShortcutAndUnlockAsync(IPathShortcut unlockingShortcut,
        CancellationToken cancellationToken)
    {
        Vector3 originalDestinationPosition = _navMeshAgent.destination;

        while (_isDead.Value == false && unlockingShortcut.IsUnlocked == false)
        {
            ChangeSoldierAnimation();

            await UniTask.WaitUntil(() =>
                {
                    Vector3 movingPosition = unlockingShortcut.IsCurrentlyBeingUnlocked
                        ? unlockingShortcut.WaitingPosition
                        : unlockingShortcut.UnlockingPosition;

                    _navMeshAgent.destination = movingPosition;

                    return (_navMeshAgent.destination - transform.position).magnitude <= _pointsMagnitudeAccuracy;
                },
                cancellationToken: cancellationToken);

            _navMeshAgent.speed = 0;

            _isUnlockingShortcut = _unlockingEnemyShortcut.IsCurrentlyBeingUnlocked == false;

            if (_isUnlockingShortcut)
            {
                unlockingShortcut.StartUnlockingShortcut();
                _isSettingAnimationSpeed = true;
            }

            ChangeSoldierAnimation();

            await UniTask.WaitUntil(() =>
                    unlockingShortcut.IsUnlocked || unlockingShortcut.IsCurrentlyBeingUnlocked == false,
                cancellationToken: cancellationToken);

            _navMeshAgent.speed = CurrentSpeed;

            ChangeSoldierAnimation();
        }

        _navMeshAgent.destination = originalDestinationPosition;
    }

    private async UniTask StartShootingQueueAsync(float shootingTime, float ammoShootingInterval, float bulletAccuracy,
        float maxBulletDirectionDeviation, float maxTargetRange, CancellationToken cancellationToken)
    {
        if (IsServer == false)
            return;

        if (_isDead.Value)
            return;

        if (_isShootingCooldownUp == false)
            return;

        if (TryGetClosestAttackingTargetPosition(maxTargetRange, out Vector3 attackingTargetPosition))
        {
            _isShooting = true;

            Quaternion soldierToAttackTargetRotation =
                Quaternion.LookRotation((attackingTargetPosition - transform.position).normalized);

            soldierToAttackTargetRotation.eulerAngles += new Vector3(0f, 30f, 0f);
            transform.rotation = soldierToAttackTargetRotation;

            _navMeshAgent.speed = 0f;

            ChangeSoldierAnimation();

            while (shootingTime > 0)
            {
                bool isBulletAccurate = Random.Range(0f, 1f) <= bulletAccuracy;
                Vector3 bulletTrajectoryPoint = attackingTargetPosition;

                bulletTrajectoryPoint += isBulletAccurate
                    ? Vector3.zero
                    : new Vector3(Random.Range(-maxBulletDirectionDeviation, maxBulletDirectionDeviation),
                        Random.Range(-maxBulletDirectionDeviation, maxBulletDirectionDeviation),
                        Random.Range(-maxBulletDirectionDeviation, maxBulletDirectionDeviation));

                Vector3 attackingDirection =
                    (bulletTrajectoryPoint - _enemySoldierShootingPointTransform.position).normalized;

                Vector3 castPosition = _enemySoldierShootingPointTransform.position;

                bool isRaycastHit = Physics.Raycast(castPosition, attackingDirection, out RaycastHit hitInfo,
                    maxTargetRange,
                    _damagingLayers);

                if (isRaycastHit && hitInfo.transform.TryGetComponent(out IHaveHealth healthComponent))
                {
                    healthComponent.TakeDamage(_currentAtk.Value);

                    if (healthComponent.IsDestroyed)
                        break;
                }

                await UniTask.WaitForSeconds(ammoShootingInterval, cancellationToken: cancellationToken);
                shootingTime -= ammoShootingInterval;
            }

            _isShooting = false;
            _isShootingCooldownUp = false;
            StartShootingCooldown(_shootingTimeCooldown, _shootingCancellationToken.Token).Forget();
        }
    }

    private async UniTaskVoid StartShootingCooldown(float shootingCooldown, CancellationToken cancellationToken)
    {
        if (_isShootingCooldownUp) return;

        await UniTask.WaitForSeconds(shootingCooldown, cancellationToken: cancellationToken);

        _isShootingCooldownUp = true;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _poVSwapper.ChangeInfraredState += PoVSwapper_OnChangeInfraredState;

        bool isInfraredEnabled =
            ClientTypeManager.CurrentClientType is ClientType.Game &&
            _poVSwapper.IsInfraredEnabled;

        _rigidbody = GetComponent<Collider>();
        _rigidbody.isTrigger = IsServer;

        _enemySoldierRendererTransform.gameObject.SetActive(isInfraredEnabled == false);
        _enemySoldierThermalRendererTransform.gameObject.SetActive(isInfraredEnabled);

        EnemySpawned?.Invoke(this);

        DataChanged?.Invoke(OperationSavingStatType.EnemiesMaxHealth, _maxHealth.Value);

        if (IsServer)
        {
            _maxHealth.Value = _readonlyEnemyInitializationStats.MaxHealth;
            _currentHealth.Value = _maxHealth.Value;
            _currentAtk.Value = _readonlyEnemyInitializationStats.BaseAtk;
            _currentSpeed.Value = _readonlyEnemyInitializationStats.BaseSpeed;

            _onHitPauseTime = _soldierAnimatorController.GetAnimationLength(_isShooting
                ? SoldierAnimations.HitWhileShooting
                : SoldierAnimations.Hit);

            _shootingTime = _enemyBaseStatsSO.EnemySoldierShootingTimeOnPointReached;
            _shootingTimeCooldown = _enemyBaseStatsSO.EnemySoldierShootingTimeCooldown;
            _ammoShootingInterval = _enemyBaseStatsSO.EnemySoldierShootingBulletInterval;
            _bulletAccuracy = _enemyBaseStatsSO.EnemySoldierBulletsAccuracy;
            _maxBulletDirectionDeviation = _enemyBaseStatsSO.MaxBulletDirectionDeviation;
            _findingFiringMachineRadius = _enemyBaseStatsSO.EnemySoldierAttackingFiringMachineRadius;
            _breakableFenceLayerMask = _enemyBaseStatsSO.BreakableFenceLayerMask;
            _isShootingCooldownUp = true;

            _navMeshAgent.speed = _currentSpeed.Value;

            Debug.Log($"given path points {_readonlyEnemyInitializationStats.FullPath.Count}");

            _enemyPath.AddRange(_readonlyEnemyInitializationStats.FullPath);

            FollowPathAsync(_enemyPath, _followingPathCancellationToken.Token).Forget();
        }
    }

    private void PoVSwapper_OnChangeInfraredState(bool newState)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        _enemySoldierRendererTransform.gameObject.SetActive(newState == false);
        _enemySoldierThermalRendererTransform.gameObject.SetActive(newState);
    }

    private bool TryFindShortcut(out IPathShortcut foundShortcut)
    {
        float cubeSides = 4f;
        Vector3 castHalfExtents = Vector3.one * (cubeSides / 2);
        Vector3 castPosition = transform.position + transform.TransformDirection(castHalfExtents);
        Vector3 castDirection = transform.forward;
        float castDistance = cubeSides * 2;

        RaycastHit[] allCastHits = Physics.BoxCastAll(castPosition, castHalfExtents, castDirection, transform.rotation,
            castDistance, _breakableFenceLayerMask);

        List<IPathShortcut> allFoundShortcuts = allCastHits.Select(raycastHit => raycastHit.transform.GetComponent<IPathShortcut>())
            .Where(shortcut => shortcut != null).ToList();

        foundShortcut = null;

        if (allFoundShortcuts.Count > 0)
            foundShortcut = allFoundShortcuts.FirstOrDefault(shortcut =>
                shortcut.OffMeshLink.endTransform.position == _navMeshAgent.nextOffMeshLinkData.endPos);

        return foundShortcut != null;
    }

    #endregion

    #region Update

    private void ChangeSoldierAnimation()
    {
        if (IsServer == false)
            return;

        SoldierAnimations newSoldierAnimation =
            SoldierAnimations.GuardIdle;

        float animationSpeed = 1f;
        bool isChangingAnimationSpeedNow = false;

        if (_isDead.Value)
        {
            newSoldierAnimation = SoldierAnimations.Death;
        }
        else if (_isHit)
        {
            if (_currentHealth.Value < 0)
                newSoldierAnimation = SoldierAnimations.Death;
            else
                newSoldierAnimation = _isShooting
                    ? SoldierAnimations.HitWhileShooting
                    : SoldierAnimations.Hit;
        }
        else if (_navMeshAgent.speed <= 0f)
        {
            if (_unlockingEnemyShortcut != null)
            {
                Debug.Log(_unlockingEnemyShortcut);

                if (_isUnlockingShortcut == false)
                {
                    newSoldierAnimation = SoldierAnimations.CombatIdle;
                }
                else
                {
                    newSoldierAnimation = SoldierAnimations.BreakingFence;

                    if (_isSettingAnimationSpeed)
                    {
                        float shortcutUnlockTimeLeft = _unlockingEnemyShortcut.ShortcutUnlockTimeLeft;

                        float fullAnimationTime =
                            _soldierAnimatorController.GetAnimationLength(SoldierAnimations
                                .BreakingFence);

                        animationSpeed = fullAnimationTime / shortcutUnlockTimeLeft;
                        _isSettingAnimationSpeed = false;
                        isChangingAnimationSpeedNow = true;
                    }
                }
            }
            else if (_isShooting)
            {
                newSoldierAnimation = SoldierAnimations.Shoot;

                float fullAnimationTime =
                    _soldierAnimatorController.GetAnimationLength(SoldierAnimations.Shoot);

                animationSpeed = fullAnimationTime / _ammoShootingInterval;
                isChangingAnimationSpeedNow = true;
            }
            else if (_isShootingCooldownUp == false)
            {
                newSoldierAnimation = SoldierAnimations.Reload;

                float fullAnimationTime =
                    _soldierAnimatorController.GetAnimationLength(SoldierAnimations.Reload);

                float reloadTimesCount = (int)_shootingTimeCooldown / fullAnimationTime;
                float fullReloadAnimationTime = fullAnimationTime / reloadTimesCount;
                animationSpeed = fullReloadAnimationTime / _shootingTimeCooldown;
                isChangingAnimationSpeedNow = true;
            }
        }
        else
        {
            newSoldierAnimation = SoldierAnimations.Run;
            animationSpeed = _currentSpeed.Value / _enemySoldierSpeedBaseAnimation;
            isChangingAnimationSpeedNow = true;
        }

        _soldierAnimatorController.ChangeAnimation(newSoldierAnimation);

        if (isChangingAnimationSpeedNow)
            _soldierAnimatorController.ChangeAnimationSpeed(newSoldierAnimation, animationSpeed);
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

        TurnOnHitState(_onHitPauseTime).Forget();
        ChangeSoldierAnimation();

        OperationUpdateManager.RequestUpdate();
    }

    private async UniTaskVoid TurnOnHitState(float hitStateDuration)
    {
        _isHit = true;
        _navMeshAgent.speed = 0f;

        await UniTask.WaitForSeconds(hitStateDuration);

        _navMeshAgent.speed = CurrentSpeed;
        _isHit = false;
        ChangeSoldierAnimation();
    }

    private void Die()
    {
        if (IsServer == false)
            return;

        if (_unlockingEnemyShortcut != null)
            _unlockingEnemyShortcut.StopUnlockingShortcut();

        _shootingCancellationToken.Cancel();
        _shortcutCancellationToken.Cancel();
        _followingPathCancellationToken.Cancel();

        _navMeshAgent.speed = 0f;
        _isDead.Value = true;

        HealthDepleted?.Invoke();

        DeleteCorpseAfterDeathAsync(_disappearTimeAfterDeath, _corpseDisappearingCancellationToken.Token).Forget();
    }

    private async UniTaskVoid DeleteCorpseAfterDeathAsync(float waitingTime, CancellationToken cancellationToken)
    {
        await UniTask.WaitForSeconds(waitingTime, cancellationToken: cancellationToken);

        Destroy(gameObject);
    }

    #endregion

    #region Get

    private bool IsCanMoveToNextPoint()
    {
        float magnitude = (_navMeshAgent.destination - transform.position).magnitude;

        return magnitude <= _pointsMagnitudeAccuracy;
    }

    private bool TryGetClosestAttackingTargetPosition(float searchRange, out Vector3 attackingTargetPosition)
    {
        attackingTargetPosition = Vector3.zero;

        if (IsServer == false)
            return false;

        Vector3 castPosition = transform.position;
        float castRadius = searchRange;

        RaycastHit[] raycastHits =
            Physics.SphereCastAll(castPosition, castRadius, Vector3.down, castRadius, _damagingLayers);

        List<KeyValuePair<IHaveHealth, Vector3>> attackTargetsWithHealth = raycastHits.OrderBy(raycastHit => raycastHit.distance)
            .Select(raycastHit =>
                new KeyValuePair<IHaveHealth, Vector3>(raycastHit.transform.GetComponent<IHaveHealth>(),
                    raycastHit.transform.position)).ToList();

        Dictionary<(IHaveHealth Key, Vector3 Value), KeyValuePair<IHaveHealth, Vector3>> possibleAttackTargets =
            attackTargetsWithHealth.Where(destructableObject =>
                    destructableObject.Key is { IsDestroyed: false } && destructableObject.Key.EntityTeam != EntityTeam)
                .ToDictionary(destructableObject => (destructableObject.Key, destructableObject.Value));

        foreach ((IHaveHealth Key, Vector3 Value) possibleAttackTarget in possibleAttackTargets.Keys)
        {
            bool isDirectionClear = true;

            RaycastHit[] firingMachineDirectionsCast = Physics.RaycastAll(
                _enemySoldierShootingPointTransform.position,
                (possibleAttackTarget.Value - _enemySoldierShootingPointTransform.position)
                .normalized, _findingFiringMachineRadius, _damagingLayers);

            foreach (RaycastHit firingMachineDirectionCast in firingMachineDirectionsCast)
            {
                if (firingMachineDirectionCast.transform.TryGetComponent(out IHaveHealth _) == false &&
                    firingMachineDirectionCast.transform != transform)
                {
                    isDirectionClear = false;

                    break;
                }
            }

            if (isDirectionClear == false)
                continue;

            attackingTargetPosition = possibleAttackTarget.Value;

            return true;
        }

        return false;
    }

    #endregion

    public void Dispose()
    {
        _poVSwapper.ChangeInfraredState -= PoVSwapper_OnChangeInfraredState;

        _corpseDisappearingCancellationToken.Cancel();
        _shootingCancellationToken.Cancel();
        _shortcutCancellationToken.Cancel();
        _followingPathCancellationToken.Cancel();
    }
}