#region

using System;

#endregion

public interface ISystemInput
{
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
}