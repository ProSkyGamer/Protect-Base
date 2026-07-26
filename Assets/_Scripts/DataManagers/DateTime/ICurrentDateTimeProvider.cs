#region

using System;

#endregion

public interface ICurrentDateTimeProvider
{
    public DateTime CurrentDateTime { get; }

    public string GetDateWithWeekDayFormattedString(DateTime date);

    protected internal string GetCurrentWeekDayString(DateTime date);

    public string GetTimeFormattedString(DateTime date);
}