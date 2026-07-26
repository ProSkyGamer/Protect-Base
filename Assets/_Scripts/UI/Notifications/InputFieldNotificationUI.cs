#region

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class InputFieldNotificationUI : MonoBehaviour, IInitializable
{
    #region Events

    public event Action<string> NotificationConfirmed;
    public event Action NotificationCanceled;

    #endregion

    #region Variables & References

    [SerializeField] private TextMeshProUGUI _notificationText;
    [SerializeField] private TMP_InputField _inputField;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _confirmButton;

    #endregion

    #region Initialization

    public void Initialize()
    {
        _inputField.onValueChanged.AddListener(_ => { OperationNameChanged(); });

        _confirmButton.onClick.AddListener(ConfirmButtonClicked);
        _cancelButton.onClick.AddListener(CancelButtonClicked);
    }

    private void ConfirmButtonClicked()
    {
        string inputFieldText = _inputField.text;
        NotificationConfirmed?.Invoke(inputFieldText);
    }

    private void CancelButtonClicked()
    {
        NotificationCanceled?.Invoke();
    }

    private void OperationNameChanged()
    {
        _confirmButton.interactable = _inputField.text != "";
    }

    #endregion

    #region Visuals

    public void ShowNotification(string notificationText)
    {
        gameObject.SetActive(true);
        _notificationText.text = notificationText;
        _inputField.text = "";
    }

    public void HideNotification()
    {
        gameObject.SetActive(false);
    }

    #endregion
}