#region

using System;
using Zenject;

#endregion

public class MultipleCloseWavesUIObserver : IInitializable, IDisposable
{
    private readonly CurrentEditingOperationManager _currentEditingOperationManager;
    private readonly MultipleCloseWavesUI _multipleCloseWavesUI;

    public MultipleCloseWavesUIObserver(CurrentEditingOperationManager currentEditingOperationManager, MultipleCloseWavesUI multipleCloseWavesUI)
    {
        _currentEditingOperationManager = currentEditingOperationManager;
        _multipleCloseWavesUI = multipleCloseWavesUI;
    }

    public void Initialize()
    {
        _currentEditingOperationManager.CurrentOperationUpdated += CurrentEditingOperationManagerCurrentOperationUpdated;

        _multipleCloseWavesUI.CloseWaveDeleted += MultipleCloseWavesUI_OnCloseWaveDeleted;
        _multipleCloseWavesUI.WaveChosen += MultipleCloseWavesUI_OnWaveChosen;

        _multipleCloseWavesUI.ClearAllWaves();
        _multipleCloseWavesUI.Hide();
    }

    private void MultipleCloseWavesUI_OnWaveChosen(OperationWave _)
    {
        _multipleCloseWavesUI.ClearAllWaves();
        _multipleCloseWavesUI.Hide();
    }

    private void MultipleCloseWavesUI_OnCloseWaveDeleted(int deletingWaveIndex)
    {
        _currentEditingOperationManager.RemoveWaveFromCurrentOperation(deletingWaveIndex);
    }

    private void CurrentEditingOperationManagerCurrentOperationUpdated()
    {
        _multipleCloseWavesUI.Hide();
    }

    public void Dispose()
    {
        _currentEditingOperationManager.CurrentOperationUpdated -= CurrentEditingOperationManagerCurrentOperationUpdated;
        _multipleCloseWavesUI.CloseWaveDeleted -= MultipleCloseWavesUI_OnCloseWaveDeleted;
        _multipleCloseWavesUI.WaveChosen -= MultipleCloseWavesUI_OnWaveChosen;
    }
}