#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class SelectedOperationPresetUI : MonoBehaviour, IInitializable
{
    #region Events

    public event Action<SavedOperationData> OperationSelected;

    public event Action<int, string> OperationRewritten;

    #endregion

    #region Variables & References

    private SavedOperationData _currentDisplayingOperationSingle;

    [SerializeField] private Button _cancelCurrentChosenDisplayingOperation;
    [SerializeField] private Button _rewriteCurrentChosenOperation;
    [SerializeField] private Button _choseCurrentDisplayingOperation;

    private FullscreenNotificationUI _fullscreenNotificationUI;
    private WaveMainInfoUIFactory _waveMainInfoUIFactory;
    private readonly List<SingleWaveMainInfoUI> _allCreatedWaves = new();

    [SerializeField] private string _notificationStringFormat =
        "Файл сохранения {0} \nбудет перезаписан на текущие \nвыставленные данные";

    #endregion

    #region Initialization

    [Inject]
    public void Construct(FullscreenNotificationUI fullscreenNotificationUI, WaveMainInfoUIFactory waveMainInfoUIFactory)
    {
        _fullscreenNotificationUI = fullscreenNotificationUI;
        _waveMainInfoUIFactory = waveMainInfoUIFactory;
    }

    public void Initialize()
    {
        _choseCurrentDisplayingOperation.onClick.AddListener(ChoseCurrentDisplayingOperationClicked);

        _cancelCurrentChosenDisplayingOperation.onClick.AddListener(CancelCurrentChosenDisplayingOperationClicked);

        _rewriteCurrentChosenOperation.onClick.AddListener(RewriteCurrentChosenOperationClicked);

        _fullscreenNotificationUI.Hide();
    }

    private void RewriteCurrentChosenOperationClicked()
    {
        string rewritingNotificationString =
            string.Format(_notificationStringFormat, _currentDisplayingOperationSingle.OperationName);

        _fullscreenNotificationUI.Show(rewritingNotificationString);

        _fullscreenNotificationUI.Confirmed += FullscreenNotificationUI_OnConfirmed;
        _fullscreenNotificationUI.Canceled += FullscreenNotificationUI_OnCanceled;
    }

    private void CancelCurrentChosenDisplayingOperationClicked()
    {
        Hide();
    }

    private void ChoseCurrentDisplayingOperationClicked()
    {
        OperationSelected?.Invoke(_currentDisplayingOperationSingle);

        Hide();
    }

    private void FullscreenNotificationUI_OnConfirmed()
    {
        _fullscreenNotificationUI.Hide();

        OperationRewritten?.Invoke(_currentDisplayingOperationSingle.OperationIndex, _currentDisplayingOperationSingle.OperationName);
    }

    private void FullscreenNotificationUI_OnCanceled()
    {
        _fullscreenNotificationUI.Hide();
    }

    private void ClearCurrentOperationWaves()
    {
        foreach (SingleWaveMainInfoUI waveMainInfoUI in _allCreatedWaves)
        {
            Destroy(waveMainInfoUI.gameObject);
        }

        _allCreatedWaves.Clear();
    }

    #endregion

    #region Visuals

    public void Show(SavedOperationData operationData)
    {
        gameObject.SetActive(true);
        ClearCurrentOperationWaves();

        _currentDisplayingOperationSingle = operationData;

        List<OperationWave> displayingWaves =
            operationData.OperationData.AllOperationWaves.OrderBy(operationWave => operationWave.WaveSpawnTime).ToList();

        foreach (OperationWave waveSingle in displayingWaves)
        {
            SingleWaveMainInfoUI newWaveSingle = _waveMainInfoUIFactory.Create(waveSingle);
            _allCreatedWaves.Add(newWaveSingle);
        }
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion
}