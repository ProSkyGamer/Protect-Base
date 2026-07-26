#region

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

public class BulletSingle : NetworkBehaviour, ISceneResettable, IDisposable, ITickable
{
    #region Variables & References

    [SerializeField] private bool _isHasVFXOnExplode;
    [SerializeField] private VFXManager.VFXType _vfxOnExplodeType;
    [SerializeField] private float _vfxLifetime = 2.5f;
    [SerializeField] private Vector3 _vfxExplosionSpawnPosition;
    [SerializeField] private float _invulnerabilityTime = .25f;

    private bool _isImmortal;
    [SerializeField] private bool _isVelocityLimited;
    [SerializeField] private int _maxVelocityAppliedCount = 3;

    private float _bulletDamage = 2f;
    private LayerMask _checkingLayerMask;
    private readonly CancellationTokenSource _movementCancellationToken = new();
    private readonly CancellationTokenSource _immortalityCancellationToken = new();
    private readonly CancellationTokenSource _lifetimeCancellationToken = new();

    private BoxCollider _bulletCollider;
    private Rigidbody _bulletRigidbody;

    private float _totalFlightTime;
    private float _totalLifetime;
    private readonly List<Vector3> _bulletTrajectory = new();

    private bool _isDestroyed;

    #endregion

    #region Initialize

    public void Initialize(float totalFlightTime, float totalLifetime, List<Vector3> bulletTrajectory,
        LayerMask damagingLayers, float bulletDamage = 1, Vector3 vfxSpawningPosition = new())
    {
        _bulletCollider = GetComponent<BoxCollider>();
        _bulletRigidbody = GetComponent<Rigidbody>();
        _vfxExplosionSpawnPosition = vfxSpawningPosition;
        _bulletDamage = bulletDamage;
        _checkingLayerMask = damagingLayers;

        _totalFlightTime = totalFlightTime;
        _totalLifetime = totalLifetime;
        _bulletTrajectory.AddRange(bulletTrajectory);

        Debug.Log("initialized");
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer == false)
            return;

        ActivateImmortality(_invulnerabilityTime, _immortalityCancellationToken.Token).Forget();
        ActiveBulletLifetime(_totalLifetime, _lifetimeCancellationToken.Token).Forget();
        MoveBulletByTrajectoryAsync(_bulletTrajectory, _totalFlightTime, _movementCancellationToken.Token).Forget();
    }

    #endregion

    #region Bullet Cycle

    private async UniTaskVoid MoveBulletByTrajectoryAsync(List<Vector3> bulletTrajectory, float totalFlightTime,
        CancellationToken cancellationToken)
    {
        int nextTrajectoryIndex = 0;
        int currentVelocityAppliedCount = 0;
        float timeBetweenPoints = totalFlightTime / bulletTrajectory.Count;
        float timeTillNextPoint = timeBetweenPoints;

        Debug.Log(bulletTrajectory.Count);

        while (nextTrajectoryIndex < bulletTrajectory.Count)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            Vector3 currentMovingDirection = bulletTrajectory[nextTrajectoryIndex] - transform.position;

            Vector3 normalizedDirection = currentMovingDirection.normalized;
            float xAngles = Mathf.Atan2(normalizedDirection.x, normalizedDirection.z);

            float yAngles = Mathf.Atan2(-normalizedDirection.y,
                Mathf.Sqrt(
                    normalizedDirection.x * normalizedDirection.x + normalizedDirection.z * normalizedDirection.z));

            Vector3 directionEulerAngles = new(yAngles * 180f / Mathf.PI, xAngles * 180f / Mathf.PI, 0f);
            transform.localEulerAngles = directionEulerAngles;

            Vector3 deltaMovement = currentMovingDirection * (2.5f / timeBetweenPoints * Time.deltaTime);

            Debug.Log($"{timeTillNextPoint}");

            if (_isVelocityLimited == false || _isVelocityLimited && currentVelocityAppliedCount < _maxVelocityAppliedCount)
            {
                _bulletRigidbody.AddForce(deltaMovement, ForceMode.VelocityChange);
                currentVelocityAppliedCount++;
                timeTillNextPoint -= Time.deltaTime;
                await UniTask.NextFrame();
            }
            else
            {
                if (timeTillNextPoint > 0)
                    await UniTask.WaitForSeconds(timeTillNextPoint, cancellationToken: cancellationToken);

                timeTillNextPoint = timeBetweenPoints;
                nextTrajectoryIndex += 1;

                if (nextTrajectoryIndex < bulletTrajectory.Count)
                {
                    currentVelocityAppliedCount = 0;
                    _bulletRigidbody.velocity = Vector3.zero;
                    Debug.Log($"next {nextTrajectoryIndex}");
                    transform.localEulerAngles = directionEulerAngles;
                }
            }
        }
    }

    private async UniTaskVoid ActivateImmortality(float vulnerabilityTime, CancellationToken cancellationToken)
    {
        _isImmortal = true;

        await UniTask.WaitForSeconds(vulnerabilityTime, cancellationToken: cancellationToken);

        _isImmortal = false;
    }

    private async UniTaskVoid ActiveBulletLifetime(float bulletLifetime, CancellationToken cancellationToken)
    {
        await UniTask.WaitForSeconds(bulletLifetime, cancellationToken: cancellationToken);

        ExplodeBullet();
    }

    public void Tick()
    {
        if (_isImmortal)
            return;

        if (CheckCollidingObjects(out List<Transform> _))
        {
            _movementCancellationToken.Cancel();
            _bulletRigidbody.velocity = Vector3.zero;

            ExplodeBullet();
        }
    }

    #endregion

    #region Collide

    private bool CheckCollidingObjects(out List<Transform> collidingObjects)
    {
        collidingObjects = new List<Transform>();

        if (IsServer == false)
            return false;

        Vector3 castPosition = transform.position + _bulletCollider.center;
        Vector3 halfExtents = _bulletCollider.size;

        Vector3 castDirection = Vector3.down;

        RaycastHit[] collidingRaycastHits = Physics.BoxCastAll(castPosition, halfExtents, castDirection,
            Quaternion.identity,
            _bulletCollider.size.magnitude, _checkingLayerMask);

        foreach (RaycastHit raycastHit in collidingRaycastHits)
        {
            if (!raycastHit.transform.TryGetComponent(out BulletSingle _))
            {
                collidingObjects.Add(raycastHit.transform);
                Debug.Log($"colliding with {raycastHit.transform.name}");
            }
        }

        return collidingObjects.Count > 0;
    }

    protected virtual List<IHaveHealth> GetFinalCollidingHealthObjects()
    {
        if (IsServer == false)
            return null;

        List<IHaveHealth> allHittingHealthObjects = new();

        CheckCollidingObjects(out List<Transform> collidingObjects);

        foreach (Transform collidingObject in collidingObjects)
        {
            if (collidingObject.TryGetComponent(out IHaveHealth healthComponent))
                allHittingHealthObjects.Add(healthComponent);
        }

        return allHittingHealthObjects;
    }

    #endregion

    #region Destroy

    public void ExplodeBullet()
    {
        if (IsServer == false)
            return;

        if (_isDestroyed)
            return;

        Debug.Log("exploded");

        _isDestroyed = true;
        OnBulletDestroy();

        Destroy(gameObject);
    }

    protected virtual void OnBulletDestroy()
    {
        if (IsServer == false)
            return;

        List<IHaveHealth> damagingEnemies = GetFinalCollidingHealthObjects();

        _immortalityCancellationToken.Cancel();
        _movementCancellationToken.Cancel();
        _lifetimeCancellationToken.Cancel();

        foreach (IHaveHealth damagingEnemy in damagingEnemies)
        {
            damagingEnemy.TakeDamage(_bulletDamage);
        }

        if (_isHasVFXOnExplode)
        {
            Vector3 vfxSpawnPosition = _vfxExplosionSpawnPosition != Vector3.zero
                ? _vfxExplosionSpawnPosition
                : transform.position;

            VFXManager.Instance.CreateVFX(_vfxOnExplodeType, _vfxLifetime, vfxSpawnPosition, Vector3.zero);
        }
    }

    #endregion

    public void OnSceneReset()
    {
        if (IsServer == false)
            return;

        Destroy(gameObject);
    }

    public void Dispose()
    {
        _immortalityCancellationToken.Cancel();
        _movementCancellationToken.Cancel();
        _lifetimeCancellationToken.Cancel();
    }
}