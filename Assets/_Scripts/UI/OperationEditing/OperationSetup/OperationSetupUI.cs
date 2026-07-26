#region

using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class OperationSetupUI : MonoBehaviour, IInitializable, IDevInterface
{
    #region Events

    public event Action VisibilityChanged;

    public event Action OperationPresetsDisplayed;
    public event Action OperationAddedWave;

    public event Action InterfaceInteraction;
    public event Action LocalInterfaceClosed;

    #endregion

    #region Variables & References

    [SerializeField] private Button _operationPresetsTabButton;
    [SerializeField] private Button _addWaveButton;

    public bool IsShown { get; private set; } = true;

    #endregion

    #region Initialization

    public void Initialize()
    {
        _operationPresetsTabButton.onClick.AddListener(OperationPresetsButtonClicked);

        _addWaveButton.onClick.AddListener(AddWaveButtonClicked);
    }

    private void AddWaveButtonClicked()
    {
        OperationAddedWave?.Invoke();
    }

    private void OperationPresetsButtonClicked()
    {
        OperationPresetsDisplayed?.Invoke();
    }

    public void InteractWithInterface()
    {
        InterfaceInteraction?.Invoke();
    }

    public void CloseLocalInterface()
    {
        LocalInterfaceClosed?.Invoke();
    }

    #endregion

    #region Visual

    public void UpdateVisuals(bool isOperationActive)
    {
        _addWaveButton.interactable = !isOperationActive;
        _operationPresetsTabButton.interactable = !isOperationActive;
    }

    public void Show()
    {
        gameObject.SetActive(true);

        IsShown = true;

        VisibilityChanged?.Invoke();
    }

    public void Hide()
    {
        if (ClientTypeManager.CurrentClientType is ClientType.OperationSettings) return;

        gameObject.SetActive(false);

        IsShown = false;

        VisibilityChanged?.Invoke();
    }

    #endregion
}