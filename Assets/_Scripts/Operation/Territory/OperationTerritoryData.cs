#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

[Serializable]
public class OperationZone
{
    public OperationTerritoryType OperationTerritoryType;

    public LayerMask ZoneLayerMask;
}

[Serializable]
public class EnemiesAssignableZoneSpawns
{
    public EnemyType EnemyType;

    public List<OperationTerritoryType> AvailableZonesForEnemyTypeToSpawn;
}

[Serializable]
public class EnemiesRestrictedZones // points for this enemies cant be placed in this zone types
{
    public EnemyType EnemyType;

    public List<OperationTerritoryType> EnemyTypeZonesRestricted;
}