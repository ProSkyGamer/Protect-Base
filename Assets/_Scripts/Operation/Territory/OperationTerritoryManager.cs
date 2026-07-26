#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

#endregion

public class OperationTerritoryManager : MonoBehaviour, IInitializable, ITerritoryInfoProvider
{
    #region Variables & References

    [SerializeField] private Transform _topRightImagePointTransform;
    [SerializeField] private Transform _bottomLeftImagePointTransform;
    [SerializeField] private Transform _topRightWorldPointTransform;
    [SerializeField] private Transform _bottomLeftWorldPointTransform;
    [SerializeField] private LayerMask _mainTerritoryLayer;
    [SerializeField] private List<OperationZone> _allOperationZones;

    [SerializeField] private List<EnemiesAssignableZoneSpawns> _allEnemiesAssignableZoneSpawns;
    [SerializeField] private List<EnemiesRestrictedZones> _allEnemiesZonesRestrictions;
    [SerializeField] private List<Transform> _allFinalDestinationPoint;

    public LayerMask LayerMaskss;

    private Vector2 _totalMapImageLength;
    private Vector2 _totalWorldTerritoryLength;

    private readonly float _castHeight = 200f;
    private readonly float _castDistance = 350f;

    private ObjectLimits _mapLimits;

    #endregion

    #region Initialization

    public void Initialize()
    {
        float mapImageLength = _topRightImagePointTransform.position.x - _bottomLeftImagePointTransform.position.x;
        float mapImageHeight = _topRightImagePointTransform.position.y - _bottomLeftImagePointTransform.position.y;
        _totalMapImageLength = new Vector2(mapImageLength, mapImageHeight);

        float mapWorldLength = _topRightWorldPointTransform.position.x - _bottomLeftWorldPointTransform.position.x;
        float mapWorldHeight = _topRightWorldPointTransform.position.z - _bottomLeftWorldPointTransform.position.z;
        _totalWorldTerritoryLength = new Vector2(mapWorldLength, mapWorldHeight);

        _mapLimits = new ObjectLimits(_bottomLeftImagePointTransform.transform.position, _topRightImagePointTransform.transform.position);
    }

    #endregion

    #region Get

    public Vector3 GetWorldPointFromMapPoint(Vector2 imagePointPosition, out bool isPointValid)
    {
        imagePointPosition.x = Mathf.Clamp(imagePointPosition.x, _bottomLeftImagePointTransform.position.x,
            _topRightImagePointTransform.position.x);

        imagePointPosition.y = Mathf.Clamp(imagePointPosition.y, _bottomLeftImagePointTransform.position.y,
            _topRightImagePointTransform.position.y);

        float worldXCoordinated = _bottomLeftWorldPointTransform.position.x +
                                  (_totalMapImageLength.x -
                                   (_topRightImagePointTransform.position.x - imagePointPosition.x)) /
                                  _totalMapImageLength.x * _totalWorldTerritoryLength.x;

        float worldZCoordinates = _bottomLeftWorldPointTransform.position.z +
                                  (_totalMapImageLength.y -
                                   (_topRightImagePointTransform.position.y - imagePointPosition.y)) /
                                  _totalMapImageLength.y * _totalWorldTerritoryLength.y;

        Vector3 worldPointPosition = new(worldXCoordinated, 0f, worldZCoordinates);
        worldPointPosition = GetHeightCorrectedWorldPoint(worldPointPosition, out isPointValid);

        return worldPointPosition;
    }

    private Vector3 GetHeightCorrectedWorldPoint(Vector3 worldPoint, out bool isPointValid)
    {
        Vector3 castPosition = new(worldPoint.x, _castHeight, worldPoint.z);
        Vector3 castDirection = Vector3.down;

        RaycastHit[] territoryCast = Physics.RaycastAll(castPosition, castDirection, _castDistance,
            _mainTerritoryLayer);

        isPointValid = territoryCast.Length > 0;

        Vector3 correctedWorldPoint = worldPoint;

        if (territoryCast.Length > 0)
            correctedWorldPoint = new Vector3(worldPoint.x, _castHeight - territoryCast[0].distance, worldPoint.z);

        return correctedWorldPoint;
    }

    public Vector2 GetMapPointFromWorldPoint(Vector3 worldPoint)
    {
        worldPoint.x = Mathf.Clamp(worldPoint.x, _bottomLeftWorldPointTransform.position.x,
            _topRightWorldPointTransform.position.x);

        worldPoint.z = Mathf.Clamp(worldPoint.z, _bottomLeftWorldPointTransform.position.z,
            _topRightWorldPointTransform.position.z);

        float mapPointXPosition = _bottomLeftImagePointTransform.transform.position.x +
                                  (_totalWorldTerritoryLength.x - (_topRightWorldPointTransform.position.x - worldPoint.x)) /
                                  _totalWorldTerritoryLength.x * _totalMapImageLength.x;

        float mapPointYPosition = _bottomLeftImagePointTransform.transform.position.y +
                                  (_totalWorldTerritoryLength.y - (_topRightWorldPointTransform.position.z - worldPoint.z)) /
                                  _totalWorldTerritoryLength.y * _totalMapImageLength.y;

        Vector2 mapPointPosition = new(mapPointXPosition, mapPointYPosition);

        return mapPointPosition;
    }

    private LayerMask GetZoneLayerMask(List<OperationTerritoryType> operationTerritoryType)
    {
        LayerMask layerMask = new();

        List<OperationZone> allLayerMasks = _allOperationZones.FindAll(operationZone =>
            operationTerritoryType.Contains(operationZone.OperationTerritoryType));

        foreach (OperationZone operationZone in allLayerMasks)
        {
            layerMask = layerMask.value | operationZone.ZoneLayerMask;
        }

        return layerMask;
    }

    public bool IsCanAssignSpawnPoint(EnemyType enemyType, Vector2 enemyImageSpawningPosition)
    {
        List<OperationTerritoryType> availableSpawningZones = _allEnemiesAssignableZoneSpawns
            .Find(spawnZone => spawnZone.EnemyType == enemyType).AvailableZonesForEnemyTypeToSpawn;

        if (availableSpawningZones.Count <= 0) return false;

        Vector3 terrainSpawningPosition = GetWorldPointFromMapPoint(enemyImageSpawningPosition, out bool isPointValid);

        if (isPointValid == false)
            return false;

        terrainSpawningPosition.y = _castHeight;
        Vector3 castDirection = Vector3.down;

        LayerMaskss = GetZoneLayerMask(availableSpawningZones);
        LayerMask zoneLayerMask = GetZoneLayerMask(availableSpawningZones);

        bool isCanAssignSpawnPoint = Physics.Raycast(terrainSpawningPosition, castDirection, _castDistance, zoneLayerMask);

        return isCanAssignSpawnPoint;
    }

    public bool IsCanAssignPathPoint(EnemyType enemyType, Vector2 enemyImageSpawningPosition)
    {
        List<OperationTerritoryType> allAllowedZones = Enum.GetValues(typeof(OperationTerritoryType)).Cast<OperationTerritoryType>().ToList();

        List<OperationTerritoryType> allRestrictedZones = _allEnemiesZonesRestrictions
            .Where(restrictedZones => restrictedZones.EnemyType == enemyType)
            .SelectMany(restrictedZones => restrictedZones.EnemyTypeZonesRestricted).ToList();

        allAllowedZones = allAllowedZones.Where(allowedZone => allRestrictedZones.Contains(allowedZone) == false).ToList();

        Vector3 terrainPathPointPosition = GetWorldPointFromMapPoint(enemyImageSpawningPosition, out bool isPointValid);

        if (!isPointValid) return false;

        terrainPathPointPosition.y = _castHeight;
        Vector3 castDirection = Vector3.down;

        return Physics.Raycast(terrainPathPointPosition, castDirection, _castDistance, GetZoneLayerMask(allAllowedZones));
    }

    public ObjectLimits GetMapLimits()
    {
        return _mapLimits;
    }

    public List<OperationTerritoryType> GetAvailableSpawnZoneTypes(EnemyType enemyType)
    {
        List<OperationTerritoryType> availableSpawnZones = _allEnemiesAssignableZoneSpawns.Where(zoneSpawn => zoneSpawn.EnemyType == enemyType)
            .SelectMany(zoneSpawn => zoneSpawn.AvailableZonesForEnemyTypeToSpawn).ToList();

        return availableSpawnZones;
    }

    public List<OperationTerritoryType> GetForbiddenEnemiesZoneTypes(EnemyType enemyType)
    {
        List<OperationTerritoryType> allForbiddenEnemyZoneTypes = _allEnemiesZonesRestrictions
            .Where(restrictedZones => restrictedZones.EnemyType == enemyType).SelectMany(restrictedZones => restrictedZones.EnemyTypeZonesRestricted)
            .ToList();

        return allForbiddenEnemyZoneTypes;
    }

    public Vector3 GetClosestWorldFinalPoint(Vector3 lastWorldPointPosition)
    {
        Transform closestPoint = _allFinalDestinationPoint.OrderBy(finalPoint => (finalPoint.position - lastWorldPointPosition).magnitude)
            .FirstOrDefault();

        return closestPoint == null ? Vector3.zero : closestPoint.position;
    }

    #endregion
}