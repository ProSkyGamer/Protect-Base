#region

using Unity.Netcode.Transports.UTP;
using UnityEngine;
using Zenject;

#endregion

public class GameInstaller : MonoInstaller
{
    [SerializeField] private UnityTransport unityTransport;

    private GameInputsAM _gameInputsAm;

    public override void InstallBindings()
    {
        Container.BindInterfacesAndSelfTo<JSONDataSavingManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<AppSettingsManager>().AsSingle().NonLazy();

        Container.Bind<ClientTypeManager>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<TCPServerConnector>().AsSingle().NonLazy();
        Container.Bind<UnityTransport>().FromInstance(unityTransport);

        Container.BindInterfacesAndSelfTo<CursorManager>().AsSingle();

        Container.BindInterfacesAndSelfTo<DynamicInjector>().AsSingle();

        InitializeInput();
    }

    private void InitializeInput()
    {
        _gameInputsAm = new GameInputsAM();
        Container.Bind<GameInputsAM>().FromInstance(_gameInputsAm).AsSingle();

        Container.BindInterfacesAndSelfTo<KeyboardDevInput>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<KeyboardFiringMachineInput>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<KeyboardSystemInput>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<KeyboardUIInput>().AsSingle().NonLazy();

        _gameInputsAm.Enable();
    }
}