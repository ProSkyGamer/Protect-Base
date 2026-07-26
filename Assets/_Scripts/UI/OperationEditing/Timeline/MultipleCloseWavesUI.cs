#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class MultipleCloseWavesUI : MonoBehaviour, IDisposable
{
    #region Events

    public event Action InterfaceHidden;

    public event Action<OperationWave> WaveChosen;

    public event Action<int> CloseWaveDeleted;

    #endregion

    #region Variables & References

    private CloseWavesUIFactory _closeWavesUIFactory;

    public bool IsDisplaying { get; private set; }

    private readonly List<CloseWaveSingleUI> _allCloseWaveSingles = new();

    #endregion

    #region Initialization

    [Inject]
    public void Construct(CloseWavesUIFactory closeWavesUIFactory)
    {
        _closeWavesUIFactory = closeWavesUIFactory;
    }

    #endregion

    #region Waves

    public void DisplayWaves(IReadOnlyList<OperationWave> wavesSingle)
    {
        Show();
        ClearAllWaves();

        foreach (OperationWave waveSingle in wavesSingle)
        {
            CloseWaveSingleUI newCloseWave = _closeWavesUIFactory.Create(waveSingle);

            _allCloseWaveSingles.Add(newCloseWave);

            newCloseWave.WaveChosen += NewCloseWaveWaveChosen;
            newCloseWave.WaveDeleteRequested += NewClose_WaveDeleteRequested;
        }
    }

    private void NewClose_WaveDeleteRequested(OperationWave deletingWave)
    {
        CloseWaveDeleted?.Invoke(deletingWave.WaveIndex);
    }

    private void NewCloseWaveWaveChosen(OperationWave chosenWave)
    {
        WaveChosen?.Invoke(chosenWave);
    }

    public void ClearAllWaves()
    {
        foreach (CloseWaveSingleUI closeWaveUI in _allCloseWaveSingles)
        {
            Destroy(closeWaveUI.gameObject);
        }

        ClearAllWaveArray();
    }

    private void ClearAllWaveArray()
    {
        foreach (CloseWaveSingleUI closeWaveSingle in _allCloseWaveSingles)
        {
            closeWaveSingle.WaveChosen -= NewCloseWaveWaveChosen;

            closeWaveSingle.WaveDeleteRequested -= NewClose_WaveDeleteRequested;
        }

        _allCloseWaveSingles.Clear();
    }

    #endregion

    #region Visual

    public void Show()
    {
        IsDisplaying = true;
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        IsDisplaying = false;
        gameObject.SetActive(false);

        InterfaceHidden?.Invoke();
    }

    #endregion

    public void Dispose()
    {
        ClearAllWaveArray();
    }
}