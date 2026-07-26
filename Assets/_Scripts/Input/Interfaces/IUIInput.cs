#region

using System;

#endregion

public interface IUIInput
{
    public event Action Up;

    public event Action Down;

    public event Action Left;

    public event Action Right;

    public event Action Interact;

    public event Action Input0;

    public event Action Input1;

    public event Action Input2;

    public event Action Input3;

    public event Action Input4;

    public event Action Input5;

    public event Action Input6;

    public event Action Input7;

    public event Action Input8;

    public event Action Input9;

    public event Action Backspace;

    public event Action PreSettingStartInput;

    public event Action PreSettingSaveButton;

    public event Action Minus;

    public event Action Clear;

    public event Action LockToggle;

    public event Action Reset;
}