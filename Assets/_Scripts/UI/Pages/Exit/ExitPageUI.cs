#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class ExitPageUI : BasePageUI, IInitializable, IDisposable
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _dateText;
    [SerializeField] private TextMeshProUGUI _timeText;
    [SerializeField] private TextMeshProUGUI _temperatureText;
    [SerializeField] private TextMeshProUGUI _pressureText;
    [SerializeField] private TextMeshProUGUI _operatorNumberText;
    [SerializeField] private TextMeshProUGUI _registrationTimeText;
    [SerializeField] private TextMeshProUGUI _alarmsCountText;
    [SerializeField] private TextMeshProUGUI _totalEventsCount;
    [SerializeField] private TMP_InputField _pinInputField;
    [SerializeField] private Button _continueWorkButton;

    [SerializeField] private ButtonSelectItemSingleUI _confirmSelectableButton;
    private Button _confirmButton;
    [SerializeField] private string _correctPinNotificationText;
    [SerializeField] private string _incorrectPinNotificationText;

    private readonly float _updatingTimeInterval = 1f;
    private readonly CancellationTokenSource _updatingTimeCancellationTokenSource = new();

    private OperatorsLoginManager _loginManager;
    private ICurrentDateTimeProvider _currentDateTimeProvider;
    private IMeteoConditionsProvider _meteoConditionsProvider;
    private StringFormatsSO _stringFormatsSO;

    public override bool IsCanHide => true;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(OperatorsLoginManager operatorsLoginManager, ICurrentDateTimeProvider dateTimeProvider,
        IMeteoConditionsProvider meteoConditionsProvider, StringFormatsSO stringFormatsSO)
    {
        _loginManager = operatorsLoginManager;
        _currentDateTimeProvider = dateTimeProvider;
        _meteoConditionsProvider = meteoConditionsProvider;
        _stringFormatsSO = stringFormatsSO;

        _confirmButton = _confirmSelectableButton.GetButtonComponent();
    }

    public void Initialize()
    {
        SubscribeToUIEvents();

        UpdateTimerTextAsync(_updatingTimeCancellationTokenSource.Token).Forget();
    }

    private void SubscribeToUIEvents()
    {
        _continueWorkButton.onClick.AddListener(OnContinueButtonPressed);
        _confirmButton.onClick.AddListener(() => { OnConfirmButtonPressed().Forget(); });
    }

    private void OnContinueButtonPressed()
    {
        RequestHide();
    }

    private async UniTaskVoid OnConfirmButtonPressed()
    {
        string pin = _pinInputField.text;
        bool isLoggedOutSuccessfully = _loginManager.WouldLoggedOutSuccessful(pin);

        await _confirmSelectableButton.DisplayNotification(isLoggedOutSuccessfully ? _correctPinNotificationText : _incorrectPinNotificationText);

        _pinInputField.text = "";

        _loginManager.LogOut(pin);
    }

    #endregion

    #region Update

    private async UniTaskVoid UpdateTimerTextAsync(CancellationToken cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _timeText.text =
                _currentDateTimeProvider.GetTimeFormattedString(_currentDateTimeProvider.CurrentDateTime);

            _dateText.text =
                _currentDateTimeProvider.GetDateWithWeekDayFormattedString(_currentDateTimeProvider.CurrentDateTime);

            await UniTask.WaitForSeconds(_updatingTimeInterval, cancellationToken: cancellationToken);
        }
    }

    #endregion

    #region Visual

    public void UpdateVisual()
    {
        DateTime date = _currentDateTimeProvider.CurrentDateTime;

        _dateText.text = _currentDateTimeProvider.GetDateWithWeekDayFormattedString(date);

        _timeText.text = _currentDateTimeProvider.GetTimeFormattedString(date);

        string temperatureStringFormat = _stringFormatsSO.TemperatureFormatString;

        string temperatureString =
            string.Format(temperatureStringFormat, _meteoConditionsProvider.TemperatureValue);

        _temperatureText.text = temperatureString;

        string pressureStringFormat = _stringFormatsSO.PressureFormatString;
        string pressureString = string.Format(pressureStringFormat, _meteoConditionsProvider.PressureValue);
        _pressureText.text = pressureString;

        ReadonlyLoginedUser currentLoginedUser = _loginManager.LoginedUser;
        _operatorNumberText.text = currentLoginedUser.CurrentLoginedUserIndex.ToString();
        DateTime registrationTime = currentLoginedUser.LoginTime;
        string hour = registrationTime.Hour > 9 ? $"{registrationTime.Hour}" : $"0{registrationTime.Hour}";
        string minute = registrationTime.Minute > 9 ? $"{registrationTime.Minute}" : $"0{registrationTime.Minute}";
        string second = registrationTime.Second > 9 ? $"{registrationTime.Second}" : $"0{registrationTime.Second}";

        string registrationTimeString = string.Format(_stringFormatsSO.HoursFormatString, hour,
            minute, second);

        _registrationTimeText.text = registrationTimeString;

        _alarmsCountText.text = currentLoginedUser.AlarmsCount.ToString();
        _totalEventsCount.text = currentLoginedUser.EventsCounts.ToString();
    }

    #endregion

    public void Dispose()
    {
        _updatingTimeCancellationTokenSource.Cancel();
    }
}