#region

using UnityEngine;
using Zenject;

#endregion

public class SingleWaveMainInfoUIInstaller : MonoInstaller
{
    [SerializeField] private SingleWaveMainInfoUI _singleWaveMainInfoUI;

    [Inject] private OperationWave _operationWave;

    public override void InstallBindings()
    {
        Container.Bind<OperationWave>().FromInstance(_operationWave);

        Container.BindInterfacesAndSelfTo<SingleWaveMainInfoUI>().FromInstance(_singleWaveMainInfoUI);
        Container.Bind<CloseWaveSingleUI>().FromInstance(_singleWaveMainInfoUI as CloseWaveSingleUI);
    }
}