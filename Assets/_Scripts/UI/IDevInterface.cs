#region

using System;

#endregion

public interface IDevInterface
{
    public event Action VisibilityChanged;

    public void Show();

    public void Hide();

    public bool IsShown { get; }
}