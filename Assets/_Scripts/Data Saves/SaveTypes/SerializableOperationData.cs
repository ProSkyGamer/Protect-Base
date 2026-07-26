#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

public class SerializableSavedOperationData
{
    public int OperationIndex;
    public string OperationName;
    public SerializableOperationData OperationData;

    public SerializableSavedOperationData()
    {
    }

    public SerializableSavedOperationData(SavedOperationData savedOperationData)
    {
        OperationIndex = savedOperationData.OperationIndex;
        OperationName = savedOperationData.OperationName;

        OperationData = new(savedOperationData.OperationData.AllOperationWaves,
            savedOperationData.OperationData.OperationWeather);
    }

    public SavedOperationData GetOperationData()
    {
        SavedOperationData savedOperationData = new SavedOperationData(OperationIndex, OperationData.GetOperationData(), OperationName);

        return savedOperationData;
    }
}

public class SerializableOperationData
{
    public List<SerializableOperationWave> AllOperationWaves;
    public SerializableWeatherConditions OperationWeather;

    public SerializableOperationData()
    {
    }

    public SerializableOperationData(IReadOnlyList<OperationWave> allOperationWaves, ReadonlyWeatherActivationConditions operationWeather)
    {
        AllOperationWaves = allOperationWaves.Select(operationWave => new SerializableOperationWave(operationWave)).ToList();
        OperationWeather = new(operationWeather);
    }

    public ReadonlyOperationData GetOperationData()
    {
        List<OperationWave> allOperationWaves = AllOperationWaves.Select(operationWave => operationWave.GetOperationWave()).ToList();

        ReadonlyOperationData readonlyOperationData = new ReadonlyOperationData(allOperationWaves, OperationWeather.GetWeatherConditions());

        return readonlyOperationData;
    }
}

public class SerializableWeatherConditions
{
    public WeatherActivationSeason Season;
    public WeatherActivationTime Time;
    public WeatherActivationCondition Weather;

    public SerializableWeatherConditions()
    {
    }

    public SerializableWeatherConditions(ReadonlyWeatherActivationConditions readonlyWeatherActivationConditions)
    {
        Season = readonlyWeatherActivationConditions.Season;
        Time = readonlyWeatherActivationConditions.Time;
        Weather = readonlyWeatherActivationConditions.Weather;
    }

    public ReadonlyWeatherActivationConditions GetWeatherConditions()
    {
        ReadonlyWeatherActivationConditions weatherConditions = new ReadonlyWeatherActivationConditions(Season, Time, Weather);

        return weatherConditions;
    }
}

public class SerializableOperationWave
{
    public int WaveIndex;
    public float WaveSpawnTime;
    public EnemyType SpawningEnemyType;
    public int SpawningEnemyCount;
    public SerializableEnemyInitializationStats ReadonlyEnemyInitializationStats;

    public SerializableOperationWave()
    {
    }

    public SerializableOperationWave(OperationWave operationWave)
    {
        WaveIndex = operationWave.WaveIndex;
        WaveSpawnTime = operationWave.WaveSpawnTime;
        SpawningEnemyType = operationWave.SpawningEnemyType;
        SpawningEnemyCount = operationWave.SpawningEnemyCount;
        ReadonlyEnemyInitializationStats = new(operationWave.ReadonlyEnemyInitializationStats);
    }

    public OperationWave GetOperationWave()
    {
        OperationWave operationWave = new OperationWave(WaveIndex, WaveSpawnTime, SpawningEnemyType, SpawningEnemyCount,
            ReadonlyEnemyInitializationStats.GetEnemyInitializationStats());

        return operationWave;
    }
}

public class SerializableEnemyInitializationStats
{
    public List<SerializablePathPoint> FullPath;
    public float MaxHealth;
    public float BaseAtk;
    public float BaseSpeed;

    public SerializableEnemyInitializationStats()
    {
    }

    public SerializableEnemyInitializationStats(ReadonlyEnemyInitializationStats enemyInitializationStats)
    {
        MaxHealth = enemyInitializationStats.MaxHealth;
        BaseAtk = enemyInitializationStats.BaseAtk;
        BaseSpeed = enemyInitializationStats.BaseSpeed;

        FullPath = enemyInitializationStats.FullPath.Select(pathPoint => new SerializablePathPoint(pathPoint)).ToList();
    }

    public ReadonlyEnemyInitializationStats GetEnemyInitializationStats()
    {
        List<ReadonlyPathPoint> fullPath = FullPath.Select(pathPoint => pathPoint.GetPathPoint()).ToList();
        ReadonlyEnemyInitializationStats enemyInitializationStats = new(fullPath, MaxHealth, BaseAtk, BaseSpeed);

        return enemyInitializationStats;
    }
}

public class SerializablePathPoint
{
    public Vector2 ScreenCenteredMapPoint;
    public Vector2 MapCenteredMapPoint;
    public Vector3 WorldPoint;
    public PathPointType PathPointType;

    public SerializablePathPoint()
    {
    }

    public SerializablePathPoint(ReadonlyPathPoint pathPoint)
    {
        ScreenCenteredMapPoint = pathPoint.ScreenCenteredMapPoint;
        MapCenteredMapPoint = pathPoint.MapCenteredMapPoint;
        WorldPoint = pathPoint.WorldPoint;
        PathPointType = pathPoint.PathPointType;
    }

    public ReadonlyPathPoint GetPathPoint()
    {
        ReadonlyPathPoint pathPoint = new ReadonlyPathPoint(ScreenCenteredMapPoint, MapCenteredMapPoint, WorldPoint, PathPointType);

        return pathPoint;
    }
}