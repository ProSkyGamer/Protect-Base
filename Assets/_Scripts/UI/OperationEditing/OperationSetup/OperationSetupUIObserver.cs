#region

using System;
using System.Collections.Generic;
using Zenject;

#endregion

public class OperationSetupUIObserver : IInitializable, IOperationsStatusListener, IDisposable
{
    #region Variables & References

    private readonly OperationSetupUI _operationSetupUI;
    private readonly OperationTimelineUI _operationTimelineUI;
    private readonly WaveSettingSingleUI _waveSettingSingleUI;
    private readonly MultipleCloseWavesUI _multipleCloseWavesUI;
    private readonly OperationMapManagerUI _operationMapManagerUI;
    private readonly AllOperationPresetsListUI _allOperationPresetsListUI;

    #endregion

    #region Initialization

    public OperationSetupUIObserver(OperationSetupUI operationSetupUI, OperationTimelineUI operationTimelineUI,
        WaveSettingSingleUI waveSettingSingleUI,
        MultipleCloseWavesUI multipleCloseWavesUI, OperationMapManagerUI operationMapManagerUI, AllOperationPresetsListUI allOperationPresetsListUI)
    {
        _operationSetupUI = operationSetupUI;
        _operationTimelineUI = operationTimelineUI;
        _waveSettingSingleUI = waveSettingSingleUI;
        _multipleCloseWavesUI = multipleCloseWavesUI;
        _operationMapManagerUI = operationMapManagerUI;
        _allOperationPresetsListUI = allOperationPresetsListUI;
    }

    public void OperationStarted()
    {
        _operationSetupUI.UpdateVisuals(true);

        _operationSetupUI.Hide();
    }

    public void OperationEnded()
    {
        _operationSetupUI.UpdateVisuals(false);
    }

    public void Initialize()
    {
        _operationSetupUI.OperationPresetsDisplayed += OperationSetupUI_OnOperationPresetsDisplayed;
        _operationSetupUI.OperationAddedWave += OperationSetupUI_OnOperationAddedWave;
        _operationSetupUI.InterfaceInteraction += OperationSetupUI_OnInterfaceInteraction;
        _operationSetupUI.LocalInterfaceClosed += OperationSetupUI_OnLocalInterfaceClosed;

        _operationTimelineUI.TimelinePointSelected += OperationTimelineUI_OnTimelinePointSelected;
        _multipleCloseWavesUI.WaveChosen += MultipleCloseWavesUI_OnWaveChosen;

        _waveSettingSingleUI.WaveHidden += WaveSettingSingleUI_OnWaveHidden;
        _multipleCloseWavesUI.InterfaceHidden += MultipleCloseWavesUI_OnInterfaceHidden;

        _operationSetupUI.Hide();
    }

    private void OperationSetupUI_OnOperationPresetsDisplayed()
    {
        _allOperationPresetsListUI.Show();
        _waveSettingSingleUI.Hide();
        _multipleCloseWavesUI.Hide();
    }

    private void OperationSetupUI_OnOperationAddedWave()
    {
        _operationTimelineUI.ShowPhantomTimelinePoint();
        _waveSettingSingleUI.DisplayBlankWave();
    }

    private void MultipleCloseWavesUI_OnWaveChosen(OperationWave chosenWave)
    {
        _waveSettingSingleUI.SetWaveData(chosenWave);
    }

    private void OperationSetupUI_OnInterfaceInteraction()
    {
        _operationMapManagerUI.InterfaceClick();
    }

    private void OperationSetupUI_OnLocalInterfaceClosed()
    {
        if (_waveSettingSingleUI.IsDisplaying)
            _operationMapManagerUI.InterfaceClick();
        else if (_allOperationPresetsListUI.IsDisplaying)
            _allOperationPresetsListUI.Hide();
        else if (_multipleCloseWavesUI.IsDisplaying)
            _multipleCloseWavesUI.Hide();
    }

    private void OperationTimelineUI_OnTimelinePointSelected(IReadOnlyList<OperationWave> allOperationWaves)
    {
        if (allOperationWaves.Count == 1)
        {
            _waveSettingSingleUI.SetWaveData(allOperationWaves[0]);
            _multipleCloseWavesUI.Hide();
        }
        else
        {
            _multipleCloseWavesUI.DisplayWaves(allOperationWaves);
            _waveSettingSingleUI.Hide();
        }
    }

    private void WaveSettingSingleUI_OnWaveHidden(bool isNewWave)
    {
        if (isNewWave)
            _operationTimelineUI.HidePhantomTimelinePoint();
        else
            _operationTimelineUI.TurnOffAllActivePoints();
    }

    private void MultipleCloseWavesUI_OnInterfaceHidden()
    {
        _operationTimelineUI.TurnOffAllActivePoints();
    }

    #endregion

    public void Dispose()
    {
        _operationSetupUI.OperationPresetsDisplayed -= OperationSetupUI_OnOperationPresetsDisplayed;
        _operationSetupUI.OperationAddedWave -= OperationSetupUI_OnOperationAddedWave;
        _operationSetupUI.InterfaceInteraction -= OperationSetupUI_OnInterfaceInteraction;
        _operationSetupUI.LocalInterfaceClosed -= OperationSetupUI_OnLocalInterfaceClosed;

        _operationTimelineUI.TimelinePointSelected -= OperationTimelineUI_OnTimelinePointSelected;
        _multipleCloseWavesUI.WaveChosen -= MultipleCloseWavesUI_OnWaveChosen;

        _waveSettingSingleUI.WaveHidden -= WaveSettingSingleUI_OnWaveHidden;
        _multipleCloseWavesUI.InterfaceHidden -= MultipleCloseWavesUI_OnInterfaceHidden;
    }
}