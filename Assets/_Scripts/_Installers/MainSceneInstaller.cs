#region

using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class MainSceneInstaller : MonoInstaller
{
    [Header("Factory Prefabs")] [SerializeField]
    private FiringMachineInstaller _firingMachinePrefab;

    [SerializeField] private FiringMachineAmmoTypesUI _firingMachineAmmoTypesPrefab;

    [SerializeField] private List<EnemyPrefabInfo> _allEnemiesPrefabInfo;
    [SerializeField] private EnemyBaseStatsSO _enemyBaseStatsSO;
    [SerializeField] private FiringMachinesSpawner _firingMachinesSpawner;

    [Header("Managers")] [SerializeField] private CameraViewManager _cameraViewManager;
    [SerializeField] private AllFiringMachinesManager _allFiringMachinesManager;
    [SerializeField] private OperationsManager _operationsManager;
    [SerializeField] private OperationPresetsManager _operationPresetsManager;
    [SerializeField] private OperationTerritoryManager _operationTerritoryManager;
    [SerializeField] private OperationUpdateManager _operationUpdateManager;
    [SerializeField] private SceneWeatherManager _sceneWeatherManager;
    [SerializeField] private WeatherEffectsManager _weatherEffectsManager;
    [SerializeField] private LightningVFXSpawner _additionalWeatherVFX;
    [SerializeField] private MarkersManager _markersManager;
    [SerializeField] private WaitingForConnectionManager _waitingForConnectionManager;
    [SerializeField] private TCPSceneResetManager _sceneResetManager;

    [SerializeField] private List<CameraSystemSingle> _allCameraSystems;

    public override void InstallBindings()
    {
        InitializeTCP();

        InitializeInputHandlers();
        InitializeGlobalDataProviders();
        InitializeFactories();

        InitializeFiringMachines();
        InitializeOperationManagers();

        InitializeManagers();

        InitializeSceneVisuals();
        InitializeObservers();
    }

    private void InitializeTCP()
    {
        Container.BindInterfacesAndSelfTo<TCPFiringMachineInput>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<TCPSystemInput>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<TCPUIInput>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<TCPTaskHandler>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<TCPGameStateCommunicator>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<TCPSceneResetManager>().FromInstance(_sceneResetManager).AsSingle().NonLazy();
    }

    private void InitializeInputHandlers()
    {
        Container.BindInterfacesAndSelfTo<DevInputHandler>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<FiringMachineInputHandler>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<SystemInputHandler>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<UIInputHandler>().AsSingle().NonLazy();
    }

    private void InitializeGlobalDataProviders()
    {
        Container.BindInterfacesAndSelfTo<DevInterfaceManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<WaitingForConnectionManager>().FromInstance(_waitingForConnectionManager).AsSingle();

        Container.BindInterfacesAndSelfTo<DYMNetworkManager>().AsSingle();
        Container.Bind<EnemyBaseStatsSO>().FromInstance(_enemyBaseStatsSO);
    }

    private void InitializeFactories()
    {
        Container.BindFactory<Transform, int, FiringMachineController, FiringMachinesFactory>()
            .FromSubContainerResolve().ByNewContextPrefab<FiringMachineInstaller>(_firingMachinePrefab).AsSingle();

        Container.BindFactory<IFiringMachineDataProvider, FiringMachineAmmoTypesUI, FiringMachineAmmoTypesUIFactory>()
            .FromComponentInNewPrefab(_firingMachineAmmoTypesPrefab).AsSingle();

        Container.Bind<List<EnemyPrefabInfo>>().FromInstance(_allEnemiesPrefabInfo);
        Container.Bind<EnemiesFactory>().AsSingle();
    }

    private void InitializeFiringMachines()
    {
        Container.BindInterfacesAndSelfTo<FiringMachinesSpawner>().FromInstance(_firingMachinesSpawner).AsSingle();
        Container.BindInterfacesAndSelfTo<AllFiringMachinesManager>().FromInstance(_allFiringMachinesManager).AsSingle();
    }

    private void InitializeOperationManagers()
    {
        Container.BindInterfacesAndSelfTo<OperationsManager>().FromInstance(_operationsManager).AsSingle();
        Container.BindInterfacesAndSelfTo<CurrentEditingOperationManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<OperationPresetsManager>().FromInstance(_operationPresetsManager).AsSingle();
        Container.BindInterfacesAndSelfTo<OperationTerritoryManager>().FromInstance(_operationTerritoryManager).AsSingle();
        Container.BindInterfacesAndSelfTo<OperationScoreManager>().AsSingle();

        Container.BindInterfacesAndSelfTo<OperationUpdateManager>().FromInstance(_operationUpdateManager).AsSingle();

        Container.BindInterfacesAndSelfTo<MarkersManager>().FromInstance(_markersManager).AsSingle();

        foreach (CameraSystemSingle cameraSystemSingle in _allCameraSystems)
        {
            Container.BindInterfacesAndSelfTo<CameraSystemSingle>().FromInstance(cameraSystemSingle);
        }
    }

    private void InitializeManagers()
    {
        Container.BindInterfacesAndSelfTo<CustomEventsManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<CurrentDateTimeManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<AlarmsManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<PreSettingsManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<OperatorsLoginManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<MeteoConditionsManager>().AsSingle();
    }

    private void InitializeSceneVisuals()
    {
        Container.BindInterfacesAndSelfTo<CameraViewManager>().FromInstance(_cameraViewManager).AsSingle();
        Container.BindInterfacesAndSelfTo<SceneWeatherManager>().FromInstance(_sceneWeatherManager).AsSingle();
        Container.BindInterfacesAndSelfTo<WeatherEffectsManager>().FromInstance(_weatherEffectsManager).AsSingle();
        Container.BindInterfacesAndSelfTo<LightningVFXSpawner>().FromInstance(_additionalWeatherVFX).AsSingle();
    }

    private void InitializeObservers()
    {
        Container.BindInterfacesAndSelfTo<OperationsManagerObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<FiringMachineManagerObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<AppSettingsObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<DateTimeManagerObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<AlarmManagerCustomEventObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<OperatorsLoginObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<MeteoConditionsObserver>().AsSingle().NonLazy();
    }
}