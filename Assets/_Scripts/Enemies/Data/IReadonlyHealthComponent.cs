#region

using System;

#endregion

public interface IReadonlyHealthComponent
{
    public event Action<float, float> HealthChanged;

    public event Action HealthDepleted;

    public float MaxHealth { get; }

    public float CurrentHealth { get; }

    public EntityTeam EntityTeam { get; }

    public bool IsDestroyed { get; }
}