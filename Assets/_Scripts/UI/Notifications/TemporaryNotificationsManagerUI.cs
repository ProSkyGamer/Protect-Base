#region

using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

#endregion

public class TemporaryNotificationsManagerUI : MonoBehaviour, IDisposable
{
    #region Variables & References

    [SerializeField] private int _maxNotificationCount = 5;

    private NotificationsSingleUIFactory _notificationsFactory;
    private readonly Queue<string> _allNotificationsQueue = new();
    private readonly List<NotificationSingleUI> _allCurrentDisplayingNotifications = new();

    private CancellationTokenSource _notificationsQueueCancellationToken = new();

    #endregion

    #region Initialization

    [Inject]
    public void Construct(NotificationsSingleUIFactory notificationsFactory)
    {
        _notificationsFactory = notificationsFactory;
    }

    #endregion

    #region Add

    public void AddNewNotification(string notificationText)
    {
        _allNotificationsQueue.Enqueue(notificationText);

        _notificationsQueueCancellationToken.Cancel();
        _notificationsQueueCancellationToken = new();
        StartNotificationsQueue(_notificationsQueueCancellationToken.Token).Forget();
    }

    private async UniTaskVoid StartNotificationsQueue(CancellationToken cancellationToken)
    {
        while (_allNotificationsQueue.Count > 0)
        {
            await UniTask.WaitUntil(() => _allCurrentDisplayingNotifications.Count < _maxNotificationCount, cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            while (_allCurrentDisplayingNotifications.Count < _maxNotificationCount && _allNotificationsQueue.Count > 0)
            {
                string newNotificationText = _allNotificationsQueue.Dequeue();
                NotificationSingleUI newCreatedNotification = _notificationsFactory.Create(newNotificationText);
                newCreatedNotification.NotificationDestroyed += NewCreatedNotification_OnNotificationDestroyed;

                _allCurrentDisplayingNotifications.Add(newCreatedNotification);
            }
        }
    }

    private void NewCreatedNotification_OnNotificationDestroyed(NotificationSingleUI destroyedNotification)
    {
        _allCurrentDisplayingNotifications.Remove(destroyedNotification);
    }

    #endregion

    public void Dispose()
    {
        _notificationsQueueCancellationToken.Cancel();
    }
}