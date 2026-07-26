#region

using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

#endregion

[RequireComponent(typeof(Button))]
public class ButtonSelectItemSingleUI : BaseSelectedItemSingleUI
{
    #region Variables & References

    [FormerlySerializedAs("_buttonNotificationDisplayer")] [SerializeField]
    private SelectableItemsNotificationDisplayer _buttonSelectableItemsNotificationDisplayer;

    private Button _currentButton;

    #endregion

    #region Initialization

    public override void Initialize()
    {
        base.Initialize();

        if (_currentButton == null)
            _currentButton = GetComponent<Button>();

        _currentButton.interactable = false;
    }

    #endregion

    #region Select

    public override void SelectItem()
    {
        base.SelectItem();

        EventSystem.current.SetSelectedGameObject(gameObject);
    }

    #endregion

    #region Interact

    public override void InteractWithItem()
    {
        base.InteractWithItem();

        if (_isInteractionUnlocked == false)
            return;

        if (IsDevInterfaceShowing)
            return;

        _currentButton.onClick.Invoke();
    }

    #endregion

    #region Notifications

    public async UniTask DisplayNotification(string notificationText)
    {
        if (_buttonSelectableItemsNotificationDisplayer == null)
            return;

        await DisplayNotificationAsync(notificationText);
    }

    private async UniTask DisplayNotificationAsync(string notificationText)
    {
        _isInteractionUnlocked = false;

        bool isNotificationHidden = false;
        _buttonSelectableItemsNotificationDisplayer.NotificationHidden += ButtonNotificationDisplayerOnNotificationHidden;

        void ButtonNotificationDisplayerOnNotificationHidden()
        {
            isNotificationHidden = true;
        }

        _buttonSelectableItemsNotificationDisplayer.ShowNotification(notificationText);

        await UniTask.WaitUntil(() => isNotificationHidden);

        _buttonSelectableItemsNotificationDisplayer.NotificationHidden -= ButtonNotificationDisplayerOnNotificationHidden;

        _isInteractionUnlocked = true;
    }

    #endregion

    #region Get

    public Button GetButtonComponent()
    {
        if (_currentButton == null)
            _currentButton = GetComponent<Button>();

        return _currentButton;
    }

    #endregion
}