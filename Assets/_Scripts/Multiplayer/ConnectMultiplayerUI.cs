#region

using System;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class ConnectMultiplayerUI : MonoBehaviour, IInitializable, IDevInterface //TODO delete fully
{
    #region Variables & References

    [SerializeField] private Button _connectGameButton;
    [SerializeField] private Button _connectSettingOperationButton;
    [SerializeField] private Button _connectStnButton;

    public bool IsShown { get; private set; }

    #endregion

    #region Initialization

    public void Initialize()
    {
        _connectGameButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartHost();
            Hide();
        });

        _connectSettingOperationButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            Hide();
        });

        _connectStnButton.onClick.AddListener(() =>
        {
            NetworkManager.Singleton.StartClient();
            Hide();
        });
    }

    #endregion

    #region Visuals

    public event Action VisibilityChanged;

    public void Show()
    {
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        IsShown = false;
        VisibilityChanged?.Invoke();
    }

    #endregion
}