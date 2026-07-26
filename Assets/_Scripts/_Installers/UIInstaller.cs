#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

#endregion

public class UIInstaller : MonoInstaller
{
    [SerializeField] private OperatorsPinsPageUI _operatorsPinsPageUI;
    [SerializeField] private NavigationPanelUI _navigationPanelUI;
    [SerializeField] private WaitingConnectionUI _waitingConnectionUI;
    [SerializeField] private RegistrationPageUI _registrationPageUI;
    [SerializeField] private AmmoPageUI _ammoPageUI;
    [SerializeField] private DateTimePageUI _dateTimePageUI;
    [SerializeField] private EventsListPageUI _eventsListPageUI;
    [SerializeField] private ChooseOperatingModePageUI _chooseOperatingModePageUI;
    [SerializeField] private MeteoConditionsPageUI _meteoConditionsPageUI;
    [SerializeField] private AlarmsPageUI _alarmsPageUI;
    [SerializeField] private MainPageUI _mainPageUI;
    [SerializeField] private SettingsPageUI _settingsPageUI;
    [SerializeField] private AppSettingsUI _appSettingsUI;
    [SerializeField] private MainFiringMachinesInterfaceUI _mainFiringMachinesInterfaceUI;
    [SerializeField] private List<FiringMachinesPageUI> _allFiringMachinePagesUI;
    [SerializeField] private List<DutyModeTab> _allDutyModeTabs;
    [SerializeField] private ExitPageUI _exitPageUI;

    [SerializeField] private ProjectDebugUI _projectDebugUI;
    [SerializeField] private ConnectMultiplayerUI _connectMultiplayerUI;
    [SerializeField] private ClientConnectionUI _clientConnectionUI;
    [SerializeField] private CameraSystemUI _cameraSystemUI;

    [SerializeField] private WaveSettingSingleUI _waveSettingSingleUI;
    [SerializeField] private List<WaveTabInfo> _allWaveTabs;
    [SerializeField] private MultipleCloseWavesUI _multipleCloseWavesUI;
    [SerializeField] private OperationTimelineUI _operationTimelineUI;
    [SerializeField] private OperationSetupUI _operationSetupUI;
    [SerializeField] private OperationSetupControlUI _operationSetupControlsUI;
    [SerializeField] private AllOperationPresetsListUI _allOperationPresetsListUI;
    [SerializeField] private SelectedOperationPresetUI _selectedOperationPresetUI;
    [SerializeField] private OperationMapManagerUI _operationMapManagerUI;
    [SerializeField] private OperationMapZonesManagerUI _mapZonesManagerUI;
    [SerializeField] private OperationMapPathManagerUI _mapPathManagerUI;
    [SerializeField] private OperationMapMarkersManagerUI _mapMarkersManagerUI;
    [SerializeField] private ActiveOperationInfoUI _activeOperationInfoUI;
    [SerializeField] private OperationWeatherSettingsUI _weatherSettingsUI;

    [SerializeField] private AlarmSingleUI _alarmSinglePrefab;
    [SerializeField] private Transform _allAlarmsContainer;
    [SerializeField] private CustomEventUI _customEventPrefab;
    [SerializeField] private Transform _customEventsContainer;
    [SerializeField] private FiringMachineSingleUI _firingMachineSinglePrefab;
    [SerializeField] private Transform _firingMachinesSingleUIContainer;
    [SerializeField] private DebugLogSingleUI _logSinglePrefab;
    [SerializeField] private Transform _logSinglesContainer;
    [SerializeField] private SavedOperationSingleUI _savedOperationSingleUI;
    [SerializeField] private Transform _savedOperationSingleContainer;
    [SerializeField] private CameraSystemSingleUI _cameraSystemSingleUI;
    [SerializeField] private Transform _cameraSystemSingleContainer;
    [SerializeField] private PathPointSingleUI _pathPointSinglePrefab;
    [SerializeField] private Transform _pathPointSingleContainer;
    [SerializeField] private CloseWaveSingleUI _closeWaveSinglePrefab;
    [SerializeField] private Transform _closeWaveSingleContainer;
    [SerializeField] private SingleWaveMainInfoUI _singleWaveMainInfoPrefab;
    [SerializeField] private Transform _singleWaveMainInfoContainer;
    [SerializeField] private MapPointDirectionLineUI _directionalLineUI;
    [SerializeField] private Transform _directionalLineContainer;
    [SerializeField] private List<MapMarkerPrefab> _allMapMarkerPrefabs;
    [SerializeField] private Transform _allMapMarkerContainer;
    [SerializeField] private List<PathPointPrefab> _allPathPointPrefabs;
    [SerializeField] private Transform _allPathPointsContainer;
    [SerializeField] private TimelinePointUI _timelinePointPrefab;
    [SerializeField] private Transform _timelinePointsContainer;

    [SerializeField] private FullscreenNotificationUI _fullscreenNotificationUI;
    [SerializeField] private InputFieldNotificationUI _inputFieldNotificationUI;
    [SerializeField] private TemporaryNotificationsManagerUI _temporaryNotificationsManagerUI;
    [SerializeField] private NotificationSingleUI _notificationSinglePrefab;
    [SerializeField] private Transform _notificationSingleContainer;

    public override void InstallBindings()
    {
        InitializeFactories();
        InitializeUIManagers();

        InitializeUIPages();
        InitializeObservers();
    }

    private void InitializeObservers()
    {
        Container.BindInterfacesAndSelfTo<ProjectDebugObserver>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<DutyModePageObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ChooseOperationModePageObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<PreLoginPagesObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<SettingsPageObserver>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<AlarmsPageObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<AmmoPageObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<DateTimePageObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<CustomEventsPageObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ExitPageObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<MeteoConditionsPageObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<OperatorPinsPageObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ClientConnectionObserver>().AsSingle().NonLazy();

        Container.BindInterfacesAndSelfTo<OperationSetupUIObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<WaveSettingSingleObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<MultipleCloseWavesUIObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<OperationTimelineUIObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<OperationPresetsUIObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<OperationSetupControlsObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<ActiveOperationUIObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<OperationMapManagerUIObserver>().AsSingle().NonLazy();
        Container.BindInterfacesAndSelfTo<OperationWeatherSettingsObserver>().AsSingle().NonLazy();
    }

    private void InitializeFactories()
    {
        Container.BindFactory<CustomEvent, CustomEventUI, CustomEventUIFactory>()
            .FromComponentInNewPrefab(_customEventPrefab).UnderTransform(_customEventsContainer).AsSingle();

        Container.BindFactory<AlarmSingle, AlarmSingleUI, AlarmsSingleUIFactory>()
            .FromSubContainerResolve().ByNewContextPrefab<AlarmSingleUIInstaller>(_alarmSinglePrefab).UnderTransform(_allAlarmsContainer).AsSingle();

        Container.BindFactory<IFiringMachineDataProvider, FiringMachineSingleUI, FiringMachineUIFactory>()
            .FromComponentInNewPrefab(_firingMachineSinglePrefab).UnderTransform(_firingMachinesSingleUIContainer).AsSingle();

        Container.BindFactory<DebugLogSingle, DebugLogSingleUI, DebugLogsUIFactory>()
            .FromComponentInNewPrefab(_logSinglePrefab).UnderTransform(_logSinglesContainer).AsSingle();

        Container.BindFactory<SavedOperationData, SavedOperationSingleUI, SavedOperationUIFactory>()
            .FromComponentInNewPrefab(_savedOperationSingleUI).UnderTransform(_savedOperationSingleContainer).AsSingle();

        Container.BindFactory<CameraSystemSingle, CameraSystemSingleUI, CameraSystemSingleUIFactory>()
            .FromSubContainerResolve().ByNewContextPrefab<CameraSystemSingleUIInstaller>(_cameraSystemSingleUI)
            .UnderTransform(_cameraSystemSingleContainer).AsSingle();

        Container.BindFactory<CameraSystemSingleUI, CameraSystemSingle, CameraSystemSingleObserver, CameraSystemSingleUIObserverFactory>().AsSingle();
        Container.BindInterfacesAndSelfTo<CameraSystemUISpawner>().AsSingle();

        Container.BindFactory<string, NotificationSingleUI, NotificationsSingleUIFactory>()
            .FromSubContainerResolve().ByNewContextPrefab<NotificationSingleInstaller>(_notificationSinglePrefab)
            .UnderTransform(_notificationSingleContainer).AsSingle();

        Container.BindFactory<bool, ObjectLimits, int, PathPointSingleUI, PathPointSingleUIFactory>()
            .FromSubContainerResolve().ByNewContextPrefab<PathPointSingleInstaller>(_pathPointSinglePrefab).UnderTransform(_pathPointSingleContainer)
            .AsSingle();

        Container.BindFactory<OperationWave, CloseWaveSingleUI, CloseWavesUIFactory>()
            .FromSubContainerResolve().ByNewContextPrefab<SingleWaveMainInfoUIInstaller>(_closeWaveSinglePrefab)
            .UnderTransform(_closeWaveSingleContainer).AsSingle();

        Container.BindFactory<OperationWave, SingleWaveMainInfoUI, WaveMainInfoUIFactory>()
            .FromSubContainerResolve().ByNewContextPrefab<SingleWaveMainInfoUIInstaller>(_singleWaveMainInfoPrefab)
            .UnderTransform(_singleWaveMainInfoContainer).AsSingle();

        Container.BindFactory<string, MapPointDirectionLineUI, DirectionLinesUIFactory>()
            .FromSubContainerResolve().ByNewContextPrefab<DirectionalLineUIInstaller>(_directionalLineUI).UnderTransform(_directionalLineContainer)
            .AsSingle();

        Container.BindFactory<IReadOnlyList<OperationWave>, TimelinePointUI, TimelinePointsUIFactory>()
            .FromComponentInNewPrefab(_timelinePointPrefab).UnderTransform(_timelinePointsContainer).AsSingle();

        Container.Bind<List<MapMarkerPrefab>>().FromInstance(_allMapMarkerPrefabs);
        Container.Bind<Transform>().WithId("MarkersContainer").FromInstance(_allMapMarkerContainer);
        Container.Bind<List<PathPointPrefab>>().FromInstance(_allPathPointPrefabs);
        Container.Bind<Transform>().WithId("PathPointsContainer").FromInstance(_allPathPointsContainer);

        Container.BindInterfacesAndSelfTo<MarkersFactory>().AsSingle();
        Container.BindInterfacesAndSelfTo<PathPointsFactory>().AsSingle();
    }

    private void InitializeUIManagers()
    {
        Container.BindInterfacesAndSelfTo<DebugManager>().AsSingle();

        Container.BindInterfacesAndSelfTo<UIManager>().AsSingle();
        Container.BindInterfacesAndSelfTo<TemporaryNotificationsManagerUI>().FromInstance(_temporaryNotificationsManagerUI).AsSingle();
        Container.BindInterfacesAndSelfTo<FullscreenNotificationUI>().FromInstance(_fullscreenNotificationUI).AsSingle();
        Container.BindInterfacesAndSelfTo<InputFieldNotificationUI>().FromInstance(_inputFieldNotificationUI).AsSingle();
        Container.BindInterfacesAndSelfTo<SelectedUIItemController>().AsSingle();
    }

    private void InitializeUIPages()
    {
        Container.BindInterfacesAndSelfTo<ConnectMultiplayerUI>().FromInstance(_connectMultiplayerUI).AsSingle();
        Container.BindInterfacesAndSelfTo<ProjectDebugUI>().FromInstance(_projectDebugUI).AsSingle();

        Container.BindInterfacesAndSelfTo<OperatorsPinsPageUI>().FromInstance(_operatorsPinsPageUI).AsSingle();
        Container.BindInterfacesAndSelfTo<NavigationPanelUI>().FromInstance(_navigationPanelUI).AsSingle();
        Container.BindInterfacesAndSelfTo<WaitingConnectionUI>().FromInstance(_waitingConnectionUI).AsSingle();
        Container.BindInterfacesAndSelfTo<RegistrationPageUI>().FromInstance(_registrationPageUI).AsSingle();
        Container.BindInterfacesAndSelfTo<AmmoPageUI>().FromInstance(_ammoPageUI).AsSingle();
        Container.BindInterfacesAndSelfTo<DateTimePageUI>().FromInstance(_dateTimePageUI).AsSingle();
        Container.BindInterfacesAndSelfTo<EventsListPageUI>().FromInstance(_eventsListPageUI).AsSingle();

        Container.BindInterfacesAndSelfTo<ChooseOperatingModePageUI>().FromInstance(_chooseOperatingModePageUI)
            .AsSingle();

        Container.BindInterfacesAndSelfTo<MeteoConditionsPageUI>().FromInstance(_meteoConditionsPageUI).AsSingle();
        Container.BindInterfacesAndSelfTo<AlarmsPageUI>().FromInstance(_alarmsPageUI).AsSingle();
        Container.BindInterfacesAndSelfTo<MainPageUI>().FromInstance(_mainPageUI).AsSingle();
        Container.BindInterfacesAndSelfTo<SettingsPageUI>().FromInstance(_settingsPageUI).AsSingle();
        Container.BindInterfacesAndSelfTo<OperationSetupUI>().FromInstance(_operationSetupUI).AsSingle();
        Container.BindInterfacesAndSelfTo<AppSettingsUI>().FromInstance(_appSettingsUI).AsSingle();

        Container.BindInterfacesAndSelfTo<MainFiringMachinesInterfaceUI>().FromInstance(_mainFiringMachinesInterfaceUI)
            .AsSingle();

        foreach (FiringMachinesPageUI firingMachinesPageUI in _allFiringMachinePagesUI)
        {
            Container.BindInterfacesAndSelfTo<FiringMachinesPageUI>().FromInstance(firingMachinesPageUI);
        }

        foreach (DutyModeTab dutyModeTab in _allDutyModeTabs)
        {
            Container.BindInterfacesAndSelfTo<DutyModeTab>().FromInstance(dutyModeTab);
        }

        Container.BindInterfacesAndSelfTo<ExitPageUI>().FromInstance(_exitPageUI).AsSingle();
        Container.BindInterfacesAndSelfTo<ClientConnectionUI>().FromInstance(_clientConnectionUI).AsSingle();
        Container.BindInterfacesAndSelfTo<CameraSystemUI>().FromInstance(_cameraSystemUI).AsSingle();

        Container.BindInterfacesAndSelfTo<WaveSettingSingleUI>().FromInstance(_waveSettingSingleUI).AsSingle();

        foreach (WaveTabInfo waveTabInfo in _allWaveTabs)
        {
            List<Type> waveTabBindTypes = GetBindTypes(waveTabInfo, typeof(WaveTabInfo));
            Container.Bind(waveTabBindTypes).FromInstance(waveTabInfo);
        }

        Container.BindInterfacesAndSelfTo<MultipleCloseWavesUI>().FromInstance(_multipleCloseWavesUI).AsSingle();
        Container.BindInterfacesAndSelfTo<OperationTimelineUI>().FromInstance(_operationTimelineUI).AsSingle();
        Container.BindInterfacesAndSelfTo<OperationSetupControlUI>().FromInstance(_operationSetupControlsUI).AsSingle();
        Container.BindInterfacesAndSelfTo<AllOperationPresetsListUI>().FromInstance(_allOperationPresetsListUI).AsSingle();
        Container.BindInterfacesAndSelfTo<SelectedOperationPresetUI>().FromInstance(_selectedOperationPresetUI).AsSingle();
        Container.BindInterfacesAndSelfTo<OperationMapManagerUI>().FromInstance(_operationMapManagerUI).AsSingle();
        Container.BindInterfacesAndSelfTo<OperationMapZonesManagerUI>().FromInstance(_mapZonesManagerUI).AsSingle();
        Container.BindInterfacesAndSelfTo<OperationMapPathManagerUI>().FromInstance(_mapPathManagerUI).AsSingle();
        Container.BindInterfacesAndSelfTo<OperationMapMarkersManagerUI>().FromInstance(_mapMarkersManagerUI).AsSingle();
        Container.BindInterfacesAndSelfTo<ActiveOperationInfoUI>().FromInstance(_activeOperationInfoUI).AsSingle();
        Container.BindInterfacesAndSelfTo<OperationWeatherSettingsUI>().FromInstance(_weatherSettingsUI).AsSingle();
    }

    private List<Type> GetBindTypes(Object bindingObject, Type baseObjectType)
    {
        Type objectType = bindingObject.GetType();
        List<Type> objectBindTypes = new List<Type>() { baseObjectType, objectType };
        objectBindTypes.AddRange(objectType.GetInterfaces());

        return objectBindTypes;
    }
}