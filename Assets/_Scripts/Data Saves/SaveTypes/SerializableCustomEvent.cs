#region

using System;

#endregion

public class SerializableCustomEvent
{
    public DateTime EventTime;
    public string EventName;
    public string EventOperator;

    public SerializableCustomEvent()
    {
    }

    public SerializableCustomEvent(CustomEvent customEvent)
    {
        EventTime = customEvent.EventTime;
        EventName = customEvent.EventName;
        EventOperator = customEvent.EventOperator;
    }

    public CustomEvent GetCustomEvent()
    {
        CustomEvent customEvent = new CustomEvent(EventTime, EventName, EventOperator);

        return customEvent;
    }
}