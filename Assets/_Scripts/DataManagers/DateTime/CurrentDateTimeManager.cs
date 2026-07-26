#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

#endregion

public class CurrentDateTimeManager : ICurrentDateTimeProvider, IInitializable, IDisposable
{
    #region Events

    public event Action NewDayReached;

    #endregion

    #region Variables & References

    private readonly string _daysDate = "26.03.2025"; //Любой заданный день 

    private readonly int _daysInt = 2; // День недели в заданную дату (для его отображения)
    // 0 - пн, 6 - вс

    private TimeSpan _currentDateTimeOffset = new(0, 0, 0);

    private const float TimeCheckingPeriod = 1f;
    private DateTime _lastCheckedDay;
    private CancellationTokenSource _cancellationTokenSource = new();
    private StringFormatsSO _stringFormatsSO;
    private IDataSavingManager _dataSavingManager;

    public DateTime CurrentDateTime => DateTime.Now - _currentDateTimeOffset;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(StringFormatsSO stringFormatsSO, IDataSavingManager dataSavingManager)
    {
        _stringFormatsSO = stringFormatsSO;
        _dataSavingManager = dataSavingManager;
    }

    public void Initialize()
    {
        _lastCheckedDay = CurrentDateTime;

        _currentDateTimeOffset = _dataSavingManager.GetSavedDateTimeOffset();

        _cancellationTokenSource = new CancellationTokenSource();
        TickCurrentTime(_cancellationTokenSource.Token).Forget();
    }

    private async UniTaskVoid TickCurrentTime(CancellationToken cancellationToken)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        while (true)
        {
            await UniTask.WaitForSeconds(TimeCheckingPeriod, cancellationToken: cancellationToken);

            if (cancellationToken.IsCancellationRequested)
                return;

            DateTime currentDateTime = CurrentDateTime;

            if (currentDateTime.Date != _lastCheckedDay.Date)
            {
                NewDayReached?.Invoke();

                _lastCheckedDay = currentDateTime.Date;
            }
        }
    }

    #endregion

    #region Set Time Offset

    public void SetTimeOffset(TimeSpan timeOffset)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        Debug.Log($"Changed time offset to {timeOffset}");

        _currentDateTimeOffset = timeOffset;
        _lastCheckedDay = CurrentDateTime;

        _dataSavingManager.SaveCurrentDateTimeOffset(_currentDateTimeOffset);
    }

    #endregion

    #region Get

    public string GetDateWithWeekDayFormattedString(DateTime date)
    {
        string todaysDayOfTheWeekString = ((ICurrentDateTimeProvider)this).GetCurrentWeekDayString(date);

        string day = date.Day > 9 ? $"{date.Day}" : $"0{date.Day}";
        string month = date.Month > 9 ? $"{date.Month}" : $"0{date.Month}";
        string year = date.Year > 9 ? $"{date.Year}" : $"0{date.Year}";
        string dateStringFormat = _stringFormatsSO.CurrentDateFormatString;
        string dateString = string.Format(dateStringFormat, day, month, year, todaysDayOfTheWeekString);

        return dateString;
    }

    string ICurrentDateTimeProvider.GetCurrentWeekDayString(DateTime date)
    {
        string todaysDayOfTheWeekString;
        int todaysDayOfTheWeekInt = (date - DateTime.Parse(_daysDate)).Days % 7;

        todaysDayOfTheWeekInt = todaysDayOfTheWeekInt < 0
            ? todaysDayOfTheWeekInt + 7 + _daysInt
            : todaysDayOfTheWeekInt + _daysInt;

        todaysDayOfTheWeekInt = Mathf.Abs(todaysDayOfTheWeekInt);

        switch (todaysDayOfTheWeekInt)
        {
            default:
                todaysDayOfTheWeekString = "понедельник";

                break;

            case 1:
                todaysDayOfTheWeekString = "вторник";

                break;

            case 2:
                todaysDayOfTheWeekString = "среда";

                break;

            case 3:
                todaysDayOfTheWeekString = "четверг";

                break;

            case 4:
                todaysDayOfTheWeekString = "пятница";

                break;

            case 5:
                todaysDayOfTheWeekString = "суббота";

                break;

            case 6:
                todaysDayOfTheWeekString = "воскресенье";

                break;
        }

        return todaysDayOfTheWeekString;
    }

    public string GetTimeFormattedString(DateTime date)
    {
        string hour = date.Hour > 9 ? $"{date.Hour}" : $"0{date.Hour}";
        string minute = date.Minute > 9 ? $"{date.Minute}" : $"0{date.Minute}";
        string second = date.Second > 9 ? $"{date.Second}" : $"0{date.Second}";

        string timeStringFormat = _stringFormatsSO.HoursFormatString;
        string timeString = string.Format(timeStringFormat, hour, minute, second);

        return timeString;
    }

    #endregion

    public void Dispose()
    {
        _cancellationTokenSource.Cancel();
    }
}