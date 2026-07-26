#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

#endregion

public class CurrentEditingOperationManager : ICurrentEditingOperationDataProvider, IInitializable
{
    #region Events

    public event Action CurrentOperationUpdated;

    #endregion

    #region Variables & References

    private OperationData _currentEditingOperationData = new();
    private readonly OperationTerritoryManager _operationTerritoryManager;
    private readonly EnemyBaseStatsSO _enemyBaseStatsSO;

    #endregion

    #region Initialization

    public CurrentEditingOperationManager(OperationTerritoryManager operationTerritoryManager, EnemyBaseStatsSO enemyBaseStatsSO)
    {
        _operationTerritoryManager = operationTerritoryManager;
        _enemyBaseStatsSO = enemyBaseStatsSO;
    }

    public void Initialize()
    {
        ResetCurrentEditingOperation();
    }

    #endregion

    #region Operation Editing

    public void SetWeatherConditions(ReadonlyWeatherActivationConditions readonlyWeatherActivationConditions)
    {
        _currentEditingOperationData.OperationWeather = readonlyWeatherActivationConditions;
    }

    public void SetCurrentEditingOperationSingle(ReadonlyOperationData operationSingle)
    {
        _currentEditingOperationData = new OperationData(operationSingle);

        CurrentOperationUpdated?.Invoke();
    }

    public void ResetCurrentEditingOperation()
    {
        ReadonlyWeatherActivationConditions operationWeather = _currentEditingOperationData.OperationWeather;

        _currentEditingOperationData = new OperationData
        {
            OperationWeather = operationWeather
        };

        CurrentOperationUpdated?.Invoke();
    }

    public void AddWaveToCurrentOperation(float spawnTime, EnemyType enemyType, int enemyCount,
        List<ReadonlyPathPoint> fullPath, float enemyAtk, float enemyHealth, float enemySpeed)
    {
        if (IsHasEnoughPathPoints(enemyType, fullPath) == false)
            return;

        int waveIndex = GetNewUnusedWaveNumber();

        OperationWave operationWave = FormOperationWave(waveIndex, spawnTime, enemyType, enemyCount,
            fullPath, enemyAtk, enemyHealth, enemySpeed);

        _currentEditingOperationData.AllOperationWaves.Add(operationWave);

        CurrentOperationUpdated?.Invoke();
    }

    public void AddWaveToCurrentOperation(float spawnTime, EnemyType enemyType, int enemyCount,
        List<ReadonlyPathPoint> fullPath, float enemyAtk, float enemyHealth, float enemySpeed, int enemySummonsCount)
    {
        if (IsHasEnoughPathPoints(enemyType, fullPath) == false)
            return;

        int waveIndex = GetNewUnusedWaveNumber();

        OperationWave operationWave = FormOperationWave(waveIndex, spawnTime, enemyType, enemyCount, fullPath,
            enemyAtk, enemyHealth, enemySpeed, enemySummonsCount);

        _currentEditingOperationData.AllOperationWaves.Add(operationWave);

        CurrentOperationUpdated?.Invoke();
    }

    public void RemoveWaveFromCurrentOperation(int waveIndex)
    {
        OperationWave removingWave =
            _currentEditingOperationData.AllOperationWaves.Find(operationWave => operationWave.WaveIndex == waveIndex);

        if (removingWave != null)
            _currentEditingOperationData.AllOperationWaves.Remove(removingWave);

        CurrentOperationUpdated?.Invoke();
    }

    public void EditWaveFromCurrentOperation(int waveIndex, float spawnTime, EnemyType enemyType, int enemyCount,
        List<ReadonlyPathPoint> fullPath, float enemyAtk, float enemyHealth, float enemySpeed)
    {
        if (IsHasEnoughPathPoints(enemyType, fullPath) == false)
            return;

        int editingWaveIndex = _currentEditingOperationData.AllOperationWaves.FindIndex(operationWave =>
            operationWave.WaveIndex == waveIndex);

        if (editingWaveIndex < 0)
            return;

        OperationWave newEditedOperationWave = FormOperationWave(waveIndex, spawnTime, enemyType,
            enemyCount, fullPath, enemyAtk, enemyHealth, enemySpeed);

        _currentEditingOperationData.AllOperationWaves[editingWaveIndex] = newEditedOperationWave;

        CurrentOperationUpdated?.Invoke();
    }

    public void EditWaveFromCurrentOperation(int waveIndex, float spawnTime, EnemyType enemyType, int enemyCount,
        List<ReadonlyPathPoint> fullPath, float enemyAtk, float enemyHealth, float enemySpeed, int enemySummonsCount)
    {
        if (IsHasEnoughPathPoints(enemyType, fullPath) == false)
            return;

        int editingWaveListIndex =
            _currentEditingOperationData.AllOperationWaves.FindIndex(operationWave =>
                operationWave.WaveIndex == waveIndex);

        if (editingWaveListIndex < 0)
            return;

        OperationWave newEditedOperationWave = FormOperationWave(waveIndex, spawnTime, enemyType,
            enemyCount,
            fullPath, enemyAtk, enemyHealth, enemySpeed, enemySummonsCount);

        _currentEditingOperationData.AllOperationWaves[editingWaveListIndex] = newEditedOperationWave;

        CurrentOperationUpdated?.Invoke();
    }

    #endregion

    #region Get

    private bool IsHasEnoughPathPoints(EnemyType enemyType, List<ReadonlyPathPoint> fullPath)
    {
        float minAllowedPathPoints =
            _enemyBaseStatsSO.GetMinEnemyPathPointsCount(enemyType);

        return fullPath.Count >= minAllowedPathPoints;
    }

    private List<ReadonlyPathPoint> GetCorrectedPath(List<ReadonlyPathPoint> correctingPath)
    {
        List<ReadonlyPathPoint> correctedPath = new();

        foreach (ReadonlyPathPoint pathPoint in correctingPath)
        {
            Vector2 screenCenteredMapPoint = pathPoint.ScreenCenteredMapPoint;
            Vector3 worldPoint = pathPoint.WorldPoint;
            PathPointType pathPointType = pathPoint.PathPointType;

            if (worldPoint == Vector3.zero)
                worldPoint = _operationTerritoryManager.GetWorldPointFromMapPoint(pathPoint.ScreenCenteredMapPoint, out bool _);
            else if (screenCenteredMapPoint == Vector2.zero)
                screenCenteredMapPoint = _operationTerritoryManager.GetMapPointFromWorldPoint(pathPoint.WorldPoint);

            ReadonlyPathPoint newPathPoint = new(screenCenteredMapPoint, pathPoint.MapCenteredMapPoint, worldPoint, pathPointType);

            correctedPath.Add(newPathPoint);
        }

        return correctedPath;
    }

    private OperationWave FormOperationWave(int waveIndex, float spawnTime, EnemyType enemyType, int enemyCount,
        List<ReadonlyPathPoint> fullPath, float enemyAtk, float enemyHealth, float enemySpeed)
    {
        fullPath = GetCorrectedPath(fullPath);

        ReadonlyEnemyInitializationStats enemyInitializationStats = new(fullPath, enemyHealth, enemyAtk, enemySpeed);

        OperationWave operationWave =
            new(waveIndex, spawnTime, enemyType, enemyCount, enemyInitializationStats);

        return operationWave;
    }

    private OperationWave FormOperationWave(int waveIndex, float spawnTime, EnemyType enemyType, int enemyCount,
        List<ReadonlyPathPoint> fullPath, float enemyAtk, float enemyHealth, float enemySpeed, int enemiesSummonsCount)
    {
        fullPath = GetCorrectedPath(fullPath);

        ReadonlyEnemyInitializationStats readonlyEnemyInitializationStats =
            new(fullPath, enemyHealth, enemyAtk, enemySpeed);

        readonlyEnemyInitializationStats =
            new VehicleInitializationStats(readonlyEnemyInitializationStats, enemiesSummonsCount);

        OperationWave newAddingOperationWave =
            new(waveIndex, spawnTime, enemyType, enemyCount, readonlyEnemyInitializationStats);

        _currentEditingOperationData.AllOperationWaves.Add(newAddingOperationWave);

        return newAddingOperationWave;
    }

    public ReadonlyOperationData GetCurrentEditingOperationSingle()
    {
        return _currentEditingOperationData;
    }

    private int GetNewUnusedWaveNumber()
    {
        int lastUsedWaveNumber = -1;

        if (_currentEditingOperationData.AllOperationWaves.Count > 0)
            lastUsedWaveNumber = _currentEditingOperationData.AllOperationWaves.Max(wave => wave.WaveIndex);

        int newWaveNumber = lastUsedWaveNumber + 1;

        return newWaveNumber;
    }

    public int GetTotalCurrentOperationWavesCount()
    {
        return _currentEditingOperationData.AllOperationWaves.Count;
    }

    #endregion
}