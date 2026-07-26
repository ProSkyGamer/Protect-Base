#region

using System;
using Zenject;

#endregion

public class FiringMachineManagerObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly AllFiringMachinesManager _allFiringMachinesManager;
    private readonly CustomEventsManager _customEventsManager;

    #endregion

    #region Initialization

    public FiringMachineManagerObserver(AllFiringMachinesManager allFiringMachinesManager,
        CustomEventsManager customEventsManager)
    {
        _allFiringMachinesManager = allFiringMachinesManager;
        _customEventsManager = customEventsManager;
    }

    public void Initialize()
    {
        _allFiringMachinesManager.ChangedFiringMachine += AllFiringMachinesManager_OnChangedFiringMachine;
    }

    private void AllFiringMachinesManager_OnChangedFiringMachine(IFiringMachineDataProvider changedFiringMachine)
    {
        _customEventsManager.AddEvent($"Выбор СУ {changedFiringMachine.FiringMachineNumber}");
    }

    #endregion

    public void Dispose()
    {
        _allFiringMachinesManager.ChangedFiringMachine -= AllFiringMachinesManager_OnChangedFiringMachine;
    }
}