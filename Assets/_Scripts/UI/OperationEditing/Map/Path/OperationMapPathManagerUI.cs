#region

using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class OperationMapPathManagerUI : MonoBehaviour, IInitializable
{
    #region Variables & References

    [SerializeField] private int _totalLinesInFullMap = 98;
    [SerializeField] private int _overflowLinesCount = 5;

    private DirectionLinesUIFactory _directionLinesUIFactory;
    private PathPointsFactory _pathPointsFactory;
    private ITerritoryInfoProvider _territoryInfoProvider;

    private ObjectLimits _mapPointsLimits;
    private float _totalMapLength;
    private readonly List<Transform> _allCurrentDisplayingPathPoints = new();
    private readonly List<Transform> _allAdditionalDirectionLines = new();
    private readonly List<Transform> _allDirectionLines = new();
    private int _selectedPathPointIndex;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ITerritoryInfoProvider territoryInfoProvider, DirectionLinesUIFactory directionLinesUIFactory,
        PathPointsFactory pathPointsFactory)
    {
        _territoryInfoProvider = territoryInfoProvider;
        _pathPointsFactory = pathPointsFactory;
        _directionLinesUIFactory = directionLinesUIFactory;
    }

    public void Initialize()
    {
        _mapPointsLimits = _territoryInfoProvider.GetMapLimits();
        _totalMapLength = _mapPointsLimits.MaxPoint.x - _mapPointsLimits.MinPoint.x;
    }

    #endregion

    #region Path

    public void DisplayCurrentEnemyPathPoints(IReadOnlyList<ReadonlyPathPoint> allEnemyPathPoints)
    {
        _selectedPathPointIndex = -1;

        ClearCurrentPathPoint();

        for (int i = 0; i < allEnemyPathPoints.Count; i++)
        {
            ReadonlyPathPoint enemyPathPoint = allEnemyPathPoints[i];

            if (enemyPathPoint.PathPointType is PathPointType.SelectedPathPoint)
            {
                _selectedPathPointIndex = i;

                continue;
            }

            AddEnemyPathPoint(enemyPathPoint);
        }
    }

    private void AddEnemyPathPoint(ReadonlyPathPoint typedPathPointSingle)
    {
        if (typedPathPointSingle.PathPointType == PathPointType.SelectedPathPoint)
            return;

        Transform newMapPathPoint = _pathPointsFactory.Create(typedPathPointSingle.PathPointType).transform;
        newMapPathPoint.position = typedPathPointSingle.ScreenCenteredMapPoint;
        _allCurrentDisplayingPathPoints.Add(newMapPathPoint);
    }

    public void DisplayCurrentSelectedPointAdditionalDirectionLines(Vector3 currentPointPosition)
    {
        foreach (Transform toDelete in _allAdditionalDirectionLines)
        {
            Destroy(toDelete.gameObject);
        }

        _allAdditionalDirectionLines.Clear();

        if (_selectedPathPointIndex - 1 >= 0)
        {
            Transform incomingToSelectedPointLine = GetInstantiatedDisplayingPathLine(
                _allCurrentDisplayingPathPoints[_selectedPathPointIndex - 1].position, currentPointPosition);

            _allAdditionalDirectionLines.Add(incomingToSelectedPointLine);
        }

        if (_selectedPathPointIndex > 0 && _selectedPathPointIndex < _allCurrentDisplayingPathPoints.Count)
        {
            Transform outgoingToSelectedPointLine = GetInstantiatedDisplayingPathLine(currentPointPosition,
                _allCurrentDisplayingPathPoints[_selectedPathPointIndex].position);

            _allAdditionalDirectionLines.Add(outgoingToSelectedPointLine);
        }
    }

    public void UpdateCurrentDirectionLines()
    {
        ClearCurrentDirectionLines();

        for (int i = 0; i < _allCurrentDisplayingPathPoints.Count; i++)
        {
            if (i + 1 >= _allCurrentDisplayingPathPoints.Count)
                break;

            if (i + 1 == _selectedPathPointIndex && _selectedPathPointIndex != -1)
                continue;

            Vector3 currentPathPointPosition = _allCurrentDisplayingPathPoints[i].position;
            Vector3 nextPathPointPosition = _allCurrentDisplayingPathPoints[i + 1].position;

            Transform newDirectionalLine = GetInstantiatedDisplayingPathLine(currentPathPointPosition, nextPathPointPosition);

            _allDirectionLines.Add(newDirectionalLine);
        }
    }

    private Transform GetInstantiatedDisplayingPathLine(Vector3 currentPathPointPosition, Vector3 nextPathPointPosition)
    {
        Vector3 directionLinePosition =
            currentPathPointPosition + (nextPathPointPosition - currentPathPointPosition) / 2;

        string mapPointLineDirectionString = GetMapPointLineDirectionString(currentPathPointPosition, nextPathPointPosition);

        MapPointDirectionLineUI newMapPointDirectionLine = _directionLinesUIFactory.Create(mapPointLineDirectionString);
        newMapPointDirectionLine.transform.position = directionLinePosition;
        Vector3 mapPointsLineDirection = (nextPathPointPosition - currentPathPointPosition).normalized;

        if (mapPointsLineDirection != Vector3.zero)
        {
            float mapPointLineAngle = Mathf.Atan2(mapPointsLineDirection.y, mapPointsLineDirection.x) * Mathf.Rad2Deg;
            newMapPointDirectionLine.transform.rotation = Quaternion.Euler(0, 0, mapPointLineAngle);
        }

        return newMapPointDirectionLine.transform;
    }

    private string GetMapPointLineDirectionString(Vector3 currentPathPointPosition, Vector3 nextPathPointPosition)
    {
        string mapPointLineDirectionString = "";
        float lineDistance = (nextPathPointPosition - currentPathPointPosition).magnitude;

        float mapLinesCount = lineDistance / _totalMapLength *
            _totalLinesInFullMap - 1;

        mapLinesCount -= _overflowLinesCount;

        for (int j = 0; j < mapLinesCount; j++) mapPointLineDirectionString += "-";

        mapPointLineDirectionString += ">";

        return mapPointLineDirectionString;
    }

    private void ClearCurrentDirectionLines()
    {
        foreach (Transform directionalLine in _allAdditionalDirectionLines)
        {
            Destroy(directionalLine.gameObject);
        }

        foreach (Transform directionalLine in _allDirectionLines)
        {
            Destroy(directionalLine.gameObject);
        }

        _allDirectionLines.Clear();
        _allAdditionalDirectionLines.Clear();
    }

    private void ClearCurrentPathPoint()
    {
        foreach (Transform pathPoint in _allCurrentDisplayingPathPoints)
        {
            Destroy(pathPoint.gameObject);
        }

        _allCurrentDisplayingPathPoints.Clear();

        ClearCurrentDirectionLines();
    }

    #endregion
}