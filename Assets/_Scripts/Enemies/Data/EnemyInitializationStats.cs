#region

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Unity.Netcode;

#endregion

[Serializable]
public class ReadonlyEnemyInitializationStats : INetworkCustomSerializable
{
    public IReadOnlyList<ReadonlyPathPoint> FullPath => _fullPath;

    protected List<ReadonlyPathPoint> _fullPath;
    private ReadonlyPathPoint[] _fullPathArray;

    public float MaxHealth => _maxHealth;

    protected float _maxHealth;

    public float BaseAtk => _baseAtk;

    protected float _baseAtk;

    public float BaseSpeed => _baseSpeed;

    protected float _baseSpeed;

    public ReadonlyEnemyInitializationStats()
    {
        _fullPath = new List<ReadonlyPathPoint>();
    }

    [JsonConstructor]
    public ReadonlyEnemyInitializationStats(List<ReadonlyPathPoint> enemyPath, float maxHealth, float baseAtk,
        float baseSpeed)
    {
        _fullPath = enemyPath;
        _maxHealth = maxHealth;
        _baseAtk = baseAtk;
        _baseSpeed = baseSpeed;
    }

    public virtual void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _maxHealth);
        serializer.SerializeValue(ref _baseAtk);
        serializer.SerializeValue(ref _baseSpeed);
        serializer.SerializeValue(ref _fullPathArray);
    }

    public void PackForNetworkTransfer()
    {
        _fullPathArray = new ReadonlyPathPoint[FullPath.Count];

        _fullPath.CopyTo(_fullPathArray);
    }

    public void UnpackAfterNetworkTransfer()
    {
        _fullPath.Clear();
        _fullPath.AddRange(_fullPathArray);
    }
}

public class EnemyInitializationStats : ReadonlyEnemyInitializationStats
{
    public new List<ReadonlyPathPoint> FullPath => _fullPath;

    public EnemyInitializationStats(ReadonlyEnemyInitializationStats readonlyEnemyInitializationStats)
    {
        _fullPath = new List<ReadonlyPathPoint>();
        _fullPath.AddRange(readonlyEnemyInitializationStats.FullPath);

        _maxHealth = readonlyEnemyInitializationStats.MaxHealth;
        _baseAtk = readonlyEnemyInitializationStats.BaseAtk;
        _baseSpeed = readonlyEnemyInitializationStats.BaseSpeed;
    }
}

public class VehicleInitializationStats : ReadonlyEnemyInitializationStats
{
    public VehicleInitializationStats(ReadonlyEnemyInitializationStats readonlyEnemyInitializationStats,
        int spawningSoldiersCount)
    {
        _fullPath = new List<ReadonlyPathPoint>();
        _fullPath.AddRange(readonlyEnemyInitializationStats.FullPath);

        _maxHealth = readonlyEnemyInitializationStats.MaxHealth;
        _baseAtk = readonlyEnemyInitializationStats.BaseAtk;
        _baseSpeed = readonlyEnemyInitializationStats.BaseSpeed;
        _spawningSoldiersCount = spawningSoldiersCount;
    }

    public int SpawningSoldiersCount => _spawningSoldiersCount;

    private int _spawningSoldiersCount;

    public override void NetworkSerialize<T>(BufferSerializer<T> serializer)
    {
        base.NetworkSerialize(serializer);

        serializer.SerializeValue(ref _spawningSoldiersCount);
    }
}