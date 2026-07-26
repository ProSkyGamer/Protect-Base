#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class OperationMapManagerUIObserver : IInitializable, IDisposable
{
    #region Events

    public event Action StartedListeningForMapPoint;

    public event Action<Vector2, Vector3> MapPointSet;
    public event Action CanceledListeningForMapPoint;

    #endregion

    #region Variables & References

    private OperationMapManagerUI _operationMapManagerUI;

    private MapListeningPointType _listeningPointType;
    private MarkerType _listeningMarkerType;
    private bool _isLastDisplayedPositionWithinBorders;
    private Vector2 _selectedPathPointPreviousPosition;
    private Vector2 _currentDisplayingPosition;
    private ObjectLimits _mapPointsLimits;

    private readonly List<IPathPointCoordinatesRequestor> _allCoordinatesRequestors = new();
    private readonly List<IPathUpdater> _allPathUpdaters = new();
    private readonly List<IMapZonesUpdater> _allMapZonesUpdaters = new();

    private ITerritoryInfoProvider _territoryInfoProvider;
    private OperationMapZonesManagerUI _mapZonesManagerUI;
    private OperationMapPathManagerUI _mapPathManagerUI;

    private OperationMapMarkersManagerUI _mapMarkersManagerUI;
    private MarkersManager _markersManager;

    private bool _isListeningForPathPoint;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(OperationMapManagerUI operationMapManagerUI, ITerritoryInfoProvider territoryInfoProvider,
        List<IPathPointCoordinatesRequestor> allPathCoordinatesRequestors,
        List<IPathUpdater> allPathUpdaters, List<IMapZonesUpdater> allMapZonesUpdaters, OperationMapZonesManagerUI mapZonesManagerUI,
        OperationMapPathManagerUI mapPathManagerUI, OperationMapMarkersManagerUI mapMarkersManagerUI, MarkersManager markersManager)
    {
        _operationMapManagerUI = operationMapManagerUI;
        _territoryInfoProvider = territoryInfoProvider;
        _mapZonesManagerUI = mapZonesManagerUI;
        _mapPathManagerUI = mapPathManagerUI;
        _mapMarkersManagerUI = mapMarkersManagerUI;
        _markersManager = markersManager;

        _allCoordinatesRequestors.AddRange(allPathCoordinatesRequestors);
        _allPathUpdaters.AddRange(allPathUpdaters);
        _allMapZonesUpdaters.AddRange(allMapZonesUpdaters);
    }

    private void StartListeningToMapPointPosition(MapListeningPointType mapListeningPointType,
        MarkerType newListeningMarkerType = MarkerType.FiringMachineMarker)
    {
        _isListeningForPathPoint = true;

        _listeningPointType = mapListeningPointType;
        _listeningMarkerType = newListeningMarkerType;

        switch (mapListeningPointType)
        {
            default:
            case MapListeningPointType.MapPoint:
                _selectedPathPointPreviousPosition = Vector2.zero;
                _operationMapManagerUI.UpdateCurrentPhantomPointPosition(_selectedPathPointPreviousPosition);

                StartedListeningForMapPoint?.Invoke();

                break;

            case MapListeningPointType.MarkerPoint:
                MapMarkerSingleUI markerSingle = _mapMarkersManagerUI.GetMarkerSingle(_listeningMarkerType);

                _mapMarkersManagerUI.StartListeningForMarkerPoint(_listeningMarkerType);

                _selectedPathPointPreviousPosition = markerSingle != null
                    ? markerSingle.transform.position
                    : Vector2.zero;

                _mapPathManagerUI.DisplayCurrentSelectedPointAdditionalDirectionLines(
                    _selectedPathPointPreviousPosition);

                break;
        }

        _mapZonesManagerUI.ChangeCurrentZonesDisplayStatus(false);
        _operationMapManagerUI.DisplayHint(_listeningPointType);
    }

    public void Initialize()
    {
        _mapPointsLimits = _territoryInfoProvider.GetMapLimits();

        _mapZonesManagerUI.ChangeCurrentZonesDisplayStatus(false);
        _mapZonesManagerUI.ClearCurrentZones();

        _operationMapManagerUI.HideAllHints();
        _operationMapManagerUI.HidePhantomPoint();

        _mapMarkersManagerUI.HideAllDisplayingAdditionalInfo();

        _operationMapManagerUI.BaseZonesDisplayed += OperationMapManagerUI_OnBaseZonesDisplayed;
        _operationMapManagerUI.InterfaceClicked += OperationMapManagerUIOnInterfaceClick;

        _markersManager.MarkerAdded += MarkersManager_OnMarkerAdded;
        _markersManager.MarkerRemoved += MarkersManager_OnMarkerRemoved;

        foreach (IPathPointCoordinatesRequestor pathPointCoordinatesRequestor in _allCoordinatesRequestors)
        {
            pathPointCoordinatesRequestor.RequestPathPoint += PathPointCoordinatesRequestor_OnRequestPathPoint;
            pathPointCoordinatesRequestor.CancelPathPointRequest += PathPointCoordinatesRequestor_OnCancelPathPointRequest;
        }

        foreach (IPathUpdater pathUpdater in _allPathUpdaters)
        {
            pathUpdater.UpdatePathVisuals += PathUpdater_OnUpdatePathVisuals;
        }

        foreach (IMapZonesUpdater mapZonesUpdater in _allMapZonesUpdaters)
        {
            mapZonesUpdater.UpdateMapZones += MapZonesUpdater_OnUpdateMapZones;
        }

        _mapMarkersManagerUI.MarkerSelected += MapMarkersManagerUI_OnMarkerSelected;
        _mapMarkersManagerUI.MarkerHover += MapMarkersManagerUI_OnMarkerHover;
        _mapMarkersManagerUI.MarkerStoppedHover += MapMarkersManagerUI_OnMarkerStoppedHover;
    }

    private void OperationMapManagerUIOnInterfaceClick()
    {
        if (_isListeningForPathPoint)
            TrySetPathPoint();
        else if (_mapMarkersManagerUI.IsDisplayingMarkerAdditionalInfo)
            _mapMarkersManagerUI.HideCurrentDisplayingMarkerAdditionalInfo();
    }

    private void OperationMapManagerUI_OnBaseZonesDisplayed()
    {
        if (_isListeningForPathPoint)
            return;

        _mapZonesManagerUI.ToggleZonesDisplayStatus();
    }

    private void MarkersManager_OnMarkerRemoved(Transform markerWorldObject)
    {
        _mapMarkersManagerUI.RemoveMarker(markerWorldObject);
    }

    private void MarkersManager_OnMarkerAdded(Vector2 markerPosition, MarkerType markerType, Transform markerWorldObject)
    {
        _mapMarkersManagerUI.AddMapMarker(markerPosition, markerType, markerWorldObject);
    }

    private void MapZonesUpdater_OnUpdateMapZones(List<CustomDisplayingZones> displayingZones)
    {
        _mapZonesManagerUI.DisplayZones(displayingZones);
    }

    private void PathUpdater_OnUpdatePathVisuals(IReadOnlyList<ReadonlyPathPoint> fullPath)
    {
        _mapPathManagerUI.DisplayCurrentEnemyPathPoints(fullPath);
        _mapPathManagerUI.UpdateCurrentDirectionLines();
    }

    private void PathPointCoordinatesRequestor_OnRequestPathPoint(MapListeningPointType mapListeningPointType, MarkerType markerType)
    {
        StartListeningToMapPointPosition(mapListeningPointType, markerType);
    }

    private void PathPointCoordinatesRequestor_OnCancelPathPointRequest()
    {
        TrySetPathPoint();
    }

    private void MapMarkersManagerUI_OnMarkerSelected(MapMarkerSingleUI selectedMarkerSingleUI)
    {
        if (_isListeningForPathPoint == false)
            return;

        bool isPointValid = _listeningPointType == MapListeningPointType.MarkerPoint &&
                            _listeningMarkerType == selectedMarkerSingleUI.MarkerType;

        if (isPointValid == false)
            return;

        ConfirmPathPoint(selectedMarkerSingleUI.transform.position, selectedMarkerSingleUI.MarkerWorldPointPosition);
    }

    private void MapMarkersManagerUI_OnMarkerHover(MapMarkerSingleUI selectedMarkerSingleUI)
    {
        if (_isListeningForPathPoint == false)
            return;

        if (selectedMarkerSingleUI == null)
            return;

        _mapPathManagerUI.DisplayCurrentSelectedPointAdditionalDirectionLines(selectedMarkerSingleUI.transform.position);
    }

    private void MapMarkersManagerUI_OnMarkerStoppedHover()
    {
        if (_isListeningForPathPoint == false)
            return;

        _mapPathManagerUI.DisplayCurrentSelectedPointAdditionalDirectionLines(_selectedPathPointPreviousPosition);
    }

    #endregion

    #region Set Path Point

    public void ChangeMousePosition(Vector2 newMousePosition)
    {
        if (_listeningPointType != MapListeningPointType.MapPoint)
            return;

        bool isPointWithinBoundaries = _mapPointsLimits.IsPointWithinBoundaries(newMousePosition);

        _isLastDisplayedPositionWithinBorders = isPointWithinBoundaries;
        _currentDisplayingPosition = isPointWithinBoundaries == false ? _selectedPathPointPreviousPosition : newMousePosition;

        _operationMapManagerUI.UpdateCurrentPhantomPointPosition(_currentDisplayingPosition);
        _mapPathManagerUI.DisplayCurrentSelectedPointAdditionalDirectionLines(_currentDisplayingPosition);
    }

    private void TrySetPathPoint()
    {
        if (_isListeningForPathPoint == false)
            return;

        if (_listeningPointType is not MapListeningPointType.MapPoint)
            return;

        if (_isLastDisplayedPositionWithinBorders)
            ConfirmPathPoint(_currentDisplayingPosition, Vector3.zero);
        else
            CancelListeningToPathPoint();
    }

    private void ConfirmPathPoint(Vector2 mapPointPosition, Vector3 worldPointPosition)
    {
        MapPointSet?.Invoke(mapPointPosition, worldPointPosition);

        StopListeningForPathPoint();
    }

    private void CancelListeningToPathPoint()
    {
        CanceledListeningForMapPoint?.Invoke();

        StopListeningForPathPoint();
    }

    private void StopListeningForPathPoint()
    {
        _isListeningForPathPoint = false;

        _mapZonesManagerUI.ClearCurrentZones();

        if (_listeningPointType == MapListeningPointType.MarkerPoint)
            _mapMarkersManagerUI.StopListeningForMarkerPoint();

        _operationMapManagerUI.HidePhantomPoint();
        _operationMapManagerUI.HideAllHints();
    }

    #endregion

    public void Dispose()
    {
        _operationMapManagerUI.BaseZonesDisplayed -= OperationMapManagerUI_OnBaseZonesDisplayed;
        _operationMapManagerUI.InterfaceClicked -= OperationMapManagerUIOnInterfaceClick;

        _markersManager.MarkerAdded -= MarkersManager_OnMarkerAdded;
        _markersManager.MarkerRemoved -= MarkersManager_OnMarkerRemoved;

        foreach (IPathPointCoordinatesRequestor pathPointCoordinatesRequestor in _allCoordinatesRequestors)
        {
            pathPointCoordinatesRequestor.RequestPathPoint -= PathPointCoordinatesRequestor_OnRequestPathPoint;
            pathPointCoordinatesRequestor.CancelPathPointRequest -= PathPointCoordinatesRequestor_OnCancelPathPointRequest;
        }

        foreach (IPathUpdater pathUpdater in _allPathUpdaters)
        {
            pathUpdater.UpdatePathVisuals -= PathUpdater_OnUpdatePathVisuals;
        }

        foreach (IMapZonesUpdater mapZonesUpdater in _allMapZonesUpdaters)
        {
            mapZonesUpdater.UpdateMapZones -= MapZonesUpdater_OnUpdateMapZones;
        }

        _mapMarkersManagerUI.MarkerSelected -= MapMarkersManagerUI_OnMarkerSelected;
        _mapMarkersManagerUI.MarkerHover -= MapMarkersManagerUI_OnMarkerHover;
        _mapMarkersManagerUI.MarkerStoppedHover -= MapMarkersManagerUI_OnMarkerStoppedHover;
    }
}