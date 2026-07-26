#region

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class FullscreenNotificationUI : MonoBehaviour, IInitializable
{
    #region Events

    public event Action Confirmed;

    public event Action Canceled;

    #endregion

    #region Variables & References

    [SerializeField] private TextMeshProUGUI _notificationText;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _cancelButton;

    #endregion

    #region Initialize

    public void Initialize()
    {
        _confirmButton.onClick.AddListener(OnConfirmReloadButtonPressed);
        _cancelButton.onClick.AddListener(OnCancelReloadButtonPressed);
    }

    private void OnConfirmReloadButtonPressed()
    {
        Confirmed?.Invoke();
    }

    private void OnCancelReloadButtonPressed()
    {
        Canceled?.Invoke();
    }

    #endregion

    #region Visuals

    public void Show(string notificationText)
    {
        gameObject.SetActive(true);

        _notificationText.text = notificationText;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion
}