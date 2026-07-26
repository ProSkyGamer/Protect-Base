#region

using System.Collections.Generic;
using System.Linq;

#endregion

public class SerializableFiringMachineAmmoTypes
{
    public int FiringMachineNumber;
    public Dictionary<ShootingBlockType, AmmoType> AllShootingBlocksAmmoTypes;

    public SerializableFiringMachineAmmoTypes()
    {
    }

    public SerializableFiringMachineAmmoTypes(FiringMachineAmmoTypes firingMachineAmmoTypes)
    {
        FiringMachineNumber = firingMachineAmmoTypes.FiringMachineNumber;

        AllShootingBlocksAmmoTypes =
            firingMachineAmmoTypes.AllShootingBlocksAmmoTypes.ToDictionary(ammoTypes => ammoTypes.Key,
                ammoTypes => ammoTypes.Value);
    }

    public FiringMachineAmmoTypes GetFiringMachineAmmoTypes()
    {
        FiringMachineAmmoTypes firingMachineAmmoTypes = new(AllShootingBlocksAmmoTypes, FiringMachineNumber);

        return firingMachineAmmoTypes;
    }
}