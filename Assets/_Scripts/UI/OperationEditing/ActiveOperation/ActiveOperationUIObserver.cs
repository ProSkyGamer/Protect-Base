#region

using Zenject;

#endregion

public class ActiveOperationUIObserver : IInitializable, IOperationsStatusListener, IOperationUpdateListener
{
    #region Variables & References

    private readonly ActiveOperationInfoUI _activeOperationInfoUI;

    #endregion

    #region Initialization

    public ActiveOperationUIObserver(ActiveOperationInfoUI activeOperationInfoUI)
    {
        _activeOperationInfoUI = activeOperationInfoUI;
    }

    public void OperationStarted()
    {
        _activeOperationInfoUI.Show();
        _activeOperationInfoUI.UpdateVisual();
    }

    public void OperationEnded()
    {
        _activeOperationInfoUI.Hide();
    }

    public void UpdateOperationsVisuals()
    {
        _activeOperationInfoUI.UpdateVisual();
    }

    public void Initialize()
    {
        _activeOperationInfoUI.Hide();
    }

    #endregion
}