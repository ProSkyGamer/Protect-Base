#region

using System;
using UnityEngine;

#endregion

public interface IDevInput
{
    public event Action LogsToggle;

    public event Action SettingsShow;

    public event Action OperationManagerToggle;

    public event Action ChangeSkybox;

    public event Action MouseClick;

    public event Action CloseInterface;

    public void StartListeningForMousePosition();

    public event Action<Vector2> MousePositionChanged;

    public void StopListeningForMousePosition();
}