#region

using System;
using UnityEngine;

#endregion

public abstract class SelectableItemsNotificationDisplayer : MonoBehaviour
{
    public event Action NotificationHidden;

    [SerializeField] protected float _displayTime = 2.5f;
    [SerializeField] protected float _hideTime = 1f;

    public abstract void ShowNotification(string notificationText);

    protected void ThrowHiddenNotification()
    {
        NotificationHidden?.Invoke();
    }
}