#region

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class EnemyPathWaveTabInfo : WaveTabInfo, IPathPointCoordinatesRequestor, IPathPointCoordinatesListener, IPathUpdater, IMapZonesUpdater
{
    #region Events

    public event Action<IReadOnlyList<ReadonlyPathPoint>> UpdatePathVisuals;
    public event Action<List<CustomDisplayingZones>> UpdateMapZones;

    public event Action<MapListeningPointType, MarkerType> RequestPathPoint;
    public event Action CancelPathPointRequest;

    #endregion

    #region Variables & References

    [SerializeField] private Button _addPathPointButton;
    [SerializeField] private Button _clearPathPointsButton;
    [SerializeField] private Transform _finalPointContainer;
    [SerializeField] private TextMeshProUGUI _totalPathPointsText;

    private ObjectLimits _mapCenteredPointLimits;
    private ObjectLimits _screenCenteredPointLimits;

    private bool _isOperationActive;
    private EnemyType _currentEnemyType;

    private readonly List<PathPointSingleUI> _allEnemyPathPointsSingle = new();
    private PathPointSingleUI _selectedPathPointSingle;

    private int _currentEnemyTypeMaxPathPoints;
    private PathPointSingleUI _finalPoint;

    private ITerritoryInfoProvider _territoryInfoProvider;
    private PathPointSingleUIFactory _pathPointSingleUIFactory;
    private EnemyBaseStatsSO _enemyBaseStatsSO;

    private bool IsCanAddPathPoint => _currentEnemyTypeMaxPathPoints < 0 || _allEnemyPathPointsSingle.Count < _currentEnemyTypeMaxPathPoints;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ITerritoryInfoProvider territoryInfoProvider, PathPointSingleUIFactory pathPointSingleUIFactory,
        EnemyBaseStatsSO enemyBaseStatsSO)
    {
        _territoryInfoProvider = territoryInfoProvider;
        _pathPointSingleUIFactory = pathPointSingleUIFactory;
        _enemyBaseStatsSO = enemyBaseStatsSO;
    }

    public override void OperationStarted()
    {
        _isOperationActive = true;

        BlockTabInteractions();
        UpdateTabVisuals();
    }

    public override void OperationEnded()
    {
        _isOperationActive = false;

        UnblockTabInteractions();
        UpdateTabVisuals();
    }

    public override void Initialize()
    {
        base.Initialize();

        _screenCenteredPointLimits = _territoryInfoProvider.GetMapLimits();

        _mapCenteredPointLimits = new ObjectLimits(GetMapCenteredPointPosition(_screenCenteredPointLimits.MinPoint),
            GetMapCenteredPointPosition(_screenCenteredPointLimits.MaxPoint));

        SubscribeToUIEvents();

        _finalPoint = _pathPointSingleUIFactory.Create(true, _mapCenteredPointLimits, -1);
        _finalPoint.transform.SetParent(_finalPointContainer);
        _finalPoint.SetType(PathPointType.FinalDestinationPoint);

        UpdateTabVisuals();
    }

    private void SubscribeToUIEvents()
    {
        _addPathPointButton.onClick.AddListener(AddPathPointButtonClicked);

        _clearPathPointsButton.onClick.AddListener(ClearPathButtonClicked);
    }

    private void ClearPathButtonClicked()
    {
        ClearPathData();

        UpdateTabVisuals();
        UpdateFinalPoint();

        UpdatePathVisuals?.Invoke(GetPathPoints());
    }

    private void AddPathPointButtonClicked()
    {
        AddPathPoint(true);
        UpdateTabVisuals();

        UpdatePathVisuals?.Invoke(GetPathPoints());
    }

    public void PathPointCoordinatesSelected(Vector2 screenCenteredPosition, Vector3 worldPointPosition)
    {
        if (IsCanAssignPoint(_selectedPathPointSingle.PointIndex, screenCenteredPosition))
            _selectedPathPointSingle.SetTrustedPosition(screenCenteredPosition);

        UpdateSelectedPointCoordinatesTo(screenCenteredPosition, worldPointPosition);

        FixSelectedPointCoordinated();

        bool isSelectedPointLast = _selectedPathPointSingle == _allEnemyPathPointsSingle[^1];

        if (isSelectedPointLast)
            UpdateFinalPoint();

        _selectedPathPointSingle.Deselect();
        _selectedPathPointSingle = null;

        UpdatePathPointsTypes();

        UpdatePathVisuals?.Invoke(GetPathPoints());
    }

    public void PathPointCoordinatesSelectionCanceled()
    {
        if (_selectedPathPointSingle == null)
            return;

        FixSelectedPointCoordinated();

        _selectedPathPointSingle.Deselect();
        _selectedPathPointSingle = null;

        UpdatePathVisuals?.Invoke(GetPathPoints());
    }

    #endregion

    #region Wave Tab

    public override void SoftResetTabInfo(EnemyType enemyType)
    {
        _currentEnemyType = enemyType;
        _currentEnemyTypeMaxPathPoints = _enemyBaseStatsSO.GetMaxEnemyPathPoints(_currentEnemyType);

        ClearPathData();

        UpdateTabVisuals();
        UpdateFinalPoint();

        UpdatePathVisuals?.Invoke(GetPathPoints());

        base.SoftResetTabInfo(enemyType);
    }

    public override void HardResetTabInfo()
    {
        SoftResetTabInfo(_currentEnemyType);
    }

    public override void CancelCurrentActions()
    {
        if (_selectedPathPointSingle == null)
            return;

        CancelPathPointRequest?.Invoke();
        UpdatePathVisuals?.Invoke(GetPathPoints());
    }

    private void BlockTabInteractions()
    {
        foreach (PathPointSingleUI pathPointSingle in _allEnemyPathPointsSingle)
        {
            pathPointSingle.ChangeBlockedState(true);
        }

        _addPathPointButton.interactable = false;
        _clearPathPointsButton.interactable = false;
    }

    private void UnblockTabInteractions()
    {
        foreach (PathPointSingleUI pathPointSingle in _allEnemyPathPointsSingle)
        {
            pathPointSingle.ChangeBlockedState(false);
        }

        _addPathPointButton.interactable = true;
        _clearPathPointsButton.interactable = true;
    }

    public override void SetWaveData(OperationWave operationWave)
    {
        ClearPathData();

        foreach (ReadonlyPathPoint enemyPathPoint in operationWave.ReadonlyEnemyInitializationStats.FullPath)
        {
            if (enemyPathPoint.PathPointType != PathPointType.FinalDestinationPoint)
            {
                AddPathPoint(enemyPathPoint);
            }
            else
            {
                _finalPoint.gameObject.SetActive(true);

                _finalPoint.SetTrustedPosition(enemyPathPoint.ScreenCenteredMapPoint);
                _finalPoint.SetPoint(enemyPathPoint.MapCenteredMapPoint, enemyPathPoint.ScreenCenteredMapPoint, enemyPathPoint.WorldPoint);
            }
        }

        UpdatePathIndexes();
        UpdatePathPointsTypes();
        UpdateTabVisuals();

        UpdatePathVisuals?.Invoke(GetPathPoints());
    }

    private void UpdateTabVisuals()
    {
        _addPathPointButton.interactable = !_isOperationActive && IsCanAddPathPoint;

        _totalPathPointsText.text = _allEnemyPathPointsSingle.Count.ToString();
    }

    #endregion

    #region Add Path Point

    private void AddPathPoint(ReadonlyPathPoint pathPoint)
    {
        if (IsCanAddPathPoint == false)
            return;

        PathPointSingleUI newPathPoint = AddPathPoint(false);

        bool isCanAssignPoint = IsCanAssignPoint(newPathPoint.PointIndex, pathPoint.ScreenCenteredMapPoint);

        if (isCanAssignPoint == false)
            return;

        newPathPoint.SetTrustedPosition(pathPoint.ScreenCenteredMapPoint);
        newPathPoint.SetType(pathPoint.PathPointType);
        newPathPoint.SetPoint(pathPoint.MapCenteredMapPoint, pathPoint.ScreenCenteredMapPoint, pathPoint.WorldPoint);
    }

    private PathPointSingleUI AddPathPoint(bool isSelecting)
    {
        if (IsCanAddPathPoint == false)
            return null;

        int addingPointIndex = _allEnemyPathPointsSingle.Count;

        PathPointSingleUI newPoint = _pathPointSingleUIFactory.Create(_isOperationActive, _mapCenteredPointLimits, addingPointIndex);
        _allEnemyPathPointsSingle.Add(newPoint);

        UpdatePathIndexes();

        PathPointType newPointType = addingPointIndex == 0 ? PathPointType.SpawnPathPoint : PathPointType.DefaultPathPoint;
        newPoint.SetType(newPointType);

        newPoint.PathPointDeleted += NewPathPoint_OnPathPointDeleted;
        newPoint.PathPointSelected += NewPathPoint_OnPathPointSelected;

        if (isSelecting)
        {
            _selectedPathPointSingle = newPoint;
            newPoint.Select();
        }

        return newPoint;
    }

    private void NewPathPoint_OnPathPointDeleted(PathPointSingleUI pathPointSingle)
    {
        if (_allEnemyPathPointsSingle.Contains(pathPointSingle) == false)
            return;

        Debug.Log("deleted");

        _allEnemyPathPointsSingle.Remove(pathPointSingle);

        if (_selectedPathPointSingle != null)
        {
            _selectedPathPointSingle.Deselect();
            _selectedPathPointSingle = null;
        }

        UpdatePathIndexes();
        UpdatePathPointsTypes();
        UpdateTabVisuals();
        UpdateFinalPoint();

        UpdatePathVisuals?.Invoke(GetPathPoints());
    }

    private void NewPathPoint_OnPathPointSelected(PathPointSingleUI pathPointSingle)
    {
        _selectedPathPointSingle = pathPointSingle;

        int pathPointIndex = pathPointSingle.PointIndex;
        MapListeningPointType mapListeningPointType = GetMapListeningPointType(pathPointIndex);

        UpdatePathVisuals?.Invoke(GetPathPoints());

        RequestPathPoint?.Invoke(mapListeningPointType, MarkerType.FiringMachineMarker);

        List<CustomDisplayingZones> availableSpawnZones = GetDisplayingZones(pathPointSingle, pathPointIndex);

        UpdateMapZones?.Invoke(availableSpawnZones);
    }

    private List<CustomDisplayingZones> GetDisplayingZones(PathPointSingleUI pathPointSingle, int pathPointIndex)
    {
        List<CustomDisplayingZones> availableSpawnZones = new();
        List<OperationTerritoryType> allZoneTypesList = Enum.GetValues(typeof(OperationTerritoryType)).Cast<OperationTerritoryType>().ToList();

        if (pathPointSingle.PointIndex == 0)
        {
            List<OperationTerritoryType> availableToSpawnZoneTypes = _territoryInfoProvider.GetAvailableSpawnZoneTypes(_currentEnemyType);

            availableSpawnZones.Add(new CustomDisplayingZones()
            {
                DisplayingZoneTypes = availableToSpawnZoneTypes,
                ZoneAvailabilityType = ZoneAvailabilityType.Available
            });

            availableSpawnZones.Add(new CustomDisplayingZones()
            {
                DisplayingZoneTypes = allZoneTypesList.Where(zoneType => availableToSpawnZoneTypes.Contains(zoneType) == false).ToList(),
                ZoneAvailabilityType = ZoneAvailabilityType.NotAvailable
            });
        }
        else
        {
            List<OperationTerritoryType> allForbiddenEnemyZones = _territoryInfoProvider.GetForbiddenEnemiesZoneTypes(_currentEnemyType);

            List<OperationTerritoryType> availableZonePathPoints =
                allZoneTypesList.Where(zoneType => allForbiddenEnemyZones.Contains(zoneType) == false).ToList();

            if (_currentEnemyType is EnemyType.Vehicle)
            {
                ZoneAvailabilityType availabilityZoneType =
                    IsHasDisembarkPoint(out int disembarkPointIndex) && pathPointIndex >= disembarkPointIndex ? ZoneAvailabilityType.Available :
                    pathPointIndex > 1 ? ZoneAvailabilityType.AvailableWithRestrictions : ZoneAvailabilityType.NotAvailable;

                availableSpawnZones.Add(new CustomDisplayingZones
                {
                    DisplayingZoneTypes = allForbiddenEnemyZones,
                    ZoneAvailabilityType = availabilityZoneType
                });
            }
            else
            {
                availableSpawnZones.Add(new CustomDisplayingZones
                {
                    DisplayingZoneTypes = allForbiddenEnemyZones,
                    ZoneAvailabilityType = ZoneAvailabilityType.NotAvailable
                });
            }

            availableSpawnZones.Add(new CustomDisplayingZones
            {
                DisplayingZoneTypes = availableZonePathPoints,
                ZoneAvailabilityType = ZoneAvailabilityType.Available
            });
        }

        return availableSpawnZones;
    }

    #endregion

    #region Update Path Points

    private void FixSelectedPointCoordinated()
    {
        int currentPathPointIndex = _selectedPathPointSingle.PointIndex;
        ReadonlyPathPoint enemyPathPoint = _selectedPathPointSingle.GetPathPoint();

        bool isCanAssignPoint = IsCanAssignPoint(currentPathPointIndex, enemyPathPoint.ScreenCenteredMapPoint);

        if (isCanAssignPoint == false)
            _selectedPathPointSingle.ResetPointToTrusted();
    }

    private void UpdateSelectedPointCoordinatesTo(Vector2 screenCenteredMapPoint, Vector3 worldPoint)
    {
        if (worldPoint != Vector3.zero && screenCenteredMapPoint == Vector2.zero)
            screenCenteredMapPoint = _territoryInfoProvider.GetMapPointFromWorldPoint(worldPoint);
        else if (worldPoint == Vector3.zero && screenCenteredMapPoint != Vector2.zero)
            worldPoint = _territoryInfoProvider.GetWorldPointFromMapPoint(screenCenteredMapPoint, out _);

        Vector2 mapCenteredMapPoint = GetMapCenteredPointPosition(screenCenteredMapPoint);

        _selectedPathPointSingle.SetPoint(mapCenteredMapPoint, screenCenteredMapPoint, worldPoint);
    }

    private void UpdateFinalPoint()
    {
        if (_finalPoint == null)
            return;

        bool isDisplayingFinalPoint = IsCurrentEnemyTypeDrone() == false && _allEnemyPathPointsSingle.Count >= 1;

        _finalPoint.gameObject.SetActive(isDisplayingFinalPoint);

        if (isDisplayingFinalPoint == false)
            return;

        ReadonlyPathPoint lastPathPoint = _allEnemyPathPointsSingle[^1].GetPathPoint();

        Vector3 closestWorldPoint = _territoryInfoProvider.GetClosestWorldFinalPoint(lastPathPoint.WorldPoint);
        Vector2 screenCenteredMapPoint = _territoryInfoProvider.GetMapPointFromWorldPoint(closestWorldPoint);
        Vector2 mapCenteredPointPosition = GetMapCenteredPointPosition(screenCenteredMapPoint);

        _finalPoint.SetPoint(mapCenteredPointPosition, screenCenteredMapPoint, closestWorldPoint);
    }

    private void UpdatePathIndexes()
    {
        for (int i = 0; i < _allEnemyPathPointsSingle.Count; i++)
        {
            PathPointSingleUI pathPointSingleUI = _allEnemyPathPointsSingle[i];
            pathPointSingleUI.SetIndex(i);
        }
    }

    private void UpdatePathPointsTypes()
    {
        if (_currentEnemyType is EnemyType.Vehicle)
        {
            int disembarkPointIndex = _allEnemyPathPointsSingle.Where(pathPointUI => _territoryInfoProvider.IsCanAssignPathPoint(
                    EnemyType.Vehicle, pathPointUI.GetPathPoint().ScreenCenteredMapPoint) == false).FirstOrDefault(pathPointUI => pathPointUI)
                ?.PointIndex ?? -1;

            foreach (PathPointSingleUI pathPointSingle in _allEnemyPathPointsSingle)
            {
                if (_selectedPathPointSingle == pathPointSingle)
                    continue;

                int currentPathPointIndex = pathPointSingle.PointIndex;
                PathPointType newPathPointType = PathPointType.DefaultPathPoint;

                if (currentPathPointIndex == 0)
                    newPathPointType = PathPointType.SpawnPathPoint;
                else if (disembarkPointIndex > 0)
                    if (currentPathPointIndex == disembarkPointIndex)
                        newPathPointType = PathPointType.SoldiersDisembarkPathPoint;
                    else if (currentPathPointIndex > disembarkPointIndex)
                        newPathPointType = PathPointType.DisembarkedSoldiersPathPoint;

                pathPointSingle.SetType(newPathPointType);
            }
        }
        else if (IsCurrentEnemyTypeDrone())
        {
            for (int i = 0; i < _allEnemyPathPointsSingle.Count; i++)
            {
                PathPointSingleUI pathPointSingle = _allEnemyPathPointsSingle[i];

                if (_selectedPathPointSingle == pathPointSingle)
                    continue;

                int currentPathPointIndex = pathPointSingle.PointIndex;
                PathPointType newPathPointType = PathPointType.DefaultPathPoint;

                if (currentPathPointIndex == 0)
                    newPathPointType = PathPointType.SpawnPathPoint;
                else if (i == _allEnemyPathPointsSingle.Count - 1)
                    newPathPointType = PathPointType.FinalDroneDestinationPoint;

                Debug.Log($"{pathPointSingle.name} {newPathPointType}");

                pathPointSingle.SetType(newPathPointType);
            }
        }
        else
        {
            foreach (PathPointSingleUI pathPointSingle in _allEnemyPathPointsSingle)
            {
                if (_selectedPathPointSingle == pathPointSingle)
                    continue;

                PathPointType newPathPointType = PathPointType.DefaultPathPoint;
                int currentPathPointIndex = pathPointSingle.PointIndex;

                if (currentPathPointIndex == 0)
                    newPathPointType = PathPointType.SpawnPathPoint;

                pathPointSingle.SetType(newPathPointType);
            }
        }
    }

    private void ClearPathData()
    {
        foreach (PathPointSingleUI pathPointSingleUI in _allEnemyPathPointsSingle)
        {
            Destroy(pathPointSingleUI.gameObject);
        }

        ClearPathPointArray();

        _selectedPathPointSingle = null;
    }

    private void ClearPathPointArray()
    {
        foreach (PathPointSingleUI pathPointSingleUI in _allEnemyPathPointsSingle)
        {
            pathPointSingleUI.PathPointDeleted -= NewPathPoint_OnPathPointDeleted;
            pathPointSingleUI.PathPointSelected -= NewPathPoint_OnPathPointSelected;
        }

        _allEnemyPathPointsSingle.Clear();
    }

    #endregion

    #region Get

    private bool IsCurrentEnemyTypeDrone()
    {
        return _currentEnemyType is (EnemyType.Drone or EnemyType.BigSlowDrone or EnemyType.SmallSpeedDrone);
    }

    private MapListeningPointType GetMapListeningPointType(int currentPathPointIndex)
    {
        MapListeningPointType mapListeningPointType =
            IsCurrentEnemyTypeDrone() && currentPathPointIndex > 0
                ? MapListeningPointType.MarkerPoint
                : MapListeningPointType.MapPoint;

        return mapListeningPointType;
    }

    private bool IsCanAssignPoint(int pointIndex, Vector2 screenCenteredMapPoint)
    {
        EnemyType currentEnemyType = _currentEnemyType;

        if (currentEnemyType == EnemyType.Vehicle)
        {
            bool isHasDisembarkPoint = IsHasDisembarkPoint(out int disembarkPointIndex);
            bool isCurrentPointAfterDisembark = isHasDisembarkPoint && pointIndex > disembarkPointIndex;
            bool isCanBecomeDisembarkPoint = isHasDisembarkPoint == false && pointIndex > 0;

            if (isCurrentPointAfterDisembark || isCanBecomeDisembarkPoint)
                currentEnemyType = EnemyType.Soldier;
        }

        if (pointIndex == 0)
        {
            if (_territoryInfoProvider.IsCanAssignSpawnPoint(currentEnemyType, screenCenteredMapPoint) == false)
                return false;
        }
        else
        {
            if (_territoryInfoProvider.IsCanAssignPathPoint(currentEnemyType, screenCenteredMapPoint) == false)
                return false;
        }

        return true;
    }

    private List<ReadonlyPathPoint> GetPathPoints()
    {
        List<ReadonlyPathPoint> currentTypedPathPointsPoints = _allEnemyPathPointsSingle.Select(pathPointUI => pathPointUI.GetPathPoint()).ToList();

        if (IsCurrentEnemyTypeDrone() == false && currentTypedPathPointsPoints.Count > 0 && _selectedPathPointSingle == null)
            currentTypedPathPointsPoints.Add(_finalPoint.GetPathPoint());

        return currentTypedPathPointsPoints;
    }

    private Vector2 GetMapCenteredPointPosition(Vector2 screenCenteredPoint)
    {
        return screenCenteredPoint - (_screenCenteredPointLimits.MinPoint + _screenCenteredPointLimits.MaxPoint) / 2;
    }

    private bool IsHasDisembarkPoint(out int disembarkPointIndex)
    {
        disembarkPointIndex = -1;

        foreach (PathPointSingleUI enemyPathPoint in _allEnemyPathPointsSingle)
        {
            if (enemyPathPoint.GetPathPoint().PathPointType !=
                PathPointType.SoldiersDisembarkPathPoint) continue;

            disembarkPointIndex = enemyPathPoint.PointIndex;

            return true;
        }

        return false;
    }

    public override Dictionary<OperationStatSingle, object> GetAllTabOperationStats()
    {
        Dictionary<OperationStatSingle, object> tabOperationStats = new();

        List<ReadonlyPathPoint> allPathPoints = GetPathPoints();
        tabOperationStats.Add(OperationStatSingle.EnemyPathPoints, allPathPoints);

        return tabOperationStats;
    }

    #endregion

    public override void Dispose()
    {
        ClearPathPointArray();
    }
}