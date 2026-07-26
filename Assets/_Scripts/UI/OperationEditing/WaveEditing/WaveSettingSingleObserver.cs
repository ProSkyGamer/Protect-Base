#region

using System;
using Zenject;

#endregion

public class WaveSettingSingleObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly WaveSettingSingleUI _waveSettingSingleUI;
    private readonly ICurrentEditingOperationDataProvider _currentEditingOperationDataProvider;

    #endregion

    #region Initialization

    public WaveSettingSingleObserver(WaveSettingSingleUI waveSettingSingleUI,
        ICurrentEditingOperationDataProvider currentEditingOperationDataProvider)
    {
        _waveSettingSingleUI = waveSettingSingleUI;
        _currentEditingOperationDataProvider = currentEditingOperationDataProvider;
    }

    public void Initialize()
    {
        _currentEditingOperationDataProvider.CurrentOperationUpdated += CurrentEditingOperationDataProviderCurrentOperationUpdated;

        _waveSettingSingleUI.UpdateVisuals();
        _waveSettingSingleUI.Hide();
    }

    private void CurrentEditingOperationDataProviderCurrentOperationUpdated()
    {
        _waveSettingSingleUI.Hide();
    }

    #endregion

    public void Dispose()
    {
        _currentEditingOperationDataProvider.CurrentOperationUpdated -= CurrentEditingOperationDataProviderCurrentOperationUpdated;
    }
}