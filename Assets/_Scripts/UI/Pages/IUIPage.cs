#region

using System;

#endregion

public interface IUIPage
{
    public event Action HideRequested;

    public void Show();
    public void Hide();
    public void RequestHide();

    public bool IsCanHide { get; }

    public void UpdateVisuals();
}