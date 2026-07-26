#region

using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

#endregion

public class DutyModePageObserver : IInitializable, IDisposable
{
    #region Events

    public event Action<FiringMachinesPageType> DutyModeActivated;

    public event Action DutyModeDeactivated;

    #endregion

    #region Variables & References

    private readonly Dictionary<FiringMachinesPageType, FiringMachinesPageUI> _allFiringMachinesPages;
    private FiringMachinesPageType _currentShowingPageType;

    private IFiringMachineDataProvider _currentDataProvider;
    private readonly ICameraStatsProvider _cameraStatsProvider;
    private readonly IAlarmsTriggerer _alarmsTriggerer;
    private readonly IAllFiringMachineInfoProvider _allFiringMachineInfoProvider;
    private readonly IPoVSwapper _poVSwapper;

    #endregion

    #region Initialization

    public DutyModePageObserver(List<FiringMachinesPageUI> allFiringMachinesPages, ICameraStatsProvider cameraStatsProvider,
        IAlarmsTriggerer alarmsTriggerer,
        IAllFiringMachineInfoProvider allFiringMachineInfoProvider, IPoVSwapper poVSwapper)
    {
        _allFiringMachinesPages = allFiringMachinesPages.ToDictionary(firingMachinesPage => firingMachinesPage.PageType);

        _cameraStatsProvider = cameraStatsProvider;
        _alarmsTriggerer = alarmsTriggerer;
        _allFiringMachineInfoProvider = allFiringMachineInfoProvider;
        _poVSwapper = poVSwapper;
    }

    public void Initialize()
    {
        foreach (KeyValuePair<FiringMachinesPageType, FiringMachinesPageUI> firingMachinesPage in _allFiringMachinesPages)
        {
            firingMachinesPage.Value.InterfaceShown += FiringMachinesPageUIOnInterfaceShown;
            firingMachinesPage.Value.InterfaceHidden += FiringMachinesPageUIOnInterfaceHidden;
        }

        _cameraStatsProvider.OnCameraAnglesChanged += CameraViewManager_OnCameraAnglesChanged;

        _allFiringMachineInfoProvider.ChangedFiringMachine += FiringMachineInfoProviderOnCurrentFiringMachineChanged;

        _poVSwapper.ChangeInfraredState += FiringMachineManager_OnInfraredModeToggle;

        _alarmsTriggerer.NewAlarmsTriggered += AlarmsManagerTriggeredAlarmsListChanged;
        _alarmsTriggerer.TriggeredAlarmsRemoved += AlarmsManagerTriggeredAlarmsListChanged;
    }

    private void FiringMachineManager_OnInfraredModeToggle(bool isInfraredEnabled)
    {
        _allFiringMachinesPages[_currentShowingPageType].UpdateCrosshair(isInfraredEnabled);
    }

    private void AlarmsManagerTriggeredAlarmsListChanged(IReadOnlyList<AlarmSingle> changedAlarms)
    {
        IReadOnlyList<AlarmSingle> allAlarms = _alarmsTriggerer.GetCurrentActiveAlarms();
        _allFiringMachinesPages[_currentShowingPageType].UpdateAlarms(allAlarms);
    }

    private void FiringMachineInfoProviderOnCurrentFiringMachineChanged(IFiringMachineDataProvider newDataProvider)
    {
        if (_currentDataProvider != null)
        {
            _currentDataProvider.PovStatusChanged -= CurrentFiringMachine_OnFiringMachineModeChanged;
            _currentDataProvider.AmmoCountChanged -= CurrentAmmoCountChanged;
        }

        _currentDataProvider = newDataProvider;

        if (_currentDataProvider != null)
        {
            _currentDataProvider.PovStatusChanged += CurrentFiringMachine_OnFiringMachineModeChanged;
            _currentDataProvider.AmmoCountChanged += CurrentAmmoCountChanged;
        }

        _allFiringMachinesPages[_currentShowingPageType].UpdateView();
        _allFiringMachinesPages[_currentShowingPageType].UpdateTab(DutyModeTabType.Ammo);
    }

    private void CurrentAmmoCountChanged()
    {
        _allFiringMachinesPages[_currentShowingPageType].UpdateTab(DutyModeTabType.Ammo);
    }

    private void CurrentFiringMachine_OnFiringMachineModeChanged()
    {
        _allFiringMachinesPages[_currentShowingPageType].UpdateView();
        _allFiringMachinesPages[_currentShowingPageType].UpdateTab(DutyModeTabType.Angles);
        _allFiringMachinesPages[_currentShowingPageType].UpdateTab(DutyModeTabType.Ammo);
    }

    private void CameraViewManager_OnCameraAnglesChanged()
    {
        _allFiringMachinesPages[_currentShowingPageType].UpdateTab(DutyModeTabType.Angles);
    }

    private void FiringMachinesPageUIOnInterfaceShown(FiringMachinesPageType shownPageType)
    {
        _allFiringMachinesPages[shownPageType].UpdateVisuals();

        IReadOnlyList<AlarmSingle> allAlarms = _alarmsTriggerer.GetCurrentActiveAlarms();
        _allFiringMachinesPages[shownPageType].UpdateAlarms(allAlarms);

        _currentShowingPageType = shownPageType;

        DutyModeActivated?.Invoke(shownPageType);
    }

    private void FiringMachinesPageUIOnInterfaceHidden()
    {
        DutyModeDeactivated?.Invoke();
    }

    #endregion

    public void Dispose()
    {
        foreach (KeyValuePair<FiringMachinesPageType, FiringMachinesPageUI> firingMachinesPage in _allFiringMachinesPages)
        {
            firingMachinesPage.Value.InterfaceShown -= FiringMachinesPageUIOnInterfaceShown;
            firingMachinesPage.Value.InterfaceHidden -= FiringMachinesPageUIOnInterfaceHidden;
        }

        if (_currentDataProvider != null)
        {
            _currentDataProvider.PovStatusChanged -= CurrentFiringMachine_OnFiringMachineModeChanged;
            _currentDataProvider.AmmoCountChanged -= CurrentAmmoCountChanged;
        }

        _cameraStatsProvider.OnCameraAnglesChanged -= CameraViewManager_OnCameraAnglesChanged;

        _allFiringMachineInfoProvider.ChangedFiringMachine -= FiringMachineInfoProviderOnCurrentFiringMachineChanged;

        _poVSwapper.ChangeInfraredState -= FiringMachineManager_OnInfraredModeToggle;

        _alarmsTriggerer.NewAlarmsTriggered -= AlarmsManagerTriggeredAlarmsListChanged;
        _alarmsTriggerer.TriggeredAlarmsRemoved -= AlarmsManagerTriggeredAlarmsListChanged;
    }
}