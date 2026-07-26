#region

using System;
using Zenject;

#endregion

public class OperationSetupControlsObserver : IInitializable, IOperationsStatusListener, IDisposable
{
    #region Variables & References

    private readonly OperationSetupControlUI _operationSetupControlUI;
    private readonly OperationsManager _operationsManager;
    private readonly CurrentEditingOperationManager _currentEditingOperationManager;

    #endregion

    #region Initialization

    public void OperationStarted()
    {
        _operationSetupControlUI.UpdateVisibility(true);
    }

    public void OperationEnded()
    {
        _operationSetupControlUI.UpdateVisibility(false);
    }

    public OperationSetupControlsObserver(OperationSetupControlUI operationSetupControlUI, OperationsManager operationsManager,
        CurrentEditingOperationManager currentEditingOperationManager)
    {
        _operationSetupControlUI = operationSetupControlUI;
        _operationsManager = operationsManager;
        _currentEditingOperationManager = currentEditingOperationManager;
    }

    public void Initialize()
    {
        _operationSetupControlUI.CurrentOperationStarted += OperationSetupControlUI_OnCurrentOperationStarted;
        _operationSetupControlUI.CurrentOperationsStopped += OperationSetupControlUI_OnCurrentOperationsStopped;
        _operationSetupControlUI.CurrentOperationReset += OperationSetupControlUI_OnCurrentOperationReset;

        _operationSetupControlUI.UpdateVisibility(false);
    }

    private void OperationSetupControlUI_OnCurrentOperationStarted()
    {
        ReadonlyOperationData operationSingle = _currentEditingOperationManager.GetCurrentEditingOperationSingle();

        _operationsManager.StartOperation(operationSingle);
    }

    private void OperationSetupControlUI_OnCurrentOperationsStopped()
    {
        _operationsManager.StopOperation();
    }

    private void OperationSetupControlUI_OnCurrentOperationReset()
    {
        _currentEditingOperationManager.ResetCurrentEditingOperation();
    }

    #endregion

    public void Dispose()
    {
        _operationSetupControlUI.CurrentOperationStarted -= OperationSetupControlUI_OnCurrentOperationStarted;
        _operationSetupControlUI.CurrentOperationsStopped -= OperationSetupControlUI_OnCurrentOperationsStopped;
        _operationSetupControlUI.CurrentOperationReset -= OperationSetupControlUI_OnCurrentOperationReset;
    }
}