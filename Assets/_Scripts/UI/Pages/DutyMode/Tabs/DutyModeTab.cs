#region

using System;
using UnityEngine;
using Zenject;

#endregion

public abstract class DutyModeTab : MonoBehaviour, IInitializable, IDisposable
{
    public abstract void UpdateTabVisual(IFiringMachineDataProvider currentFiringMachineDataProvider);

    public abstract DutyModeTabType DutyModeTabType { get; }
    public abstract void Initialize();
    public abstract void Dispose();
}