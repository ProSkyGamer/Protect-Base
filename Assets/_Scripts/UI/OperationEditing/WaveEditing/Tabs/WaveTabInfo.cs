#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public abstract class WaveTabInfo : MonoBehaviour, IInitializable, IDisposable, IOperationsStatusListener
{
    #region Events

    public event Action<EnemyType> ResetValueRequested;
    public event Action<EnemyType> TabSoftReset;

    #endregion

    #region Variables & References

    [SerializeField] private Transform _waveTabTransform;
    [SerializeField] private Button _waveTabButton;
    [SerializeField] private Image _waveTabStateImage;
    [SerializeField] private Sprite _tabClosedSprite;
    [SerializeField] private Sprite _tabOpenSprite;

    private bool _currentTabState;

    #endregion

    #region Initialization

    public abstract void OperationStarted();

    public abstract void OperationEnded();

    public virtual void Initialize()
    {
        _waveTabButton.onClick.AddListener(OnWaveTabButtonClicked);

        ChangeTabState(true);
    }

    private void OnWaveTabButtonClicked()
    {
        ChangeTabState(!_currentTabState);
    }

    #endregion

    #region Reset

    protected void RequestTabReset(EnemyType enemyType)
    {
        ResetValueRequested?.Invoke(enemyType);
    }

    public virtual void SoftResetTabInfo(EnemyType enemyType)
    {
        TabSoftReset?.Invoke(enemyType);
    }

    public abstract void HardResetTabInfo();

    public abstract void CancelCurrentActions();

    #endregion

    #region Visuals

    private void ChangeTabState(bool newState)
    {
        _waveTabTransform.gameObject.SetActive(newState);
        _waveTabStateImage.sprite = newState ? _tabOpenSprite : _tabClosedSprite;

        _currentTabState = newState;
    }

    public abstract void SetWaveData(OperationWave operationWave);

    public void FullyHideTab()
    {
        _waveTabTransform.gameObject.SetActive(false);
        _waveTabButton.gameObject.SetActive(false);
    }

    public void ShowTabButton()
    {
        _waveTabButton.gameObject.SetActive(true);
    }

    #endregion

    #region Get

    public abstract Dictionary<OperationStatSingle, object> GetAllTabOperationStats();

    #endregion

    public abstract void Dispose();
}