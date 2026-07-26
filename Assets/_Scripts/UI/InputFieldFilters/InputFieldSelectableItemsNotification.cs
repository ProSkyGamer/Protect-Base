#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class InputFieldSelectableItemsNotification : SelectableItemsNotificationDisplayer, IInitializable, IDisposable
{
    #region Variables & References

    [SerializeField] private Transform _notificationTransform;
    [SerializeField] private TextMeshProUGUI _notificationText;
    [SerializeField] private Image _notificationBackgroundImage;

    private readonly CancellationTokenSource _notificationCancellationToken = new();

    #endregion

    #region Initialization

    public void Initialize()
    {
        _notificationTransform.gameObject.SetActive(false);
    }

    #endregion

    #region Notification

    public override void ShowNotification(string notificationText)
    {
        ShowNotification(notificationText, _displayTime, _hideTime, _notificationCancellationToken.Token).Forget();
    }

    private async UniTaskVoid ShowNotification(string notificationText, float displayTime, float hideTime,
        CancellationToken cancellationToken)
    {
        _notificationTransform.gameObject.SetActive(true);

        _notificationText.text = notificationText;

        Color notificationBackgroundColor = _notificationBackgroundImage.color;
        notificationBackgroundColor.a = 1f;
        _notificationBackgroundImage.color = notificationBackgroundColor;

        Color notificationTextColor = _notificationText.color;
        notificationTextColor.a = 1f;
        _notificationText.color = notificationTextColor;

        await UniTask.WaitForSeconds(displayTime, cancellationToken: cancellationToken);

        await UniTaskAsyncEnumerable.EveryUpdate()
            .TakeUntil(UniTask.Delay(TimeSpan.FromSeconds(hideTime), cancellationToken: cancellationToken))
            .ForEachAsync(_ =>
            {
                notificationBackgroundColor = _notificationBackgroundImage.color;
                notificationBackgroundColor.a -= Time.deltaTime / hideTime;
                _notificationBackgroundImage.color = notificationBackgroundColor;

                notificationTextColor = _notificationText.color;
                notificationTextColor.a -= Time.deltaTime / hideTime;
                _notificationText.color = notificationTextColor;
            }, cancellationToken: cancellationToken);

        _notificationTransform.gameObject.SetActive(false);

        ThrowHiddenNotification();
    }

    #endregion

    public void Dispose()
    {
        _notificationCancellationToken.Cancel();
    }
}