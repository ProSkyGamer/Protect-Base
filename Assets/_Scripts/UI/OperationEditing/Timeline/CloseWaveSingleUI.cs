#region

using System;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class CloseWaveSingleUI : SingleWaveMainInfoUI
{
    #region Events

    public event Action<OperationWave> WaveChosen;
    public event Action<OperationWave> WaveDeleteRequested;

    #endregion

    #region Variables & References

    [SerializeField] private Button _deleteWaveButton;
    private Button _closeWaveSingleButton;

    #endregion

    #region Initialization

    public override void Initialize()
    {
        _closeWaveSingleButton = GetComponent<Button>();

        base.Initialize();

        _closeWaveSingleButton.onClick.AddListener(CloseWaveButtonClicked);

        _deleteWaveButton.onClick.AddListener(DeleteWaveButtonClicked);
    }

    private void DeleteWaveButtonClicked()
    {
        WaveDeleteRequested?.Invoke(OperationWave);
    }

    private void CloseWaveButtonClicked()
    {
        WaveChosen?.Invoke(OperationWave);
    }

    #endregion
}