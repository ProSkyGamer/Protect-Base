#region

using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class OperationSetupControlUI : MonoBehaviour, IInitializable
{
    #region Events

    public event Action CurrentOperationReset;
    public event Action CurrentOperationStarted;
    public event Action CurrentOperationsStopped;

    #endregion

    #region Variables & References

    [SerializeField] private Button _clearCurrentOperationWavesTabButton;
    [SerializeField] private Button _startOperationButton;
    [SerializeField] private Button _stopOperationButton;

    #endregion

    #region Initialization

    public void Initialize()
    {
        _clearCurrentOperationWavesTabButton.onClick.AddListener(ClearOperationWavesButtonPressed);

        _startOperationButton.onClick.AddListener(StartOperationButtonPressed);

        _stopOperationButton.onClick.AddListener(StopOperationButtonClicked);
    }

    private void StopOperationButtonClicked()
    {
        CurrentOperationsStopped?.Invoke();
    }

    private void StartOperationButtonPressed()
    {
        CurrentOperationStarted?.Invoke();
    }

    private void ClearOperationWavesButtonPressed()
    {
        CurrentOperationReset?.Invoke();
    }

    #endregion

    #region Visuals

    public void UpdateVisibility(bool isOperationActive)
    {
        _clearCurrentOperationWavesTabButton.interactable = !isOperationActive;
        _startOperationButton.gameObject.SetActive(!isOperationActive);
        _stopOperationButton.gameObject.SetActive(isOperationActive);
    }

    #endregion
}