#region

using System;
using Zenject;

#endregion

public class CustomEventsPageObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly EventsListPageUI _eventsListPageUI;
    private readonly ICustomEventsProvider _customEventsProvider;

    #endregion

    #region Initialization

    public CustomEventsPageObserver(EventsListPageUI eventsListPageUI, ICustomEventsProvider customEventsProvider)
    {
        _eventsListPageUI = eventsListPageUI;
        _customEventsProvider = customEventsProvider;
    }

    public void Initialize()
    {
        _customEventsProvider.ListUpdated += _customEventsProvider_OnListUpdated;

        _eventsListPageUI.PageShown += EventsListPageUI_OnPageShown;
    }

    private void EventsListPageUI_OnPageShown()
    {
        _eventsListPageUI.UpdateCurrentEventsList();
    }

    private void _customEventsProvider_OnListUpdated()
    {
        _eventsListPageUI.UpdateCurrentEventsList();
    }

    #endregion

    public void Dispose()
    {
        _customEventsProvider.ListUpdated -= _customEventsProvider_OnListUpdated;

        _eventsListPageUI.PageShown -= EventsListPageUI_OnPageShown;
    }
}