#region

using System;
using Unity.Netcode;
using Zenject;

#endregion

public class FiringMachineHealthController : NetworkBehaviour, IHaveHealth, ISceneResettable
{
    #region Events

    public event Action<float, float> HealthChanged;

    public event Action HealthDepleted;

    #endregion

    #region Variables & References

    private readonly NetworkVariable<float> _currentHealth = new();

    private FiringMachineStatsSO _firingMachineStatsSO;

    #endregion

    #region Properties

    public float MaxHealth => _firingMachineStatsSO.MaxHealth;

    public float CurrentHealth => _currentHealth.Value;

    public bool IsDestroyed => _currentHealth.Value <= 0;

    public EntityTeam EntityTeam => EntityTeam.FiringMachine;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(FiringMachineStatsSO firingMachineStatsSO)
    {
        _firingMachineStatsSO = firingMachineStatsSO;
    }

    public override void OnNetworkSpawn()
    {
        _currentHealth.OnValueChanged += CurrentHealth_OnValueChanged;

        if (IsServer == false)
            return;

        _currentHealth.Value = MaxHealth;
    }

    private void CurrentHealth_OnValueChanged(float previousValue, float newValue)
    {
        if (Math.Abs(previousValue - newValue) < 1e-3)
            return;

        HealthChanged?.Invoke(newValue, previousValue - newValue < 0 ? 0f : previousValue - newValue);
    }

    #endregion

    #region Health

    public void TakeDamage(float damage)
    {
        if (IsServer == false)
            return;

        _currentHealth.Value -= damage;

        HealthChanged?.Invoke(_currentHealth.Value, damage);

        if (_currentHealth.Value <= 0f)
            Die();
    }

    private void Die()
    {
        if (IsServer == false)
            return;

        HealthDepleted?.Invoke();
    }

    #endregion

    public void OnSceneReset()
    {
        if (IsServer)
            _currentHealth.Value = MaxHealth;
    }
}