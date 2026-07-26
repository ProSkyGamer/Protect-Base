#region

using System;
using System.Collections.Generic;
using Zenject;

#endregion

public class SystemInputHandler : IInitializable, IDisposable
{
    private readonly List<ISystemInput> _systemInputs = new();
    private readonly AllFiringMachinesManager _allFiringMachinesManager;
    private readonly List<FiringMachineObserver> _allFiringMachinesObservers = new();
    private readonly DYMNetworkManager _dymNetworkManager;
    private readonly ClientTypeManager _clientTypeManager;

    public SystemInputHandler(List<ISystemInput> systemInputs, ClientTypeManager clientTypeManager,
        AllFiringMachinesManager allFiringMachinesManager, DYMNetworkManager dymNetworkManager,
        List<FiringMachineObserver> allFiringMachinesObservers)
    {
        _systemInputs.AddRange(systemInputs);
        _allFiringMachinesManager = allFiringMachinesManager;
        _dymNetworkManager = dymNetworkManager;
        _clientTypeManager = clientTypeManager;
        _allFiringMachinesObservers.AddRange(allFiringMachinesObservers);
    }

    public void Initialize()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game) return;

        foreach (ISystemInput systemInput in _systemInputs)
        {
            systemInput.DYMNetworkToggle += SystemInput_DYMNetworkToggle;
            systemInput.DutyModeToggle += SystemInput_DutyModeToggle;
            systemInput.SwitchActive += SystemInput_OnSwitchActive;
            systemInput.PowerToggle += SystemInput_OnPowerToggle;
            systemInput.ChooseSingleShootingType += SystemInput_OnChooseSingleShootingType;
            systemInput.ChooseMultiShootingType += SystemInput_OnChooseMultiShootingType;
            systemInput.SwitchNextActiveFiringMachine += SystemInput_OnSwitchNextActiveFiringMachine;
            systemInput.SwitchPreviousActiveFiringMachine += SystemInput_OnSwitchPreviousActiveFiringMachine;
            systemInput.ProjectorToggle += SystemInput_OnProjectorToggle;
            systemInput.InfraredToggle += SystemInput_OnInfraredToggle;
        }
    }

    private void SystemInput_DYMNetworkToggle()
    {
        _dymNetworkManager.ToggleDYMNetwork();
    }

    private void SystemInput_DutyModeToggle()
    {
        _allFiringMachinesManager.DutyModeToggle();
    }

    private void SystemInput_OnSwitchActive(int firingMachineNumber)
    {
        _allFiringMachinesManager.SwitchCurrentFiringMachine(firingMachineNumber, false);
    }

    private void SystemInput_OnPowerToggle(int firingMachineNumber, bool isSendingMessageBack)
    {
        if (!isSendingMessageBack)
        {
            FiringMachineObserver firingMachineObserver =
                _allFiringMachinesObservers.Find(observer=>
                    observer.ObservingFiringMachineNumber == firingMachineNumber);

            if (firingMachineObserver != null)

                firingMachineObserver.InterruptNextPowerSwitchObservation();
        }

        _allFiringMachinesManager.PowerToggle(firingMachineNumber);
    }

    private void SystemInput_OnChooseSingleShootingType()
    {
        _allFiringMachinesManager.ChangeShootingType(ShootingType.Single);
    }

    private void SystemInput_OnChooseMultiShootingType()
    {
        _allFiringMachinesManager.ChangeShootingType(ShootingType.Multi);
    }

    private void SystemInput_OnSwitchNextActiveFiringMachine()
    {
        _allFiringMachinesManager.SwitchSelectedToNext();
    }

    private void SystemInput_OnSwitchPreviousActiveFiringMachine()
    {
        _allFiringMachinesManager.SwitchSelectedToPrevious();
    }

    private void SystemInput_OnProjectorToggle()
    {
        _allFiringMachinesManager.ProjectorToggle();
    }

    private void SystemInput_OnInfraredToggle()
    {
        _allFiringMachinesManager.InfraredToggle();
    }

    public void Dispose()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game) return;

        foreach (ISystemInput systemInput in _systemInputs)
        {
            systemInput.DYMNetworkToggle -= SystemInput_DYMNetworkToggle;
            systemInput.DutyModeToggle -= SystemInput_DutyModeToggle;
            systemInput.SwitchActive -= SystemInput_OnSwitchActive;
            systemInput.PowerToggle -= SystemInput_OnPowerToggle;
            systemInput.ChooseSingleShootingType -= SystemInput_OnChooseSingleShootingType;
            systemInput.ChooseMultiShootingType -= SystemInput_OnChooseMultiShootingType;
            systemInput.SwitchNextActiveFiringMachine -= SystemInput_OnSwitchNextActiveFiringMachine;
            systemInput.SwitchPreviousActiveFiringMachine -= SystemInput_OnSwitchPreviousActiveFiringMachine;
            systemInput.ProjectorToggle -= SystemInput_OnProjectorToggle;
            systemInput.InfraredToggle -= SystemInput_OnInfraredToggle;
        }
    }
}