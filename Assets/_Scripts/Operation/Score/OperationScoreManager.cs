#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class OperationScoreManager : IInitializable, IOperationsStatusListener, IDisposable
{
    #region Variables & References

    private readonly List<IOperationStatsDataProvider> _allOperationStatsDataProviders = new();
    private readonly Dictionary<OperationSavingStatType, object> _operationSavingData = new();
    private readonly DynamicInjector _dynamicInjector;

    #endregion

    #region Initialization

    public OperationScoreManager(List<IOperationStatsDataProvider> allOperationStatsDataProviders, DynamicInjector dynamicInjector)
    {
        _dynamicInjector = dynamicInjector;
        _allOperationStatsDataProviders.AddRange(allOperationStatsDataProviders);

        _dynamicInjector.InterfaceInjected += DynamicInjector_OnInterfaceInjected;
    }

    private void DynamicInjector_OnInterfaceInjected(Type injectingType, object injectingObject)
    {
        if (injectingType != typeof(IOperationStatsDataProvider))
            return;

        StartListeningTo(injectingObject as IOperationStatsDataProvider);
    }

    public void Initialize()
    {
        foreach (IOperationStatsDataProvider operationStatsDataProvider in _allOperationStatsDataProviders)
        {
            StartListeningTo(operationStatsDataProvider);
        }
    }

    private void StartListeningTo(IOperationStatsDataProvider operationStatsDataProvider)
    {
        operationStatsDataProvider.DataChanged += OperationStatsDataProvider_OnDataChanged;
    }

    private void OperationStatsDataProvider_OnDataChanged(OperationSavingStatType statType, object statValue)
    {
        AddSavingData(statType, statValue);
    }

    private void AddSavingData(OperationSavingStatType statType, object statValue)
    {
        _operationSavingData.TryGetValue(statType, out object currentSavedData);

        switch (statType)
        {
            default:
            case OperationSavingStatType.FiringMachineMaxHealth:
            case OperationSavingStatType.FiringMachineDamageTaken:
            case OperationSavingStatType.EnemiesMaxHealth:
            case OperationSavingStatType.EnemiesDamageTaken:
                currentSavedData = currentSavedData != null ? (float)currentSavedData : 0f + (float)statValue;

                break;

            case OperationSavingStatType.FiringMachineDestroyed:
            case OperationSavingStatType.FiringMachineRegularShotInitiated:
            case OperationSavingStatType.FiringMachineRegularShotHit:
            case OperationSavingStatType.FiringMachineExplosiveShotInitiated:
            case OperationSavingStatType.FiringMachineExplosiveShotHit:
                currentSavedData = (currentSavedData != null ? (int)currentSavedData : 0) + (int)statValue;

                break;

            case OperationSavingStatType.WeatherSeason:
            case OperationSavingStatType.WeatherTime:
            case OperationSavingStatType.WeatherConditions:
                currentSavedData = statValue;

                break;

            case OperationSavingStatType.SpawnedEnemyType:
            case OperationSavingStatType.KilledEnemyType:
                Dictionary<EnemyType, int> currentEnemies = currentSavedData as Dictionary<EnemyType, int> ?? new();

                EnemyType enemyType = (EnemyType)statValue;

                currentEnemies.TryGetValue(enemyType, out int countedEnemies);
                countedEnemies += 1;

                if (currentEnemies.TryAdd(enemyType, countedEnemies) == false)
                    currentEnemies[enemyType] = countedEnemies;

                currentSavedData = currentEnemies;

                break;
        }

        if (_operationSavingData.TryAdd(statType, currentSavedData) == false)
            _operationSavingData[statType] = currentSavedData;
    }

    public void OperationStarted()
    {
        _operationSavingData.Clear();
    }

    public void OperationEnded()
    {
        // TODO send message with score via TCP
    }

    #endregion

    #region Get

    public string GetOperationDataString()
    {
        string operationDataString = "{ ";

        foreach (KeyValuePair<OperationSavingStatType, object> operationData in _operationSavingData)
        {
            operationDataString += GetDataSingleString(operationData.Key, operationData.Value);
            operationDataString += ",";
        }

        operationDataString += "}";

        Debug.Log($"[OperationScoreManager.GetOperationDataString Line 104] {operationDataString}");

        return operationDataString;
    }

    private string GetDataSingleString(OperationSavingStatType statType, object statValue)
    {
        string dataSingleString;

        switch (statType)
        {
            default:
            case OperationSavingStatType.FiringMachineMaxHealth:
            case OperationSavingStatType.FiringMachineDamageTaken:
            case OperationSavingStatType.EnemiesMaxHealth:
            case OperationSavingStatType.EnemiesDamageTaken:
            case OperationSavingStatType.FiringMachineDestroyed:
            case OperationSavingStatType.FiringMachineRegularShotInitiated:
            case OperationSavingStatType.FiringMachineRegularShotHit:
            case OperationSavingStatType.FiringMachineExplosiveShotInitiated:
            case OperationSavingStatType.FiringMachineExplosiveShotHit:
                dataSingleString = $"{{{statType} : {(int)(statValue ?? 0)}}}";

                break;

            case OperationSavingStatType.WeatherSeason:
                dataSingleString = $"{{{statType} : {((WeatherActivationSeason)statValue).ToString()}}}";

                break;

            case OperationSavingStatType.WeatherTime:
                dataSingleString = $"{{{statType} : {((WeatherActivationTime)statValue).ToString()}}}";

                break;

            case OperationSavingStatType.WeatherConditions:
                dataSingleString = $"{{{statType} : {((WeatherActivationCondition)statValue).ToString()}}}";

                break;

            case OperationSavingStatType.SpawnedEnemyType:
            case OperationSavingStatType.KilledEnemyType:
                Dictionary<EnemyType, int> currentEnemies = statValue as Dictionary<EnemyType, int> ?? new();

                dataSingleString = "{ ";
                dataSingleString += statType == OperationSavingStatType.SpawnedEnemyType ? "Spawned" : "Killed";
                dataSingleString += ": ";

                foreach (KeyValuePair<EnemyType, int> enemiesCountPair in currentEnemies)
                {
                    dataSingleString += $"{enemiesCountPair.Key.ToString()} : {enemiesCountPair.Value.ToString()},";
                }

                dataSingleString = dataSingleString.Remove(dataSingleString.Length - 1);
                dataSingleString += "}";

                break;
        }

        return dataSingleString;
    }

    #endregion

    public void Dispose()
    {
        _dynamicInjector.InterfaceInjected -= DynamicInjector_OnInterfaceInjected;

        foreach (IOperationStatsDataProvider operationStatsDataProvider in _allOperationStatsDataProviders)
        {
            if (operationStatsDataProvider == null)
                continue;

            operationStatsDataProvider.DataChanged -= OperationStatsDataProvider_OnDataChanged;
        }
    }
}