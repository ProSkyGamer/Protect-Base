#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class ButtonSelectableItemsNotification : SelectableItemsNotificationDisplayer, IInitializable, IDisposable
{
    #region Variables & References

    [SerializeField] private Transform _notificationTransform;
    [SerializeField] private TextMeshProUGUI _notificationText;

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

        await UniTask.WaitForSeconds(displayTime, cancellationToken: cancellationToken);

        if (hideTime <= 0)
            await UniTask.NextFrame();
        else
            await UniTask.WaitForSeconds(hideTime, cancellationToken: cancellationToken);

        _notificationTransform.gameObject.SetActive(false);

        ThrowHiddenNotification();
    }

    #endregion

    public void Dispose()
    {
        _notificationCancellationToken.Cancel();
    }
}