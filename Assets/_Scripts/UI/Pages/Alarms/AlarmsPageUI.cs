#region

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class AlarmsPageUI : BasePageUI, IInitializable
{
    #region Events

    public event Action AnyInputFieldValueChanged;
    public event Action<int, int, int, int, FiringMachineEnableType, AlarmType> AlarmAdded;
    public event Action RemovedAllAlarms;

    #endregion

    #region Variables & References

    [SerializeField] private int _totalDisplayingSavedAlarms = 5;
    [SerializeField] private NumberInputFieldFilterUI _deviceNumberInputField;
    [SerializeField] private NumberInputFieldFilterUI _alarmNumberInputField;
    [SerializeField] private NumberInputFieldFilterUI _firingMachineNumberInputField;
    [SerializeField] private NumberInputFieldFilterUI _preSettingNumberInputField;
    [SerializeField] private TMP_Dropdown _firingMachineEnableTypeDropdown;
    [SerializeField] private TMP_Dropdown _alarmTypeDropdown;
    [SerializeField] private Button _applyButton;
    [SerializeField] private Button _resetAllButton;
    [SerializeField] private Button _quitButton;

    private IAlarmsDataProvider _alarmsDataProvider;
    private IPreSettingsProvider _preSettingsProvider;

    private EnumTranslationValuesSO _enumTranslationValuesSO;
    private IAllFiringMachineInfoProvider _allFiringMachineInfoProvider;
    private AlarmsSingleUIFactory _alarmsSingleUIFactory;
    private readonly List<AlarmSingleUI> _allCreatedAlarms = new();

    public override bool IsCanHide => true;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(IAlarmsDataProvider alarmsDataProvider, IPreSettingsProvider preSettingsProvider,
        EnumTranslationValuesSO enumTranslationValuesSO,
        IAllFiringMachineInfoProvider allFiringMachineInfoProvider,
        AlarmsSingleUIFactory alarmsSingleUIFactory)
    {
        _alarmsDataProvider = alarmsDataProvider;
        _preSettingsProvider = preSettingsProvider;
        _enumTranslationValuesSO = enumTranslationValuesSO;
        _allFiringMachineInfoProvider = allFiringMachineInfoProvider;
        _alarmsSingleUIFactory = alarmsSingleUIFactory;
    }

    private void OnValidate()
    {
        _totalDisplayingSavedAlarms = _totalDisplayingSavedAlarms % 2 == 0
            ? _totalDisplayingSavedAlarms - 1
            : _totalDisplayingSavedAlarms;
    }

    public void Initialize()
    {
        InitializeButtons();
        InitializeDropdownValues();
        InitializeInputFieldLimits().Forget();

        SubscribeToEvents();
    }

    private void InitializeButtons()
    {
        _applyButton.onClick.AddListener(OnApplyButtonPressed);
        _resetAllButton.onClick.AddListener(OnResetButtonPressed);
        _quitButton.onClick.AddListener(OnQuitButtonPressed);
    }

    private void InitializeDropdownValues()
    {
        string[] allFiringMechanismEnableTypes = Enum.GetNames(typeof(FiringMachineEnableType));
        string[] allAlarmTypes = Enum.GetNames(typeof(AlarmType));

        _firingMachineEnableTypeDropdown.options.Clear();

        for (int i = 0; i < allFiringMechanismEnableTypes.Length; i++)
            _firingMachineEnableTypeDropdown.options.Add(
                new TMP_Dropdown.OptionData(_enumTranslationValuesSO.GetFiringMachineEnableTypeFullText(
                    (FiringMachineEnableType)i)));

        _alarmTypeDropdown.options.Clear();

        for (int i = 0; i < allAlarmTypes.Length; i++)
            _alarmTypeDropdown.options.Add(
                new TMP_Dropdown.OptionData(_enumTranslationValuesSO.GetAlarmTypeFullString((AlarmType)i)));
    }

    private async UniTaskVoid InitializeInputFieldLimits()
    {
        _deviceNumberInputField.SetMinValue(_alarmsDataProvider.MinDeviceNumber);
        _deviceNumberInputField.SetMaxValue(_alarmsDataProvider.MaxDeviceNumber);

        _alarmNumberInputField.SetMinValue(_alarmsDataProvider.MinAlarmNumber);
        _alarmNumberInputField.SetMaxValue(_alarmsDataProvider.MaxAlarmNumber);

        _firingMachineNumberInputField.SetMinValue(_allFiringMachineInfoProvider.GetFiringMachineMinNumber());
        int maxFiringMachineNumber = await _allFiringMachineInfoProvider.GetFiringMachineMaxNumber();
        _firingMachineNumberInputField.SetMaxValue(maxFiringMachineNumber);

        _preSettingNumberInputField.SetMinValue(_preSettingsProvider.PreSettingMinIndex);
        _preSettingNumberInputField.SetMaxValue(_preSettingsProvider.PreSettingMaxIndex);
    }

    private void SubscribeToEvents()
    {
        _deviceNumberInputField.TextChanged += InputField_OnAnyValueChanged;
        _alarmNumberInputField.TextChanged += InputField_OnAnyValueChanged;
        _firingMachineNumberInputField.TextChanged += InputField_OnAnyValueChanged;
    }

    private void OnApplyButtonPressed()
    {
        int deviceNumber = _deviceNumberInputField.GetIntValue();
        int alarmNumber = _alarmNumberInputField.GetIntValue();
        int firingMachineNumber = _firingMachineNumberInputField.GetIntValue();
        int preSettingNumber = _preSettingNumberInputField.GetIntValue();
        FiringMachineEnableType firingMachineEnableType = (FiringMachineEnableType)_firingMachineEnableTypeDropdown.value;
        AlarmType alarmType = (AlarmType)_alarmTypeDropdown.value;

        AlarmAdded?.Invoke(deviceNumber, alarmNumber, firingMachineNumber, preSettingNumber, firingMachineEnableType, alarmType);
    }

    private void OnResetButtonPressed()
    {
        RemovedAllAlarms?.Invoke();
    }

    private void OnQuitButtonPressed()
    {
        RequestHide();
    }

    private void InputField_OnAnyValueChanged(string _)
    {
        AnyInputFieldValueChanged?.Invoke();
    }

    #endregion

    #region Visual

    public void UpdateVisual()
    {
        SelectedUIItemController.DeactivatePseudoSelection();

        ClearCurrentDisplayingAlarms();

        IReadOnlyList<AlarmSingle> allAlarmsSingle = _alarmsDataProvider.GetAllAlarmsSingleByData(
            _deviceNumberInputField.GetIntValue(),
            _firingMachineNumberInputField.GetIntValue());

        List<AlarmSingle> allDisplayingAlarms = allAlarmsSingle.OrderBy(alarmSingle => alarmSingle.FiringMachineNumber).ToList();

        foreach (AlarmSingle displayingAlarm in allDisplayingAlarms)
        {
            AlarmSingleUI alarmSingleUI = _alarmsSingleUIFactory.Create(displayingAlarm);
            _allCreatedAlarms.Add(alarmSingleUI);

            if (displayingAlarm.DeviceNumber != _deviceNumberInputField.GetIntValue() ||
                displayingAlarm.AlarmNumber != _alarmNumberInputField.GetIntValue() ||
                displayingAlarm.FiringMachineNumber != _firingMachineNumberInputField.GetIntValue()) continue;

            SelectedUIItemController.ActivatePseudoSelection(alarmSingleUI);
        }
    }

    private void ClearCurrentDisplayingAlarms()
    {
        foreach (AlarmSingleUI createdAlarm in _allCreatedAlarms)
        {
            Destroy(createdAlarm.gameObject);
        }

        _allCreatedAlarms.Clear();
    }

    #endregion
}