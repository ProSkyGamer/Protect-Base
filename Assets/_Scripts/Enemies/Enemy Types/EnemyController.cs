#region

using System;
using System.Collections.Generic;
using Unity.Netcode;

#endregion

public abstract class EnemyController : NetworkBehaviour, IOperationStatsDataProvider
{
    public abstract event Action<EnemyController> EnemySpawned;
    public abstract event Action<OperationSavingStatType, object> DataChanged;

    public abstract IHaveHealth HealthComponent { get; }

    public float MaxHealth => _maxHealth.Value;

    public float CurrentHealth => _currentHealth.Value;

    public float CurrentAtk => _currentAtk.Value;

    public float CurrentSpeed => _currentSpeed.Value;

    public bool IsDestroyed => _isDead.Value;

    public abstract EnemyType EnemyType { get; }

    public abstract IReadOnlyList<ReadonlyPathPoint> EnemyPath { get; }

    protected readonly NetworkVariable<float> _maxHealth = new();
    protected readonly NetworkVariable<float> _currentHealth = new();
    protected readonly NetworkVariable<float> _currentSpeed = new();
    protected readonly NetworkVariable<float> _currentAtk = new();
    protected readonly NetworkVariable<bool> _isDead = new();
}