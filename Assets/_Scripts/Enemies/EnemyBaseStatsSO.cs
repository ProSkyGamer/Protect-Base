#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#endregion

[Serializable]
public class EnemyStatValue
{
    public EnemyStatSize StatSize;
    public float StatValue;
}

[Serializable]
public class BaseEnemyStats
{
    public EnemyType EnemyType;

    [Header("HP")] public List<EnemyStatValue> AllHealthValues;

    [Header("ATK")] public List<EnemyStatValue> AllAtkValues;

    [Header("SPD")] public List<EnemyStatValue> AllSpeedValues;

    [Header("PATH")] public int MinPathPointCount = 2;
    public int MaxEnemyPathPoint = -1;

    [Header("OTHER")] public int MaxEnemyWaveCount;
    public int EnemySummonsCount;
}

//[CreateAssetMenu()]
public class EnemyBaseStatsSO : ScriptableObject
{
    public List<BaseEnemyStats> AllBaseEnemyStats;
    public float EnemySoldierAttackingFiringMachineRadius = 60f;
    public float EnemySoldierShootingTimeCooldown = 15f;
    public float EnemySoldierShootingTimeOnPointReached = 5f;
    public float EnemySoldierShootingBulletInterval = .25f;
    [Range(0f, 1f)] public float EnemySoldierBulletsAccuracy = .375f;
    public float MaxBulletDirectionDeviation = 5f;
    public LayerMask BreakableFenceLayerMask;

    public float GetEnemyHealthStat(EnemyType enemyType, EnemyStatSize statSize)
    {
        float statValue = AllBaseEnemyStats.Where(baseEnemyStats =>
                baseEnemyStats.EnemyType == enemyType).SelectMany(baseEnemyStats => baseEnemyStats.AllHealthValues)
            .FirstOrDefault(enemyStatsValue => enemyStatsValue.StatSize == statSize)?.StatValue ?? -1f;

        return statValue;
    }

    public float GetEnemyAtkStat(EnemyType enemyType, EnemyStatSize statSize)
    {
        float statValue = AllBaseEnemyStats.Where(baseEnemyStats =>
                baseEnemyStats.EnemyType == enemyType).SelectMany(baseEnemyStats => baseEnemyStats.AllAtkValues)
            .FirstOrDefault(enemyStatsValue => enemyStatsValue.StatSize == statSize)?.StatValue ?? -1f;

        return statValue;
    }

    public float GetEnemySpeedStat(EnemyType enemyType, EnemyStatSize statSize)
    {
        float statValue = AllBaseEnemyStats.Where(baseEnemyStats =>
                baseEnemyStats.EnemyType == enemyType).SelectMany(baseEnemyStats => baseEnemyStats.AllSpeedValues)
            .FirstOrDefault(enemyStatsValue => enemyStatsValue.StatSize == statSize)?.StatValue ?? -1f;

        return statValue;
    }

    public int GetMaxEnemyWaveCount(EnemyType enemyType)
    {
        int maxEnemyWaveCount = AllBaseEnemyStats.FirstOrDefault(enemyStats => enemyStats.EnemyType == enemyType)?.MaxEnemyWaveCount ?? -1;

        return maxEnemyWaveCount;
    }

    public int GetMinEnemyPathPointsCount(EnemyType enemyType)
    {
        int minPathPointCount = AllBaseEnemyStats.FirstOrDefault(enemyStats => enemyStats.EnemyType == enemyType)?.MinPathPointCount ?? -1;

        return minPathPointCount;
    }

    public int GetMaxEnemySummonsCount(EnemyType enemyType)
    {
        int maxSummonsCount = AllBaseEnemyStats.FirstOrDefault(enemyStats => enemyStats.EnemyType == enemyType)?.EnemySummonsCount ?? -1;

        return maxSummonsCount;
    }

    public int GetMaxEnemyPathPoints(EnemyType enemyType)
    {
        int maxPathPoints = AllBaseEnemyStats.FirstOrDefault(enemyStats => enemyStats.EnemyType == enemyType)?.MaxEnemyPathPoint ?? -1;

        return maxPathPoints;
    }

    public EnemyStatSize GetEnemyHealthStatIndexFromValue(EnemyType enemyType, float statValue)
    {
        EnemyStatSize statSize = AllBaseEnemyStats.Where(baseEnemyStats =>
                baseEnemyStats.EnemyType == enemyType).SelectMany(baseEnemyStats => baseEnemyStats.AllHealthValues)
            .FirstOrDefault(enemyStatsValue => Math.Abs(enemyStatsValue.StatValue - statValue) < 0.001f)?.StatSize ?? EnemyStatSize.Small;

        return statSize;
    }

    public EnemyStatSize GetEnemyAtkStatIndexFromValue(EnemyType enemyType, float statValue)
    {
        EnemyStatSize statSize = AllBaseEnemyStats.Where(baseEnemyStats =>
                baseEnemyStats.EnemyType == enemyType).SelectMany(baseEnemyStats => baseEnemyStats.AllAtkValues)
            .FirstOrDefault(enemyStatsValue => Math.Abs(enemyStatsValue.StatValue - statValue) < 0.001f)?.StatSize ?? EnemyStatSize.Small;

        return statSize;
    }

    public EnemyStatSize GetEnemySpeedStatIndexFromValue(EnemyType enemyType, float statValue)
    {
        EnemyStatSize statSize = AllBaseEnemyStats.Where(baseEnemyStats =>
                baseEnemyStats.EnemyType == enemyType).SelectMany(baseEnemyStats => baseEnemyStats.AllSpeedValues)
            .FirstOrDefault(enemyStatsValue => Math.Abs(enemyStatsValue.StatValue - statValue) < 0.001f)?.StatSize ?? EnemyStatSize.Small;

        return statSize;
    }
}