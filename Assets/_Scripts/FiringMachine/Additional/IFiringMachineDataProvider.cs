#region

using System;
using UnityEngine;

#endregion

public interface IFiringMachineDataProvider
{
    public event Action<int> HealthChanged;

    public event Action<int> HealthDepleted;

    public event Action AmmoCountChanged;

    public event Action PovStatusChanged;

    public event Action ActiveChanged;

    public int FiringMachineNumber { get; }

    public IReadonlyHealthComponent ReadonlyHealthComponent { get; }

    public bool IsActive { get; }

    public Vector3 CurrentEulerAngles { get; }

    public Vector3 MinEulerAngles { get; }

    public Vector3 MaxEulerAngles { get; }

    public int ExplosiveBlockDistance { get; }
    public int MinExplosiveBlockDistance { get; }
    public int MaxExplosiveBlockDistance { get; }

    public int FocusLevel { get; }

    public int ZoomLevel { get; }

    public AmmoType SelectedAmmoType { get; }

    public int CurrentAmmoCount { get; }

    public bool CurrentPoVStatus { get; }

    public int GetShootingBlockAmmoCount(ShootingBlockType shootingBlockType);
    public int GetShootingBlockMaxAmmoCount(ShootingBlockType shootingBlockType);
    public AmmoType GetShootingBlockAmmoType(ShootingBlockType shootingBlockType);
    public bool IsShootingBlockSelected(ShootingBlockType shootingBlockType);
}