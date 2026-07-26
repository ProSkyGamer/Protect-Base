#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class FiringMachineInputHandler : IInitializable, IDisposable
{
    private readonly List<IFiringMachineInput> _firingMachineInputs = new();
    private readonly AllFiringMachinesManager _allFiringMachinesManager;
    private readonly CursorManager _cursorManager;

    public FiringMachineInputHandler(List<IFiringMachineInput> firingMachineInput,
        AllFiringMachinesManager allFiringMachinesManager, CursorManager cursorManager)
    {
        _firingMachineInputs.AddRange(firingMachineInput);
        _firingMachineInputs = firingMachineInput;
        _allFiringMachinesManager = allFiringMachinesManager;
        _cursorManager = cursorManager;
    }

    public void Initialize()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game) return;

        foreach (IFiringMachineInput firingMachineInput in _firingMachineInputs)
        {
            firingMachineInput.Rotation += FiringMachineInput_OnRotation;
            firingMachineInput.HideCursor += FiringMachineInput_OnHideCursor;
            firingMachineInput.ShowCursor += FiringMachineInput_OnShowCursor;
            firingMachineInput.PowerToggle += FiringMachineInput_OnPowerToggle;
            firingMachineInput.ChooseMainFiringBlock += FiringMachineInput_OnChooseMainFiringBlock;
            firingMachineInput.ChooseFirstExplosiveBlock += FiringMachineInput_OnChooseFirstExplosiveBlock;
            firingMachineInput.ChooseSecondExplosiveBlock += FiringMachineInput_OnChooseSecondExplosiveBlock;
            firingMachineInput.FiringModeToggle += FiringMachineInput_OnFiringModeToggle;
            firingMachineInput.Shoot += FiringMachineInput_OnShoot;
            firingMachineInput.WarningShot += FiringMachineInput_OnWarningShot;
            firingMachineInput.Reload += FiringMachineInput_OnReload;
            firingMachineInput.RangeRight += FiringMachineInput_OnRangeRight;
            firingMachineInput.RangeLeft += FiringMachineInput_OnRangeLeft;
            firingMachineInput.RangeUp += FiringMachineInput_OnRangeUp;
            firingMachineInput.RangeDown += FiringMachineInput_OnRangeDown;
            firingMachineInput.RangeUpDouble += FiringMachineInput_OnRangeUpDouble;
            firingMachineInput.RangeDownDouble += FiringMachineInput_OnRangeDownDouble;
            firingMachineInput.ZoomIn += FiringMachineInput_OnZoomIn;
            firingMachineInput.ZoomOut += FiringMachineInput_OnZoomOut;
            firingMachineInput.FocusPlus += FiringMachineInput_OnFocusPlus;
            firingMachineInput.FocusMinus += FiringMachineInput_OnFocusMinus;
        }
    }

    private void FiringMachineInput_OnShowCursor()
    {
        _cursorManager.ShowCursor();
    }

    private void FiringMachineInput_OnHideCursor()
    {
        _cursorManager.HideCursor();
    }

    private void FiringMachineInput_OnRotation(Vector2 normalizedRotationDelta)
    {
        if (normalizedRotationDelta != Vector2.zero)
            _allFiringMachinesManager.StartCurrentFiringMachineRotation(normalizedRotationDelta);
        else
            _allFiringMachinesManager.StopCurrentFiringMachineRotation();
    }

    private void FiringMachineInput_OnPowerToggle()
    {
        _allFiringMachinesManager.PowerToggle();
    }

    private void FiringMachineInput_OnChooseMainFiringBlock()
    {
        _allFiringMachinesManager.ChangeCurrentFiringBlock(ShootingBlockType.Main);
    }

    private void FiringMachineInput_OnChooseFirstExplosiveBlock()
    {
        _allFiringMachinesManager.ChangeCurrentFiringBlock(ShootingBlockType.ExplosiveOne);
    }

    private void FiringMachineInput_OnChooseSecondExplosiveBlock()
    {
        _allFiringMachinesManager.ChangeCurrentFiringBlock(ShootingBlockType.ExplosiveTwo);
    }

    private void FiringMachineInput_OnFiringModeToggle()
    {
        _allFiringMachinesManager.FiringModeToggle();
    }

    private void FiringMachineInput_OnShoot()
    {
        _allFiringMachinesManager.ShootCurrentFiringMachine();
    }

    private void FiringMachineInput_OnWarningShot()
    {
        _allFiringMachinesManager.WarningShootCurrentFiringMachine();
    }

    private void FiringMachineInput_OnReload()
    {
        _allFiringMachinesManager.ReloadCurrentFiringMachine();
    }

    private void FiringMachineInput_OnRangeRight()
    {
        _allFiringMachinesManager.RotateFiringMachine(true);
    }

    private void FiringMachineInput_OnRangeLeft()
    {
        _allFiringMachinesManager.RotateFiringMachine(false);
    }

    private void FiringMachineInput_OnRangeUp()
    {
        _allFiringMachinesManager.ChangeExplosiveBlockDistance(true, true);
    }

    private void FiringMachineInput_OnRangeDown()
    {
        _allFiringMachinesManager.ChangeExplosiveBlockDistance(true, false);
    }

    private void FiringMachineInput_OnRangeUpDouble()
    {
        _allFiringMachinesManager.ChangeExplosiveBlockDistance(false, true);
    }

    private void FiringMachineInput_OnRangeDownDouble()
    {
        _allFiringMachinesManager.ChangeExplosiveBlockDistance(false, false);
    }

    private void FiringMachineInput_OnZoomIn()
    {
        _allFiringMachinesManager.ChangeZoomLevel(-1);
    }

    private void FiringMachineInput_OnZoomOut()
    {
        _allFiringMachinesManager.ChangeZoomLevel(1);
    }

    private void FiringMachineInput_OnFocusPlus()
    {
        _allFiringMachinesManager.ChangeFocusLevel(1);
    }

    private void FiringMachineInput_OnFocusMinus()
    {
        _allFiringMachinesManager.ChangeFocusLevel(-1);
    }

    public void Dispose()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game) return;

        foreach (IFiringMachineInput firingMachineInput in _firingMachineInputs)
        {
            firingMachineInput.Rotation -= FiringMachineInput_OnRotation;
            firingMachineInput.PowerToggle -= FiringMachineInput_OnPowerToggle;
            firingMachineInput.ChooseMainFiringBlock -= FiringMachineInput_OnChooseMainFiringBlock;
            firingMachineInput.ChooseFirstExplosiveBlock -= FiringMachineInput_OnChooseFirstExplosiveBlock;
            firingMachineInput.ChooseSecondExplosiveBlock -= FiringMachineInput_OnChooseSecondExplosiveBlock;
            firingMachineInput.FiringModeToggle -= FiringMachineInput_OnFiringModeToggle;
            firingMachineInput.Shoot -= FiringMachineInput_OnShoot;
            firingMachineInput.WarningShot -= FiringMachineInput_OnWarningShot;
            firingMachineInput.Reload -= FiringMachineInput_OnReload;
            firingMachineInput.RangeRight -= FiringMachineInput_OnRangeRight;
            firingMachineInput.RangeLeft -= FiringMachineInput_OnRangeLeft;
            firingMachineInput.RangeUp -= FiringMachineInput_OnRangeUp;
            firingMachineInput.RangeDown -= FiringMachineInput_OnRangeDown;
            firingMachineInput.RangeUpDouble -= FiringMachineInput_OnRangeUpDouble;
            firingMachineInput.RangeDownDouble -= FiringMachineInput_OnRangeDownDouble;
            firingMachineInput.ZoomIn -= FiringMachineInput_OnZoomIn;
            firingMachineInput.ZoomOut -= FiringMachineInput_OnZoomOut;
            firingMachineInput.FocusPlus -= FiringMachineInput_OnFocusPlus;
            firingMachineInput.FocusMinus -= FiringMachineInput_OnFocusMinus;
        }
    }
}