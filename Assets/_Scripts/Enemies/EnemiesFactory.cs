#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

#endregion

public class EnemiesFactory
{
    private readonly DiContainer _container;
    private readonly Dictionary<EnemyType, EnemyPrefabInfo> _allEnemiesPrefabsInfo;
    private readonly DynamicInjector _dynamicInjector;

    public EnemiesFactory(DiContainer container, List<EnemyPrefabInfo> enemyPrefabInfos, DynamicInjector dynamicInjector)
    {
        _container = container;
        _dynamicInjector = dynamicInjector;

        _allEnemiesPrefabsInfo = enemyPrefabInfos.ToDictionary(enemyPrefab => enemyPrefab.EnemyType);
    }

    public EnemyController Create(EnemyType enemyType,
        EnemyInitializationStats enemyInitializationStats)
    {
        if (_allEnemiesPrefabsInfo.TryGetValue(enemyType, out EnemyPrefabInfo creatingEnemyPrefab) == false)
        {
            Debug.LogError($"Not found prefab for enemy type: {enemyType}");

            return null;
        }

        DiContainer subContainer = _container.CreateSubContainer();

        List<ReadonlyPathPoint> enemyPath = new();
        enemyPath.AddRange(enemyInitializationStats.FullPath);

        enemyPath = GetCorrectedEnemyPath(enemyPath, creatingEnemyPrefab.SpawnPointDispersion,
            creatingEnemyPrefab.PathPointDispersion, out Vector3 spawnPointPosition);

        enemyInitializationStats.FullPath.Clear();
        enemyInitializationStats.FullPath.AddRange(enemyPath);

        ReadonlyEnemyInitializationStats readonlyEnemyInitializationStats = enemyInitializationStats;

        subContainer.BindInstance(readonlyEnemyInitializationStats);

        EnemyController enemyController =
            subContainer.InstantiatePrefabForComponent<EnemyController>(creatingEnemyPrefab.EnemyController, spawnPointPosition, Quaternion.identity,
                null);

        _dynamicInjector.InjectAllInterfacesFrom(enemyController);

        return enemyController;
    }

    private List<ReadonlyPathPoint> GetCorrectedEnemyPath(
        List<ReadonlyPathPoint> fullEnemyPath, Vector3 spawnPointDispersion, Vector3 pathPointDispersion, out Vector3 spawnPointPosition)
    {
        List<ReadonlyPathPoint> newEnemyPath = new();
        newEnemyPath.AddRange(fullEnemyPath);

        Vector3 pathPointMinDispersion = pathPointDispersion;
        Vector3 pathPointMaxDispersion = -pathPointDispersion;
        Vector3 spawnPointMinDispersion = spawnPointDispersion;
        Vector3 spawnPointMaxDispersion = -spawnPointDispersion;

        List<ReadonlyPathPoint> enemyPath = new();

        foreach (ReadonlyPathPoint pathPoint in newEnemyPath)
        {
            Vector3 pointDispersion = Vector3.zero;

            if (pathPoint.PathPointType is not PathPointType.SpawnPathPoint)
                pointDispersion = new Vector3(
                    GetRandomValueFromRange(pathPointMinDispersion.x, pathPointMaxDispersion.x),
                    pathPointDispersion.y,
                    GetRandomValueFromRange(pathPointMinDispersion.z, pathPointMaxDispersion.z));
            else if (pathPoint.PathPointType is PathPointType.SpawnPathPoint)
                pointDispersion =
                    new Vector3(
                        GetRandomValueFromRange(spawnPointMinDispersion.x, spawnPointMaxDispersion.x),
                        spawnPointDispersion.y,
                        GetRandomValueFromRange(spawnPointMinDispersion.z, spawnPointMaxDispersion.z));

            ReadonlyPathPoint newAddingPoint = new(pathPoint.ScreenCenteredMapPoint, pathPoint.MapCenteredMapPoint,
                pathPoint.WorldPoint + pointDispersion, pathPoint.PathPointType);

            enemyPath.Add(newAddingPoint);
        }

        spawnPointPosition = enemyPath[0].WorldPoint;
        enemyPath.RemoveAt(0);

        return enemyPath;
    }

    private float GetRandomValueFromRange(float min, float max)
    {
        if (min > max)
            (min, max) = (max, min);

        return Random.Range(min, max);
    }
}