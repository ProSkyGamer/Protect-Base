#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class WaveSettingSingleUI : MonoBehaviour, IInitializable, IOperationsStatusListener, IDisposable
{
    #region Events

    public event Action<bool> WaveHidden;

    #endregion

    #region Variables & References

    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _closeButton;

    private OperationWave _currentEditingWave;
    private bool _isOperationActive;

    private CurrentEditingOperationManager _currentEditingOperationManager;
    private TemporaryNotificationsManagerUI _temporaryNotificationsManagerUI;
    private readonly List<WaveTabInfo> _allWaveTabs = new();

    public bool IsDisplaying { get; private set; }

    #endregion

    #region Initialize

    [Inject]
    public void Construct(CurrentEditingOperationManager currentEditingOperationManager,
        TemporaryNotificationsManagerUI temporaryNotificationsManagerUI,
        List<WaveTabInfo> allWaveTabs)
    {
        _currentEditingOperationManager = currentEditingOperationManager;
        _temporaryNotificationsManagerUI = temporaryNotificationsManagerUI;

        _allWaveTabs.AddRange(allWaveTabs);
    }

    public void OperationStarted()
    {
        _isOperationActive = true;

        _cancelButton.gameObject.SetActive(false);
        _saveButton.gameObject.SetActive(false);
        _closeButton.gameObject.SetActive(true);
    }

    public void OperationEnded()
    {
        _isOperationActive = false;

        _cancelButton.gameObject.SetActive(true);
        _saveButton.gameObject.SetActive(true);
        _closeButton.gameObject.SetActive(false);
    }

    public void Initialize()
    {
        foreach (WaveTabInfo waveTabInfo in _allWaveTabs)
        {
            waveTabInfo.ResetValueRequested += WaveTabInfo_OnResetValueRequested;
        }

        _cancelButton.onClick.AddListener(CancelButtonClicked);

        _closeButton.onClick.AddListener(CloseButtonClicked);

        _saveButton.onClick.AddListener(SaveButtonClicked);
    }

    private void CloseButtonClicked()
    {
        Hide();
        _currentEditingWave = null;
    }

    private void CancelButtonClicked()
    {
        Hide();
        _currentEditingWave = null;
    }

    private void SaveButtonClicked()
    {
        Dictionary<OperationStatSingle, object> allOperationData = _allWaveTabs.SelectMany(waveTabInfo => waveTabInfo.GetAllTabOperationStats())
            .GroupBy(keyValuePair => keyValuePair.Key).ToDictionary(grouping => grouping.Key, grouping => grouping.Last().Value);

        if (IsAllDataFilled(allOperationData) == false)
        {
            _temporaryNotificationsManagerUI.AddNewNotification("Не все данные заполнены!");

            return;
        }

        int waveSpawnTime = (int)allOperationData[OperationStatSingle.WaveSpawnTime];
        EnemyType enemyType = (EnemyType)allOperationData[OperationStatSingle.SpawnEnemyType];
        int enemyCount = (int)allOperationData[OperationStatSingle.SpawnEnemyCount];
        List<ReadonlyPathPoint> fullPath = allOperationData[OperationStatSingle.EnemyPathPoints] as List<ReadonlyPathPoint>;

        if (IsPathValid(fullPath) == false)
        {
            _temporaryNotificationsManagerUI.AddNewNotification("Недостаточное кол-во точек пути!");

            return;
        }

        float newEnemyHealth = (float)allOperationData[OperationStatSingle.EnemyHealth];
        float newEnemyAtk = (float)allOperationData[OperationStatSingle.EnemyAtk];
        float newEnemySpeed = (float)allOperationData[OperationStatSingle.EnemySpeed];

        if (enemyType != EnemyType.Vehicle)
        {
            if (_currentEditingWave != null)
                _currentEditingOperationManager.EditWaveFromCurrentOperation(_currentEditingWave.WaveIndex, waveSpawnTime, enemyType, enemyCount,
                    fullPath, newEnemyAtk, newEnemyHealth, newEnemySpeed);
            else
                _currentEditingOperationManager.AddWaveToCurrentOperation(waveSpawnTime, enemyType, enemyCount, fullPath, newEnemyAtk,
                    newEnemyHealth, newEnemySpeed);
        }
        else
        {
            int enemySummonsCount = (int)allOperationData[OperationStatSingle.EnemySummonsCount];

            if (_currentEditingWave != null)
                _currentEditingOperationManager.EditWaveFromCurrentOperation(_currentEditingWave.WaveIndex, waveSpawnTime, enemyType, enemyCount,
                    fullPath, newEnemyAtk, newEnemyHealth, newEnemySpeed, enemySummonsCount);
            else
                _currentEditingOperationManager.AddWaveToCurrentOperation(waveSpawnTime, enemyType, enemyCount, fullPath, newEnemyAtk,
                    newEnemyHealth, newEnemySpeed, enemySummonsCount);
        }

        _currentEditingWave = null;
    }

    private void WaveTabInfo_OnResetValueRequested(EnemyType enemyType)
    {
        foreach (WaveTabInfo waveTabInfo in _allWaveTabs)
        {
            waveTabInfo.SoftResetTabInfo(enemyType);
        }
    }

    private static bool IsPathValid(List<ReadonlyPathPoint> fullPath)
    {
        if (fullPath == null || fullPath.Count == 0)
            return false;

        PathPointType lastPathPointType = fullPath[^1].PathPointType;

        if (lastPathPointType is not (PathPointType.FinalDestinationPoint
            or PathPointType.FinalDroneDestinationPoint))
            return false;

        return true;
    }

    private bool IsAllDataFilled(Dictionary<OperationStatSingle, object> allOperationData)
    {
        if (allOperationData.ContainsKey(OperationStatSingle.WaveSpawnTime) == false)
            return false;

        if (allOperationData.ContainsKey(OperationStatSingle.SpawnEnemyType) == false)
            return false;

        if (allOperationData.ContainsKey(OperationStatSingle.SpawnEnemyCount) == false)
            return false;

        if (allOperationData.ContainsKey(OperationStatSingle.EnemyPathPoints) == false)
            return false;

        if (allOperationData.ContainsKey(OperationStatSingle.EnemyHealth) == false)
            return false;

        if (allOperationData.ContainsKey(OperationStatSingle.EnemyAtk) == false)
            return false;

        if (allOperationData.ContainsKey(OperationStatSingle.EnemySpeed) == false)
            return false;

        return true;
    }

    #endregion

    #region Set

    public void SetWaveData(OperationWave waveSingle)
    {
        Show();

        _currentEditingWave = waveSingle;

        foreach (WaveTabInfo waveTabInfo in _allWaveTabs)
        {
            waveTabInfo.SetWaveData(waveSingle);
        }
    }

    public void DisplayBlankWave()
    {
        Show();
    }

    #endregion

    #region Visual

    public void Show()
    {
        IsDisplaying = true;

        gameObject.SetActive(true);

        foreach (WaveTabInfo waveTabInfo in _allWaveTabs)
        {
            waveTabInfo.HardResetTabInfo();
        }
    }

    public void Hide()
    {
        IsDisplaying = false;

        foreach (WaveTabInfo waveTabInfo in _allWaveTabs)
        {
            waveTabInfo.CancelCurrentActions();
            waveTabInfo.HardResetTabInfo();
        }

        WaveHidden?.Invoke(_currentEditingWave == null);

        gameObject.SetActive(false);
    }

    public void UpdateVisuals()
    {
        _cancelButton.gameObject.SetActive(_isOperationActive == false);
        _saveButton.gameObject.SetActive(_isOperationActive == false);
        _closeButton.gameObject.SetActive(_isOperationActive);
    }

    #endregion

    public void Dispose()
    {
        foreach (WaveTabInfo waveTabInfo in _allWaveTabs)
        {
            waveTabInfo.ResetValueRequested -= WaveTabInfo_OnResetValueRequested;
        }
    }
}