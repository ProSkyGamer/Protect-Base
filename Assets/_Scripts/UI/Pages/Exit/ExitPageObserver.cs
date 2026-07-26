#region

using System;
using Zenject;

#endregion

public class ExitPageObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly ExitPageUI _exitPageUI;

    #endregion

    #region Initialization

    public ExitPageObserver(ExitPageUI exitPageUI)
    {
        _exitPageUI = exitPageUI;
    }

    public void Initialize()
    {
        _exitPageUI.PageShown += ExitPageUI_OnPageShown;
    }

    private void ExitPageUI_OnPageShown()
    {
        _exitPageUI.UpdateVisual();
    }

    #endregion

    public void Dispose()
    {
        _exitPageUI.PageShown -= ExitPageUI_OnPageShown;
    }
}