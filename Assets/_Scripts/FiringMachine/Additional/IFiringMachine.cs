#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public interface IFiringMachine
{
    /// <summary>
    ///     Номер машины для отображения в интерфейсе (+1 к значению индекса)
    /// </summary>
    public int FiringMachineNumber { get; }

    public IHaveHealth HealthComponent { get; }

    public IPovProvider PovProvider { get; }

    public IFiringMachineDataProvider DataProvider { get; }

    public void TurnOn();
    public void TurnOff();

    public void SelectActive();
    public void DeselectActive();

    public void TurnOnProjector();
    public void TurnOffProjector();

    public void StartRotation(Vector2 normalizedRotationDelta);
    public void RotateFiringMachineTo(Vector2Int rotationDeltaSide);
    public void StopRotation();
    public void ChangeExplosiveBlockDistance(bool isNormalDistance, bool isPositive);

    public void ChangeZoomLevel(int deltaLevel);
    public void ChangeFocusLevel(int deltaFocusLevel);

    public void ChangeAmmoType(ShootingBlockType shootingBlockType, AmmoType ammoType);

    public void ChangeToPreSetting(PreSettingSingle preSettingSingle);

    public void ChangeFiringModeToggle();
    public void ChangeShootingBlockType(ShootingBlockType shootingBlockType);
    public void Shoot(ShootingType shootingType);
    public void WarningShoot();
    public void ResetAmmoCount();

    public List<ShootingBlockType> AllShootingBlockTypes { get; }
}