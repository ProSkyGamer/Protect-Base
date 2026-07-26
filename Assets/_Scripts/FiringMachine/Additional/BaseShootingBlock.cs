#region

using System;
using System.Collections.Generic;
using Unity.Netcode;

#endregion

public abstract class BaseShootingBlock : NetworkBehaviour, IOperationStatsDataProvider
{
    public abstract event Action AmmoCountChanged;
    public abstract event Action<OperationSavingStatType, object> DataChanged;

    public abstract AmmoType CurrentAmmoType { get; }

    public abstract int CurrentAmmoCount { get; }

    public abstract int MaxAmmoCount { get; }

    public abstract ShootingBlockType ShootingBlockType { get; }

    public abstract void ResetAmmoCount();
    public abstract void SetAmmoType(AmmoType blockAmmoType);
    public abstract void Shoot(ShootingType shootingType);
}

public class FiringMachineAmmoTypes
{
    public readonly int FiringMachineNumber;
    public readonly IReadOnlyDictionary<ShootingBlockType, AmmoType> AllShootingBlocksAmmoTypes;

    public FiringMachineAmmoTypes(Dictionary<ShootingBlockType, AmmoType> allShootingBlocksAmmoTypes, int firingMachineNumber)
    {
        FiringMachineNumber = firingMachineNumber;
        AllShootingBlocksAmmoTypes = allShootingBlocksAmmoTypes;
    }
}