#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public interface ITerritoryInfoProvider
{
    public Vector3 GetWorldPointFromMapPoint(Vector2 imagePointPosition, out bool isPointValid);
    public Vector2 GetMapPointFromWorldPoint(Vector3 worldPoint);

    public bool IsCanAssignSpawnPoint(EnemyType enemyType, Vector2 enemyImageSpawningPosition);
    public bool IsCanAssignPathPoint(EnemyType enemyType, Vector2 enemyImageSpawningPosition);

    public ObjectLimits GetMapLimits();
    public List<OperationTerritoryType> GetAvailableSpawnZoneTypes(EnemyType enemyType);

    public List<OperationTerritoryType> GetForbiddenEnemiesZoneTypes(EnemyType enemyType);

    public Vector3 GetClosestWorldFinalPoint(Vector3 lastWorldPointPosition);
}