#region

using System;
using UnityEngine;

#endregion

public interface IFiringMachineInput
{
    public event Action HideCursor;

    public event Action ShowCursor;

    public event Action<Vector2> Rotation;

    public event Action PowerToggle;

    public event Action ChooseMainFiringBlock;

    public event Action ChooseFirstExplosiveBlock;

    public event Action ChooseSecondExplosiveBlock;

    public event Action FiringModeToggle;

    public event Action Shoot;

    public event Action WarningShot;

    public event Action Reload;

    public event Action RangeRight;

    public event Action RangeLeft;

    public event Action RangeUp;

    public event Action RangeDown;

    public event Action RangeUpDouble;

    public event Action RangeDownDouble;

    public event Action ZoomIn;

    public event Action ZoomOut;

    public event Action FocusPlus;

    public event Action FocusMinus;
}