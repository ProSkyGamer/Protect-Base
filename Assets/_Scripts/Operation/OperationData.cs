#region

using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

#endregion

public class SavedOperationData : INetworkCustomSerializable
{
    public int OperationIndex => _operationIndex;
    private int _operationIndex;

    public ReadonlyOperationData OperationData => _operationData;

    private ReadonlyOperationData _operationData;

    public string OperationName { get; private set; }

    private FixedString512Bytes _operationNameFixed = "";

    public SavedOperationData()
    {
        _operationData = new ReadonlyOperationData();
    }

    [JsonConstructor]
    public SavedOperationData(int operationIndex, ReadonlyOperationData operationData, string operationName)
    {
        _operationIndex = operationIndex;
        OperationName = operationName;
        _operationData = operationData;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _operationIndex);
        serializer.SerializeValue(ref _operationData);
        serializer.SerializeValue(ref _operationNameFixed);
    }

    public void PackForNetworkTransfer()
    {
        _operationData.PackForNetworkTransfer();
        _operationNameFixed = string.IsNullOrEmpty(OperationName) ? "" : OperationName;
    }

    public void UnpackAfterNetworkTransfer()
    {
        _operationData.UnpackAfterNetworkTransfer();
        OperationName = _operationNameFixed.Value;
    }
}

public class OperationData : ReadonlyOperationData
{
    public new List<OperationWave> AllOperationWaves
    {
        get => AllWaves;
        private set => AllWaves = value;
    }

    public new ReadonlyWeatherActivationConditions OperationWeather
    {
        get => OperationWeatherConditions;
        set => OperationWeatherConditions = value;
    }

    public OperationData()
    {
        AllOperationWaves = new List<OperationWave>();
        OperationWeatherConditions = new ReadonlyWeatherActivationConditions();
    }

    [JsonConstructor]
    public OperationData(List<OperationWave> allOperationWaves, ReadonlyWeatherActivationConditions operationWeather)
    {
        AllOperationWaves = allOperationWaves;
        OperationWeather = operationWeather;
    }

    public OperationData(ReadonlyOperationData readonlyOperationData)
    {
        AllOperationWaves = new List<OperationWave>();

        AllOperationWaves.AddRange(readonlyOperationData.AllOperationWaves);
        OperationWeatherConditions = readonlyOperationData.OperationWeather;
    }
}

public class ReadonlyOperationData : INetworkCustomSerializable
{
    public IReadOnlyList<OperationWave> AllOperationWaves => AllWaves;

    protected List<OperationWave> AllWaves;

    private OperationWave[] _allOperationWavesArray;

    public ReadonlyWeatherActivationConditions OperationWeather => OperationWeatherConditions;

    protected ReadonlyWeatherActivationConditions OperationWeatherConditions;

    public ReadonlyOperationData()
    {
        AllWaves = new List<OperationWave>();
        OperationWeatherConditions = new ReadonlyWeatherActivationConditions();
    }

    [JsonConstructor]
    public ReadonlyOperationData(List<OperationWave> allWaves,
        ReadonlyWeatherActivationConditions readonlyWeatherConditions)
    {
        AllWaves = allWaves;
        OperationWeatherConditions = readonlyWeatherConditions;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _allOperationWavesArray);
        serializer.SerializeValue(ref OperationWeatherConditions);
    }

    public void PackForNetworkTransfer()
    {
        OperationWeatherConditions.PackForNetworkTransfer();

        foreach (OperationWave operationWave in AllOperationWaves)
        {
            operationWave.PackForNetworkTransfer();
        }

        _allOperationWavesArray = new OperationWave[AllOperationWaves.Count];

        for (int i = 0; i < AllOperationWaves.Count; i++)
        {
            OperationWave operationWave = AllOperationWaves[i];
            _allOperationWavesArray[i] = operationWave;
        }
    }

    public void UnpackAfterNetworkTransfer()
    {
        OperationWeatherConditions.UnpackAfterNetworkTransfer();

        AllWaves.AddRange(_allOperationWavesArray);

        foreach (OperationWave operationWave in AllOperationWaves)
        {
            operationWave.UnpackAfterNetworkTransfer();
        }
    }
}

public class OperationWave : INetworkCustomSerializable
{
    public int WaveIndex => _waveIndex;

    private int _waveIndex;

    public float WaveSpawnTime => _waveSpawnTime;

    private float _waveSpawnTime;

    public EnemyType SpawningEnemyType => _spawningEnemyType;

    private EnemyType _spawningEnemyType;

    public int SpawningEnemyCount => _spawningEnemyCount;

    private int _spawningEnemyCount;

    public ReadonlyEnemyInitializationStats ReadonlyEnemyInitializationStats => _readonlyEnemyInitializationStats;

    private ReadonlyEnemyInitializationStats _readonlyEnemyInitializationStats;

    public OperationWave()
    {
        _readonlyEnemyInitializationStats = new ReadonlyEnemyInitializationStats();
    }

    [JsonConstructor]
    public OperationWave(int waveIndex, float waveSpawnTime, EnemyType spawningEnemyType,
        int spawningEnemyCount,
        ReadonlyEnemyInitializationStats readonlyEnemyInitializationStats)
    {
        _waveIndex = waveIndex;
        _waveSpawnTime = waveSpawnTime;
        _spawningEnemyType = spawningEnemyType;
        _spawningEnemyCount = spawningEnemyCount;
        _readonlyEnemyInitializationStats = readonlyEnemyInitializationStats;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _waveIndex);
        serializer.SerializeValue(ref _waveSpawnTime);
        serializer.SerializeValue(ref _spawningEnemyType);
        serializer.SerializeValue(ref _spawningEnemyCount);
        serializer.SerializeValue(ref _readonlyEnemyInitializationStats);
    }

    public void PackForNetworkTransfer()
    {
        ReadonlyEnemyInitializationStats.PackForNetworkTransfer();
    }

    public void UnpackAfterNetworkTransfer()
    {
        ReadonlyEnemyInitializationStats.UnpackAfterNetworkTransfer();
    }
}

public class ReadonlyPathPoint : INetworkCustomSerializable
{
    public Vector2 ScreenCenteredMapPoint => _screenCenteredMapPoint;

    private Vector2 _screenCenteredMapPoint;

    /// <summary>
    ///     Позиция точки на карте относительно карты (ноль - в углу карты, а не углу экрана)
    /// </summary>
    public Vector2 MapCenteredMapPoint => _mapCenteredMapPoint;

    private Vector2 _mapCenteredMapPoint;
    public Vector3 WorldPoint => _worldPoint;

    private Vector3 _worldPoint;

    public PathPointType PathPointType => _pathPointType;

    private PathPointType _pathPointType;

    public ReadonlyPathPoint()
    {
    }

    [JsonConstructor]
    public ReadonlyPathPoint(Vector2 screenCenteredMapPoint, Vector2 mapCenteredMapPoint, Vector3 worldPoint,
        PathPointType pathPointType)
    {
        _screenCenteredMapPoint = screenCenteredMapPoint;
        _mapCenteredMapPoint = mapCenteredMapPoint;
        _worldPoint = worldPoint;
        _pathPointType = pathPointType;
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _screenCenteredMapPoint);
        serializer.SerializeValue(ref _mapCenteredMapPoint);
        serializer.SerializeValue(ref _worldPoint);
        serializer.SerializeValue(ref _pathPointType);
    }

    public void PackForNetworkTransfer()
    {
    }

    public void UnpackAfterNetworkTransfer()
    {
    }
}