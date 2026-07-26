#region

using UnityEngine;
using Zenject;

#endregion

public class AlarmSingleUIInstaller : MonoInstaller
{
    [SerializeField] private AlarmSingleUI _alarmSingleUI;

    [Inject] private AlarmSingle _alarmSingle;

    public override void InstallBindings()
    {
        Container.Bind<AlarmSingle>().FromInstance(_alarmSingle);

        Container.BindInterfacesAndSelfTo<AlarmSingleUI>().FromInstance(_alarmSingleUI);
    }
}