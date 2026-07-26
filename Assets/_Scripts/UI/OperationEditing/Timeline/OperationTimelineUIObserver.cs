#region

using System;
using Zenject;

#endregion

public class OperationTimelineUIObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly CurrentEditingOperationManager _currentEditingOperationManager;
    private readonly OperationTimelineUI _operationTimelineUI;

    #endregion

    #region Initialization

    public OperationTimelineUIObserver(CurrentEditingOperationManager currentEditingOperationManager, OperationTimelineUI operationTimelineUI)
    {
        _currentEditingOperationManager = currentEditingOperationManager;
        _operationTimelineUI = operationTimelineUI;
    }

    public void Initialize()
    {
        _operationTimelineUI.WaveDeleted += OperationTimelineUI_OnWaveDeleted;

        _currentEditingOperationManager.CurrentOperationUpdated += CurrentEditingOperationManagerCurrentOperationUpdated;

        ReadonlyOperationData currentOperation = _currentEditingOperationManager.GetCurrentEditingOperationSingle();
        _operationTimelineUI.UpdateTimeline(currentOperation);
    }

    private void OperationTimelineUI_OnWaveDeleted(int deletingWaveIndex)
    {
        _currentEditingOperationManager.RemoveWaveFromCurrentOperation(deletingWaveIndex);
    }

    private void CurrentEditingOperationManagerCurrentOperationUpdated()
    {
        ReadonlyOperationData currentOperation = _currentEditingOperationManager.GetCurrentEditingOperationSingle();

        _operationTimelineUI.UpdateTimeline(currentOperation);
    }

    #endregion

    public void Dispose()
    {
        _operationTimelineUI.WaveDeleted -= OperationTimelineUI_OnWaveDeleted;
        _currentEditingOperationManager.CurrentOperationUpdated -= CurrentEditingOperationManagerCurrentOperationUpdated;
    }
}