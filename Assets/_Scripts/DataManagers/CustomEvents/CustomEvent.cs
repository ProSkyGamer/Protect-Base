#region

using System;

#endregion

public class CustomEvent
{
    public DateTime EventTime { get; }
    public string EventName { get; }
    public string EventOperator { get; }

    public CustomEvent(DateTime eventTime, string eventName, string eventOperator)
    {
        EventTime = eventTime;
        EventName = eventName;
        EventOperator = eventOperator;
    }
}