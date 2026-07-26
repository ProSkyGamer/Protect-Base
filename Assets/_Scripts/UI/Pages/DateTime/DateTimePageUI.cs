#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class DateTimePageUI : BasePageUI, IInitializable, IDisposable
{
    #region Events

    public event Action<TimeSpan> TimeOffsetChanged;

    #endregion

    #region Variables & References

    [SerializeField] private TextMeshProUGUI _currentDateText;
    [SerializeField] private TextMeshProUGUI _currentTimeText;
    [SerializeField] private NumberInputFieldFilterUI _dayDateInputField;
    [SerializeField] private NumberInputFieldFilterUI _monthDateInputField;
    [SerializeField] private NumberInputFieldFilterUI _yearDateInputField;
    [SerializeField] private NumberInputFieldFilterUI _hoursDateInputField;
    [SerializeField] private NumberInputFieldFilterUI _minutesDateInputField;
    [SerializeField] private NumberInputFieldFilterUI _secondsDateInputField;
    [SerializeField] private ButtonSelectItemSingleUI _applySelectableButton;
    private Button _applyButton;

    [SerializeField] private string _successfullyAppliedNotificationText;
    [SerializeField] private Button _quitButton;

    private DateTime _enteredPageDateTime;
    private readonly float _updatingTimeInterval = 1f;
    private readonly CancellationTokenSource _updateCancellationToken = new();
    private ICurrentDateTimeProvider _dateTimeProvider;

    public override bool IsCanHide => true;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ICurrentDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public void Initialize()
    {
        _applyButton = _applySelectableButton.GetButtonComponent();

        SubscribeToUIEvents();

        UpdateTimerTextAsync(_updateCancellationToken.Token).Forget();
    }

    private void SubscribeToUIEvents()
    {
        _applyButton.onClick.AddListener(OnApplyButtonPressed);
        _quitButton.onClick.AddListener(OnQuitButtonPressed);
    }

    private void OnApplyButtonPressed()
    {
        int yearValue = _yearDateInputField.GetIntValue();
        int monthValue = _monthDateInputField.GetIntValue();
        int dayValue = _dayDateInputField.GetIntValue();
        int hourValue = _hoursDateInputField.GetIntValue();
        int minutesValue = _minutesDateInputField.GetIntValue();
        int secondsValue = _secondsDateInputField.GetIntValue();

        DateTime newSetDate = new(yearValue, monthValue,
            dayValue, hourValue, minutesValue,
            secondsValue);

        TimeSpan newSetTimeOffset = DateTime.Now - newSetDate;

        _enteredPageDateTime = DateTime.Now;

        _applySelectableButton.DisplayNotification(_successfullyAppliedNotificationText).Forget();

        TimeOffsetChanged?.Invoke(newSetTimeOffset);
    }

    private void OnQuitButtonPressed()
    {
        RequestHide();
    }

    #endregion

    #region Update

    private async UniTaskVoid UpdateTimerTextAsync(CancellationToken cancellationToken)
    {
        await UniTaskAsyncEnumerable.Interval(TimeSpan.FromSeconds(_updatingTimeInterval)).TakeUntilCanceled(cancellationToken).ForEachAsync(_ =>
        {
            _currentTimeText.text = _dateTimeProvider.GetTimeFormattedString(_dateTimeProvider.CurrentDateTime);

            _currentDateText.text =
                _dateTimeProvider.GetDateWithWeekDayFormattedString(_dateTimeProvider.CurrentDateTime);
        }, cancellationToken: cancellationToken);
    }

    #endregion

    #region Visual

    public override void Show()
    {
        _enteredPageDateTime = DateTime.Now;

        base.Show();
    }

    public void UpdateVisual()
    {
        _currentDateText.text = _dateTimeProvider.GetDateWithWeekDayFormattedString(_enteredPageDateTime);
        _currentTimeText.text = _dateTimeProvider.GetTimeFormattedString(_enteredPageDateTime);

        DateTime currentSetDate = _dateTimeProvider.CurrentDateTime;

        _dayDateInputField.SetAndFilterText(currentSetDate.Day.ToString());
        _monthDateInputField.SetAndFilterText(currentSetDate.Month.ToString());
        _yearDateInputField.SetAndFilterText(currentSetDate.Year.ToString());
        _hoursDateInputField.SetAndFilterText(_enteredPageDateTime.Hour.ToString());
        _minutesDateInputField.SetAndFilterText(_enteredPageDateTime.Minute.ToString());
        _secondsDateInputField.SetAndFilterText(_enteredPageDateTime.Second.ToString());
    }

    #endregion

    public void Dispose()
    {
        _updateCancellationToken.Cancel();
    }
}