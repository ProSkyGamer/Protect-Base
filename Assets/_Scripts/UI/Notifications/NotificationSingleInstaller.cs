#region

using UnityEngine;
using Zenject;

#endregion

public class NotificationSingleInstaller : MonoInstaller
{
    [SerializeField] private NotificationSingleUI _notificationSingleUI;

    [Inject] private string _notificationTextString;

    public override void InstallBindings()
    {
        Container.Bind<string>().FromInstance(_notificationTextString);

        Container.BindInterfacesAndSelfTo<NotificationSingleUI>().FromInstance(_notificationSingleUI);
    }
}