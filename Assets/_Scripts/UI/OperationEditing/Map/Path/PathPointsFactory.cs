#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

#endregion

public class PathPointsFactory
{
    private readonly DiContainer _container;
    private readonly Dictionary<PathPointType, PathPointSingle> _allPathPointsPrefabs;

    [Inject(Id = "PathPointsContainer")] private Transform _pathPointsContainer;

    public PathPointsFactory(DiContainer container, List<PathPointPrefab> pathPointLinesPrefabs)
    {
        _container = container;

        _allPathPointsPrefabs = pathPointLinesPrefabs.ToDictionary(pathPointPrefab => pathPointPrefab.PathPointType,
            pathPointPrefab => pathPointPrefab.PathPointTypePrefab);
    }

    public PathPointSingle Create(PathPointType pathPointType)
    {
        DiContainer subContainer = _container.CreateSubContainer();

        if (_allPathPointsPrefabs.TryGetValue(pathPointType, out PathPointSingle pathPointPrefab) == false)
        {
            Debug.LogError($"Not found prefab for enemy type: {pathPointType}");

            return null;
        }

        return subContainer.InstantiatePrefabForComponent<PathPointSingle>(pathPointPrefab, _pathPointsContainer);
    }
}