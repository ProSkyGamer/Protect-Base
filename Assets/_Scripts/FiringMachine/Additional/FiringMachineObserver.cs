#region

using System;
using UnityEngine;
using Zenject;

#endregion

public class FiringMachineObserver : IInitializable, IOperationsStatusListener, IOperationStatsDataProvider, IDisposable
{
    #region Events

    public event Action<OperationSavingStatType, object> DataChanged;

    #endregion

    #region Variables & References

    private readonly FiringMachineController _observingFiringMachine;
    private readonly FiringMachineVisuals _firingMachineVisuals;
    private readonly CustomEventsManager _customEventsManager;
    private readonly TCPServerConnector _tcpServerConnector;

    private bool _isSendingPowerStateChange = true;

    public int ObservingFiringMachineNumber => _observingFiringMachine.FiringMachineNumber;

    #endregion

    #region Initialization

    public FiringMachineObserver(FiringMachineController observingFiringMachine,
        CustomEventsManager customEventsManager, TCPServerConnector tcpServerConnector,
        FiringMachineVisuals firingMachineVisuals)
    {
        _observingFiringMachine = observingFiringMachine;
        _customEventsManager = customEventsManager;
        _tcpServerConnector = tcpServerConnector;
        _firingMachineVisuals = firingMachineVisuals;
    }

    public void OperationStarted()
    {
        DataChanged?.Invoke(OperationSavingStatType.FiringMachineMaxHealth, _observingFiringMachine.HealthComponent.CurrentHealth);
    }

    public void OperationEnded()
    {
    }

    public void Initialize()
    {
        _observingFiringMachine.PovStatusChanged += ObservingFiringMachine_OnStatusChanged;
        _observingFiringMachine.PovAnglesChanged += ObservingFiringMachine_OnPovAnglesChanged;
        _observingFiringMachine.ShotInitiated += ObservingFiringMachine_OnShotInitiated;
        _observingFiringMachine.FiringModeStateChanged += ObservingFiringMachine_OnFiringModeStateChanged;
        _observingFiringMachine.HealthDepleted += ObservingFiringMachine_OnHealthDepleted;
        _observingFiringMachine.HealthComponent.HealthChanged += HealthComponent_OnHealthChanged;
    }

    private void HealthComponent_OnHealthChanged(float newHealth, float damageTaken)
    {
        DataChanged?.Invoke(OperationSavingStatType.FiringMachineDamageTaken, damageTaken);
    }

    private void ObservingFiringMachine_OnPovAnglesChanged()
    {
        _firingMachineVisuals.SetFiringMachineRotationVisuals(_observingFiringMachine.CurrentEulerAnglesWithoutBase);
    }

    private void ObservingFiringMachine_OnHealthDepleted(int obj)
    {
        _tcpServerConnector.SendMessageByConnection(
            GetFiringMachineBrokenStateInfoString(ObservingFiringMachineNumber));

        DataChanged?.Invoke(OperationSavingStatType.FiringMachineDestroyed, 1);
    }

    private void ObservingFiringMachine_OnFiringModeStateChanged(bool firingModeState)
    {
        string sendingMessage = firingModeState ? "Разблок. стрельбы" : "Блокир. стрельбы";
        _customEventsManager.AddEvent(sendingMessage);

        string debugMessage = firingModeState
            ? $"UNLOCKED second shooting step for {_observingFiringMachine.FiringMachineNumber}"
            : $"LOCKED second shooting step for {_observingFiringMachine.FiringMachineNumber}";

        Debug.Log(debugMessage);
    }

    private void ObservingFiringMachine_OnShotInitiated(ShootingBlockType shootingBlockType, ShootingType shootingType)
    {
        string sendingMessage = shootingBlockType switch
        {
            ShootingBlockType.ExplosiveOne => "Граната 1",
            ShootingBlockType.ExplosiveTwo => "Граната 2",
            var _ => shootingType is ShootingType.Single ? "Одиночный выстрел" : "Залп"
        };

        _customEventsManager.AddEvent(sendingMessage);

        Debug.Log($"Shot from {_observingFiringMachine.FiringMachineNumber}");
    }

    private void ObservingFiringMachine_OnStatusChanged()
    {
        if (_observingFiringMachine.CurrentPoVStatus)
        {
            Debug.Log($"Firing machine {_observingFiringMachine.FiringMachineNumber} turned ON");

            _customEventsManager.AddEvent($"Включение СУ {_observingFiringMachine.FiringMachineNumber}");
        }
        else
        {
            Debug.Log($"Firing machine {_observingFiringMachine.FiringMachineNumber} turned OFF");

            _customEventsManager.AddEvent($"Выключение СУ {_observingFiringMachine.FiringMachineNumber}");
        }

        if (!_isSendingPowerStateChange)
        {
            _isSendingPowerStateChange = true;

            return;
        }

        _tcpServerConnector.SendMessageByConnection(
            GetFiringMachineStateInfoString(ObservingFiringMachineNumber,
                _observingFiringMachine.CurrentPoVStatus));
    }

    #endregion

    #region Inperuptions

    public void InterruptNextPowerSwitchObservation()
    {
        _isSendingPowerStateChange = false;
    }

    #endregion

    #region Get

    private string GetFiringMachineStateInfoString(int firingMachineIndex, bool isEnabled)
    {
        string firingMachineStateInfoString = "firingmachine_{0}_[{1}]";

        firingMachineStateInfoString = string.Format(firingMachineStateInfoString, isEnabled ? "enabled" : "disabled",
            firingMachineIndex);

        return firingMachineStateInfoString;
    }

    private string GetFiringMachineBrokenStateInfoString(int firingMachineIndex)
    {
        string firingMachineStateInfoString = $"firingmachine_broken_[{firingMachineIndex}]";

        return firingMachineStateInfoString;
    }

    #endregion

    public void Dispose()
    {
        _observingFiringMachine.PovStatusChanged -= ObservingFiringMachine_OnStatusChanged;
        _observingFiringMachine.PovAnglesChanged -= ObservingFiringMachine_OnPovAnglesChanged;
        _observingFiringMachine.ShotInitiated -= ObservingFiringMachine_OnShotInitiated;
        _observingFiringMachine.FiringModeStateChanged -= ObservingFiringMachine_OnFiringModeStateChanged;
        _observingFiringMachine.HealthDepleted -= ObservingFiringMachine_OnHealthDepleted;
    }
}