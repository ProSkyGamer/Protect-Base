#region

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

#endregion

public class MarkerSingle
{
    public MapMarkerSingleUI MarkerSingleUI { get; }

    public MarkerAdditionalInfoButtonUI MarkerAdditionalInfoButton { get; }

    public MarkerSingle(MapMarkerSingleUI mapMarkerSingleUI, MarkerAdditionalInfoButtonUI markerAdditionalInfoButton)
    {
        MarkerSingleUI = mapMarkerSingleUI;
        MarkerAdditionalInfoButton = markerAdditionalInfoButton;
    }
}

public class MarkersFactory
{
    private readonly Dictionary<MarkerType, MarkerSingleInstaller> _allMarkerPrefabs;
    private readonly DiContainer _container;

    [Inject(Id = "MarkersContainer")] private Transform _markersContainer;

    public MarkersFactory(DiContainer container, List<MapMarkerPrefab> markerPrefabs)
    {
        _allMarkerPrefabs = markerPrefabs.ToDictionary(markerPrefab => markerPrefab.MarkerType,
            markerPrefab => markerPrefab.MarkerSingleInstaller);

        _container = container;
    }

    public MarkerSingle Create(MarkerType markerType, Transform worldObject)
    {
        if (_allMarkerPrefabs.TryGetValue(markerType, out MarkerSingleInstaller markerPrefab) == false)
        {
            Debug.LogError($"No prefab for {markerType}");

            return null;
        }

        DiContainer subContainer = _container.CreateSubContainer();
        subContainer.BindInstance(markerType);
        subContainer.BindInstance(worldObject);

        MarkerSingleInstaller markerSingle = subContainer.InstantiatePrefabForComponent<MarkerSingleInstaller>(markerPrefab, _markersContainer);

        return markerSingle.MarkerSingle;
    }
}