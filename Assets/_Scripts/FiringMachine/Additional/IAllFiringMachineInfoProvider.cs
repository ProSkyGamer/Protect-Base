#region

using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

#endregion

public interface IAllFiringMachineInfoProvider
{
    public event Action<IFiringMachineDataProvider> ChangedFiringMachine;

    public UniTask<List<IFiringMachineDataProvider>> GetAllDataProviders();

    public int GetFiringMachineMinNumber();

    public UniTask<int> GetFiringMachineMaxNumber();

    public bool IsAnyEnabled { get; }
}