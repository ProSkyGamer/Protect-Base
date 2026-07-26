#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class ChooseOperatingModePageUI : BasePageUI, IInitializable, IDisposable
{
    #region Events

    public event Action MeteoConditionsButtonPressed;

    public event Action DutyModeButtonPressed;

    public event Action EventsListButtonPressed;

    public event Action SettingsButtonPressed;

    #endregion

    #region Variables & References

    [SerializeField] private TextMeshProUGUI _operatorIndexText;
    [SerializeField] private TextMeshProUGUI _dateText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _temperatureText;
    [SerializeField] private TextMeshProUGUI _pressureText;
    [SerializeField] private Button _enterMeteorologicalConditionsButton;
    [SerializeField] private Button _dutyModeButton;
    [SerializeField] private Button _eventsListButton;
    [SerializeField] private Button _settingsButton;
    [SerializeField] private ButtonSelectItemSingleUI _quitSelectableButton;
    private Button _quitButton;

    [SerializeField] private string _turnOffFiringMachinesNotificationText;
    private readonly float _updatingTimeInterval = 1f;
    private readonly CancellationTokenSource _updatingTimeCancellationTokenSource = new();
    private ICurrentDateTimeProvider _dateTimeProvider;
    private IMeteoConditionsProvider _meteoConditionsProvider;
    private OperatorsLoginManager _operatorsLoginManager;
    private IAllFiringMachineInfoProvider _allFiringMachineInfoProvider;
    private StringFormatsSO _stringFormatsSO;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ICurrentDateTimeProvider dateTimeProvider, IMeteoConditionsProvider meteoConditionsProvider,
        OperatorsLoginManager operatorsLoginManager, IAllFiringMachineInfoProvider allFiringMachineInfoProvider,
        StringFormatsSO stringFormatsSO)
    {
        _dateTimeProvider = dateTimeProvider;
        _meteoConditionsProvider = meteoConditionsProvider;
        _operatorsLoginManager = operatorsLoginManager;
        _allFiringMachineInfoProvider = allFiringMachineInfoProvider;
        _stringFormatsSO = stringFormatsSO;
    }

    public void Initialize()
    {
        _quitButton = _quitSelectableButton.GetButtonComponent();

        SubscribeToUIEvents();

        UpdateTimerTextAsync(_updatingTimeCancellationTokenSource.Token).Forget();
    }

    private void SubscribeToUIEvents()
    {
        _enterMeteorologicalConditionsButton.onClick.AddListener(OnEnterMeteorologicalConditionsButtonPressed);
        _dutyModeButton.onClick.AddListener(OnDutyModeButtonPressed);
        _eventsListButton.onClick.AddListener(OnEventsListButtonPressed);
        _settingsButton.onClick.AddListener(OnSettingsButtonPressed);
        _quitButton.onClick.AddListener(OnQuitButtonPressed);
    }

    private void OnEnterMeteorologicalConditionsButtonPressed()
    {
        MeteoConditionsButtonPressed?.Invoke();
    }

    private void OnDutyModeButtonPressed()
    {
        DutyModeButtonPressed?.Invoke();
    }

    private void OnEventsListButtonPressed()
    {
        EventsListButtonPressed?.Invoke();
    }

    private void OnSettingsButtonPressed()
    {
        SettingsButtonPressed?.Invoke();
    }

    private void OnQuitButtonPressed()
    {
        if (_allFiringMachineInfoProvider.IsAnyEnabled)
            _quitSelectableButton.DisplayNotification(_turnOffFiringMachinesNotificationText).Forget();
        else
            RequestHide();
    }

    private async UniTaskVoid UpdateTimerTextAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _timeText.text = _dateTimeProvider.GetTimeFormattedString(_dateTimeProvider.CurrentDateTime);
            _dateText.text = _dateTimeProvider.GetDateWithWeekDayFormattedString(_dateTimeProvider.CurrentDateTime);

            await UniTask.WaitForSeconds(_updatingTimeInterval, cancellationToken: cancellationToken);
        }
    }

    #endregion

    #region Visual

    public void UpdateVisual()
    {
        _operatorIndexText.text = _operatorsLoginManager.LoginedUser.CurrentLoginedUserIndex.ToString();

        string temperatureStringFormat = _stringFormatsSO.TemperatureFormatString;

        string temperatureString =
            string.Format(temperatureStringFormat, _meteoConditionsProvider.TemperatureValue);

        _temperatureText.text = temperatureString;

        string pressureStringFormat = _stringFormatsSO.PressureFormatString;
        string pressureString = string.Format(pressureStringFormat, _meteoConditionsProvider.PressureValue);
        _pressureText.text = pressureString;
    }

    #endregion

    public void Dispose()
    {
        _updatingTimeCancellationTokenSource?.Cancel();
    }
}