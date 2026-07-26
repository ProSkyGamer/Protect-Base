#region

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

[Serializable]
public class EnemyPrefabInfo
{
    public EnemyType EnemyType;
    public EnemyController EnemyController;
    public Vector3 SpawnPointDispersion;
    public Vector3 PathPointDispersion;
}

public class OperationsManager : NetworkBehaviour, IOperationDataProvider, IOperationStatsDataProvider, ISceneResettable, IDisposable
{
    #region Events

    public event Action<ReadonlyOperationData> OperationStarted;

    public event Action OperationStopped;

    public event Action<OperationSavingStatType, object> DataChanged;

    #endregion

    #region Variables & References

    private readonly List<EnemyController> _currentAliveEnemies = new();

    [SerializeField] private float _maxOperationLength = 600f;

    private readonly NetworkVariable<float> _currentOperationTime = new();

    private readonly NetworkVariable<bool> _isOperationActive = new();
    private readonly NetworkVariable<bool> _isAllWavesSpawned = new();
    private readonly NetworkVariable<int> _spawnedWavesCount = new();
    private readonly NetworkVariable<int> _totalWavesCount = new();

    private CancellationTokenSource _operationCancellationToken = new();
    private EnemiesFactory _enemiesFactory;
    private MarkersManager _markersManager;

    #endregion

    #region Properties

    public float MaxOperationLength => _maxOperationLength;

    public float CurrentOperationTime => _currentOperationTime.Value;

    public bool IsOperationActive => _isOperationActive.Value;

    public int TotalWavesCount => _totalWavesCount.Value;

    public int SpawnedWavesCount => _spawnedWavesCount.Value;

    public int CurrentlyAliveEnemies => _currentAliveEnemies.Count;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(EnemiesFactory enemiesFactory, MarkersManager markersManager)
    {
        _enemiesFactory = enemiesFactory;
        _markersManager = markersManager;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer == false)
            _isOperationActive.OnValueChanged += IsOperationActive_OnValueChanged;
    }

    private void IsOperationActive_OnValueChanged(bool previousValue, bool newValue)
    {
        if (IsServer)
            return;

        if (previousValue == newValue)
            return;

        if (newValue == false)
            OperationStopped?.Invoke();
    }

    #endregion

    #region Operation Life Cycle

    private async UniTaskVoid RunOperationAsync(ReadonlyOperationData operationData,
        CancellationToken cancellationToken)
    {
        _isOperationActive.Value = true;
        float operationWaveSpawnCheckInterval = .5f;

        List<OperationWave> notSpawnedOperationWaves = new List<OperationWave>();

        notSpawnedOperationWaves.AddRange(operationData.AllOperationWaves);

        Debug.Log(notSpawnedOperationWaves.Count);

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (notSpawnedOperationWaves.Count > 0)
                for (int i = 0; i < notSpawnedOperationWaves.Count; i++)
                {
                    OperationWave waveSingle = notSpawnedOperationWaves[i];

                    if (waveSingle.WaveSpawnTime > _currentOperationTime.Value)
                        continue;

                    SpawnWave(waveSingle);

                    notSpawnedOperationWaves.RemoveAt(i);
                    i--;

                    _spawnedWavesCount.Value += 1;
                    _isAllWavesSpawned.Value = notSpawnedOperationWaves.Count > 0;
                }

            await UniTask.WaitForSeconds(operationWaveSpawnCheckInterval, cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            _currentOperationTime.Value += operationWaveSpawnCheckInterval;
        }
    }

    private async UniTask PeriodicallyUpdateOperationInfoAsync(CancellationToken cancellationToken)
    {
        float operationTimerUpdateInterval = .125f;

        while (_isOperationActive.Value)
        {
            await UniTask.WaitForSeconds(operationTimerUpdateInterval, cancellationToken: cancellationToken);
            OperationUpdateManager.RequestUpdate();
        }
    }

    private void SpawnWave(OperationWave operationWave)
    {
        if (IsServer == false)
            return;

        Debug.Log($"spawned wave {operationWave.WaveSpawnTime} {operationWave.SpawningEnemyType} {operationWave.SpawningEnemyCount}");

        SpawnEnemy(operationWave.SpawningEnemyType, operationWave.SpawningEnemyCount,
            operationWave.ReadonlyEnemyInitializationStats);
    }

    #endregion

    #region Operation

    public void StartOperation(ReadonlyOperationData operationData)
    {
        if (ClientTypeManager.CurrentClientType is not (ClientType.OperationSettings or ClientType.Game) &&
            IsServer == false)
            return;

        operationData.PackForNetworkTransfer();

        StartOperationServerRpc(operationData);
    }

    [ServerRpc(RequireOwnership = false)]
    private void StartOperationServerRpc(ReadonlyOperationData operationData)
    {
        if (operationData == null)
            return;

        operationData.UnpackAfterNetworkTransfer();

        if (operationData.AllOperationWaves.Count <= 0)
            return;

        _currentOperationTime.Value = 0f;
        _isOperationActive.Value = true;

        _totalWavesCount.Value = operationData.AllOperationWaves.Count;

        RunOperationAsync(operationData, _operationCancellationToken.Token).Forget();
        PeriodicallyUpdateOperationInfoAsync(_operationCancellationToken.Token).Forget();

        operationData.PackForNetworkTransfer();
        StartOperationClientRpc(operationData);

        OperationUpdateManager.RequestUpdate();
    }

    [ClientRpc]
    private void StartOperationClientRpc(ReadonlyOperationData operationData)
    {
        operationData.UnpackAfterNetworkTransfer();

        DataChanged?.Invoke(OperationSavingStatType.WeatherSeason, operationData.OperationWeather.Season);
        DataChanged?.Invoke(OperationSavingStatType.WeatherTime, operationData.OperationWeather.Time);
        DataChanged?.Invoke(OperationSavingStatType.WeatherConditions, operationData.OperationWeather.Weather);

        OperationStarted?.Invoke(operationData);
    }

    public void StopOperation()
    {
        if (ClientTypeManager.CurrentClientType is not (ClientType.OperationSettings
                or ClientType.Game) && IsServer == false)
            return;

        StopOperationServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void StopOperationServerRpc()
    {
        if (_isOperationActive.Value == false)
            return;

        _isOperationActive.Value = false;
        _currentOperationTime.Value = 0f;
        _spawnedWavesCount.Value = 0;

        _operationCancellationToken.Cancel();
        _operationCancellationToken = new();

        foreach (EnemyController enemyController in _currentAliveEnemies)
        {
            Destroy(enemyController.gameObject);
        }

        _currentAliveEnemies.Clear();

        OperationUpdateManager.RequestUpdate();

        OperationStopped?.Invoke();
    }

    private void SpawnEnemy(EnemyType spawningEnemyType, int spawningEnemyCount,
        ReadonlyEnemyInitializationStats readonlyEnemyInitializationStats)
    {
        if (IsServer == false)
            return;

        for (int i = 0; i < spawningEnemyCount; i++)
        {
            EnemyInitializationStats enemyInitializationStats = new(readonlyEnemyInitializationStats);

            EnemyController newSpawningEnemy =
                _enemiesFactory.Create(spawningEnemyType, enemyInitializationStats);

            NetworkObject newSpawningEnemyNetworkObject = newSpawningEnemy.GetComponent<NetworkObject>();
            newSpawningEnemyNetworkObject.Spawn();

            _currentAliveEnemies.Add(newSpawningEnemy);
            newSpawningEnemy.HealthComponent.HealthDepleted += EnemyHealthComponent_OnHealthDepleted;

            _markersManager.AddMapMarker(newSpawningEnemy.transform, Vector2.zero, MarkerType.EnemyMarker);

            DataChanged?.Invoke(OperationSavingStatType.SpawnedEnemyType, spawningEnemyType);
        }

        OperationUpdateManager.RequestUpdate();
    }

    private void EnemyHealthComponent_OnHealthDepleted()
    {
        RemoveDeadEnemiesFromList();
    }

    private void RemoveDeadEnemiesFromList()
    {
        for (int i = 0; i < _currentAliveEnemies.Count; i++)
        {
            EnemyController enemyController = _currentAliveEnemies[i];

            DataChanged?.Invoke(OperationSavingStatType.KilledEnemyType, enemyController.EnemyType);

            if (enemyController.HealthComponent.IsDestroyed)
            {
                _currentAliveEnemies.RemoveAt(i);
                i--;
            }
        }
    }

    #endregion

    public void OnSceneReset()
    {
        if (IsServer == false)
            return;

        StopOperation();
    }

    public void Dispose()
    {
        _operationCancellationToken.Cancel();
    }
}