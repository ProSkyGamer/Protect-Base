#region

using System;
using Zenject;

#endregion

public class DateTimePageObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly DateTimePageUI _dateTimePageUI;
    private readonly CurrentDateTimeManager _currentDateTimeManager;

    #endregion

    #region Initialization

    public DateTimePageObserver(DateTimePageUI dateTimePageUI, CurrentDateTimeManager currentDateTimeManager)
    {
        _dateTimePageUI = dateTimePageUI;
        _currentDateTimeManager = currentDateTimeManager;
    }

    public void Initialize()
    {
        _dateTimePageUI.TimeOffsetChanged += DateTimePageUI_OnTimeOffsetChanged;
        _dateTimePageUI.PageShown += DateTimePageUI_OnPageShown;
    }

    private void DateTimePageUI_OnPageShown()
    {
        _dateTimePageUI.UpdateVisual();
    }

    private void DateTimePageUI_OnTimeOffsetChanged(TimeSpan timeOffset)
    {
        _currentDateTimeManager.SetTimeOffset(timeOffset);

        _dateTimePageUI.UpdateVisual();
    }

    #endregion

    public void Dispose()
    {
        _dateTimePageUI.TimeOffsetChanged -= DateTimePageUI_OnTimeOffsetChanged;
        _dateTimePageUI.PageShown -= DateTimePageUI_OnPageShown;
    }
}