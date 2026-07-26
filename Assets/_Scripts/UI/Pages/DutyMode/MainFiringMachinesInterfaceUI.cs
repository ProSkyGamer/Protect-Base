#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class MainFiringMachinesInterfaceUI : MonoBehaviour, IInitializable, ISceneResettable, IDisposable
{
    #region Variables & References

    private Dictionary<DutyModeTabType, DutyModeTab> _allDutyModeTabs = new();
    [SerializeField] private TextMeshProUGUI _currentModeText;

    [SerializeField] private Transform _fullAlarmTextTransform;
    [SerializeField] private TextMeshProUGUI _currentAlarmNumberText;

    [SerializeField] private Transform _mainCrosshairTransform;
    [SerializeField] private Transform _invertedCrosshairTransform;

    [SerializeField] private Transform _firingMachineDisabledViewTransform;
    private bool _isFiringMachinesInitialized;
    private readonly CancellationTokenSource _initializationCancellationToken = new();

    private IFiringMachineDataProvider _currentDataProvider;
    private IAllFiringMachineInfoProvider _allFiringMachineInfoProvider;
    private FiringMachineUIFactory _firingMachineUIFactory;
    private readonly List<FiringMachineSingleUI> _allCreatedFiringMachinesUI = new();

    #endregion

    #region Initialization

    [Inject]
    public void Construct(IAllFiringMachineInfoProvider allFiringMachineInfoProvider, FiringMachineUIFactory firingMachineUIFactory,
        List<DutyModeTab> allDutyModeTabs)
    {
        _allFiringMachineInfoProvider = allFiringMachineInfoProvider;
        _firingMachineUIFactory = firingMachineUIFactory;

        _allDutyModeTabs = allDutyModeTabs.ToDictionary(dutyModeTab => dutyModeTab.DutyModeTabType);
    }

    public void Initialize()
    {
        _allFiringMachineInfoProvider.ChangedFiringMachine += FiringMachineInfoProviderOnCurrentFiringMachineChanged;

        _invertedCrosshairTransform.gameObject.SetActive(false);
    }

    private void FiringMachineInfoProviderOnCurrentFiringMachineChanged(IFiringMachineDataProvider newDataProvider)
    {
        _currentDataProvider = newDataProvider;
    }

    #endregion

    #region Visual

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateCrosshair(bool isInfraredEnabled)
    {
        _invertedCrosshairTransform.gameObject.SetActive(isInfraredEnabled);
        _mainCrosshairTransform.gameObject.SetActive(!isInfraredEnabled);
    }

    public void ChangeModeText(string modeText)
    {
        _currentModeText.text = modeText;
    }

    public async UniTask UpdateVisual()
    {
        if (_isFiringMachinesInitialized == false)
            await InitializeFiringMachines(_initializationCancellationToken.Token);

        UpdateTab(DutyModeTabType.Static);

        UpdateTab(DutyModeTabType.Angles);
        UpdateTab(DutyModeTabType.Ammo);
    }

    private async UniTask InitializeFiringMachines(CancellationToken cancellationToken)
    {
        _isFiringMachinesInitialized = true;

        foreach (FiringMachineSingleUI firingMachineSingleUI in _allCreatedFiringMachinesUI)
        {
            Destroy(firingMachineSingleUI.gameObject);
        }

        _allCreatedFiringMachinesUI.Clear();

        List<IFiringMachineDataProvider> allDataProviders =
            await _allFiringMachineInfoProvider.GetAllDataProviders();

        if (cancellationToken.IsCancellationRequested)
            return;

        foreach (IFiringMachineDataProvider dataProvider in allDataProviders)
        {
            FiringMachineSingleUI newFiringMachine =
                _firingMachineUIFactory.Create(dataProvider);

            _allCreatedFiringMachinesUI.Add(newFiringMachine);
        }
    }

    public void UpdateView()
    {
        _firingMachineDisabledViewTransform.gameObject.SetActive(_currentDataProvider == null ||
                                                                 _currentDataProvider.CurrentPoVStatus == false);
    }

    public void UpdateTab(DutyModeTabType tabType)
    {
        _allDutyModeTabs[tabType]?.UpdateTabVisual(_currentDataProvider);
    }

    public void UpdateAlarms(IReadOnlyList<AlarmSingle> activeAlarms)
    {
        _fullAlarmTextTransform.gameObject.SetActive(activeAlarms.Count > 0);

        string currentAlarmsText = "";

        foreach (AlarmSingle currentActiveAlarm in activeAlarms)
        {
            currentAlarmsText += $"{currentActiveAlarm.FiringMachineNumber}, ";
        }

        if (currentAlarmsText != "")
            currentAlarmsText = currentAlarmsText.Remove(currentAlarmsText.Length - 2);

        _currentAlarmNumberText.text = currentAlarmsText;
    }

    #endregion

    public void OnSceneReset()
    {
        _currentDataProvider = null;

        _firingMachineDisabledViewTransform.gameObject.SetActive(true);
    }

    public void Dispose()
    {
        _allFiringMachineInfoProvider.ChangedFiringMachine -= FiringMachineInfoProviderOnCurrentFiringMachineChanged;
        _initializationCancellationToken.Cancel();
    }
}