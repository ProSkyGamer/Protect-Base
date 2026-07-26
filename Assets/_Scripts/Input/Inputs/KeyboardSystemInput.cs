#region

using System;
using UnityEngine.InputSystem;
using Zenject;

#endregion

public class KeyboardSystemInput : ISystemInput, IInitializable, IDisposable
{
    private readonly GameInputsAM _gameInput;

    public KeyboardSystemInput(GameInputsAM gameInput)
    {
        _gameInput = gameInput;
    }

    #region Events

    public event Action DYMNetworkToggle;

    public event Action DutyModeToggle;

    public event Action<int> SwitchActive;

    public event Action<int, bool> PowerToggle;

    public event Action SwitchNextActiveFiringMachine;

    public event Action SwitchPreviousActiveFiringMachine;

    public event Action ChooseSingleShootingType;

    public event Action ChooseMultiShootingType;

    public event Action ProjectorToggle;

    public event Action InfraredToggle;

    #endregion

    public void Initialize()
    {
        _gameInput.SystemInput.NetworkToggle.performed += OnNetworkTogglePerformed;
        _gameInput.SystemInput.DutyModeToggleButton.performed += OnDutyModeToggleButtonPerformed;

        _gameInput.SystemInput.SwitchNextActiveFiringMachineButton.performed +=
            OnSwitchNextActiveFiringMachineButtonPerformed;

        _gameInput.SystemInput.SwitchPreviousActiveFiringMachineButton.performed +=
            OnSwitchPreviousActiveFiringMachineButtonPerformed;

        _gameInput.SystemInput.ChooseSingleShootingTypeButton.performed += ChooseSingleShootingTypeButtonPerformed;
        _gameInput.SystemInput.ChooseMultiShootingTypeButton.performed += ChooseMultiShootingTypeButtonPerformed;
        _gameInput.SystemInput.ProjectorToggleButton.performed += OnProjectorToggleButtonPerformed;
        _gameInput.SystemInput.InfraredToggleButton.performed += OnInfraredToggleButtonPerformed;
    }

    private void OnNetworkTogglePerformed(InputAction.CallbackContext _)
    {
        DYMNetworkToggle?.Invoke();
    }

    private void OnDutyModeToggleButtonPerformed(InputAction.CallbackContext _)
    {
        DutyModeToggle?.Invoke();
    }

    private void OnSwitchNextActiveFiringMachineButtonPerformed(InputAction.CallbackContext _)
    {
        SwitchNextActiveFiringMachine?.Invoke();
    }

    private void OnSwitchPreviousActiveFiringMachineButtonPerformed(InputAction.CallbackContext _)
    {
        SwitchPreviousActiveFiringMachine?.Invoke();
    }

    private void ChooseSingleShootingTypeButtonPerformed(InputAction.CallbackContext obj)
    {
        ChooseSingleShootingType?.Invoke();
    }

    private void ChooseMultiShootingTypeButtonPerformed(InputAction.CallbackContext obj)
    {
        ChooseMultiShootingType?.Invoke();
    }

    private void OnProjectorToggleButtonPerformed(InputAction.CallbackContext _)
    {
        ProjectorToggle?.Invoke();
    }

    private void OnInfraredToggleButtonPerformed(InputAction.CallbackContext _)
    {
        InfraredToggle?.Invoke();
    }

    public void Dispose()
    {
        _gameInput.SystemInput.NetworkToggle.performed -= OnNetworkTogglePerformed;
        _gameInput.SystemInput.DutyModeToggleButton.performed -= OnDutyModeToggleButtonPerformed;

        _gameInput.SystemInput.SwitchNextActiveFiringMachineButton.performed -=
            OnSwitchNextActiveFiringMachineButtonPerformed;

        _gameInput.SystemInput.SwitchPreviousActiveFiringMachineButton.performed -=
            OnSwitchPreviousActiveFiringMachineButtonPerformed;

        _gameInput.SystemInput.ProjectorToggleButton.performed -= OnProjectorToggleButtonPerformed;
        _gameInput.SystemInput.InfraredToggleButton.performed -= OnInfraredToggleButtonPerformed;
    }
}