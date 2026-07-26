#region

using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class EventsListPageUI : BasePageUI
{
    #region Variables & References

    [SerializeField] private int _maxDisplayingEventsCount;
    private int _bottomEventIndex;
    private int _topEventIndex;

    private CustomEventUI _selectedEvent;
    private int _selectedEventIndex;
    private readonly List<CustomEventUI> _currentDisplayingEvents = new();
    private List<CustomEvent> _allCustomEvents;
    private bool _isChangingToFirst;

    private ICustomEventsProvider _customEventsProvider;
    private CustomEventUIFactory _customEventUIFactory;
    public override bool IsCanHide => true;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ICustomEventsProvider customEventsProvider, CustomEventUIFactory customEventUIFactory)
    {
        _customEventsProvider = customEventsProvider;
        _customEventUIFactory = customEventUIFactory;
    }

    #endregion

    #region Visual

    public void UpdateCurrentEventsList()
    {
        if (IsShown == false)
            return;

        _allCustomEvents = _customEventsProvider.GetAllEventsList();

        _bottomEventIndex = _allCustomEvents.Count - 1;
        _selectedEventIndex = _bottomEventIndex;

        UpdateEventsPage();

        _selectedEvent =
            _currentDisplayingEvents[_selectedEventIndex - _topEventIndex];

        SelectedUIItemController.SelectItem(_selectedEvent);
    }

    private void UpdateEventsPage()
    {
        if (IsShown == false)
            return;

        ClearCurrentEventsList();

        _topEventIndex = _bottomEventIndex - _maxDisplayingEventsCount + 1;

        _topEventIndex = Mathf.Clamp(_topEventIndex, 0, _allCustomEvents.Count);

        Debug.Log(_topEventIndex);
        Debug.Log(_bottomEventIndex);

        if (_allCustomEvents.Count == 0)
            return;

        for (int i = _topEventIndex; i <= _bottomEventIndex; i++)
        {
            CustomEvent currentEvent = _allCustomEvents[i];
            CustomEventUI newEvent = _customEventUIFactory.Create(currentEvent);

            if (_currentDisplayingEvents.Count != 0)
            {
                newEvent.SetUpSelectedItem(_currentDisplayingEvents[^1]);
                _currentDisplayingEvents[^1].SetDownSelectedItem(newEvent);
            }

            if (_bottomEventIndex != _allCustomEvents.Count - 1)
                newEvent.SetLeftSelectedItem(newEvent);

            if (_topEventIndex != 0)
                newEvent.SetRightSelectedItem(newEvent);

            _currentDisplayingEvents.Add(newEvent);

            newEvent.UpEventTriggered += NewEventSingleUpEventTriggered;
            newEvent.DownEventTriggered += NewEventSingleDownEventTriggered;
            newEvent.LeftEventTriggered += NewEventSingleLeftEventTriggered;
            // СЛЕДУЮЩАЯ страница (индексы увеличиваются)
            newEvent.RightEventTriggered += NewEventSingleRightEventTriggered;
            // ПРЕДЫДУЩАЯ страница (индексы уменьшаются)
        }

        if (_bottomEventIndex != _allCustomEvents.Count - 1)
            _currentDisplayingEvents[^1].SetDownSelectedItem(_currentDisplayingEvents[^1]);

        if (_topEventIndex != 0)
            _currentDisplayingEvents[0].SetUpSelectedItem(_currentDisplayingEvents[0]);
    }

    private void ClearCurrentEventsList()
    {
        if (IsShown == false)
            return;

        foreach (CustomEventUI displayingEvent in _currentDisplayingEvents)
        {
            displayingEvent.UpEventTriggered -= NewEventSingleUpEventTriggered;
            displayingEvent.DownEventTriggered -= NewEventSingleDownEventTriggered;
            displayingEvent.RightEventTriggered -= NewEventSingleRightEventTriggered;
            displayingEvent.LeftEventTriggered -= NewEventSingleLeftEventTriggered;

            Destroy(displayingEvent.gameObject);
        }

        _currentDisplayingEvents.Clear();
    }

    private void NewEventSingleLeftEventTriggered()
    {
        if (IsShown == false)
            return;

        _bottomEventIndex += _maxDisplayingEventsCount;

        _bottomEventIndex = _bottomEventIndex >= _allCustomEvents.Count
            ? _allCustomEvents.Count - 1
            : _bottomEventIndex;

        _currentDisplayingEvents[_selectedEventIndex - _topEventIndex]
            .StopInteraction();

        UpdateEventsPage();

        _selectedEventIndex += _maxDisplayingEventsCount;

        _selectedEventIndex = _selectedEventIndex >= _allCustomEvents.Count
            ? _allCustomEvents.Count - 1
            : _selectedEventIndex;

        _selectedEvent =
            _currentDisplayingEvents[_selectedEventIndex - _topEventIndex];

        SelectedUIItemController.SelectItem(_selectedEvent);
    }

    private void NewEventSingleDownEventTriggered()
    {
        if (IsShown == false)
            return;

        if (_selectedEventIndex == _bottomEventIndex &&
            _selectedEventIndex != _allCustomEvents.Count - 1)
        {
            _bottomEventIndex += _maxDisplayingEventsCount;

            _bottomEventIndex = _bottomEventIndex >= _allCustomEvents.Count
                ? _allCustomEvents.Count - 1
                : _bottomEventIndex;

            _currentDisplayingEvents[_selectedEventIndex - _topEventIndex]
                .StopInteraction();

            UpdateEventsPage();

            _selectedEventIndex = _topEventIndex;

            _selectedEvent =
                _currentDisplayingEvents[0];

            SelectedUIItemController.SelectItem(_selectedEvent);
        } // К СЛЕДУЮЩИМ ИВЕНТАМ
        else
        {
            if (_isChangingToFirst)
            {
                _currentDisplayingEvents[_selectedEventIndex - _topEventIndex]
                    .StopInteraction();

                _selectedEventIndex = _bottomEventIndex;
                _selectedEvent = _currentDisplayingEvents[^1];
                SelectedUIItemController.SelectItem(_selectedEvent);
            }
            else
            {
                _selectedEventIndex++;
            }
        }

        _isChangingToFirst = false;
    }

    private void NewEventSingleRightEventTriggered()
    {
        if (IsShown == false)
            return;

        _bottomEventIndex -= _maxDisplayingEventsCount;

        _bottomEventIndex = _bottomEventIndex < 0
            ? 0
            : _bottomEventIndex;

        _currentDisplayingEvents[_selectedEventIndex - _topEventIndex]
            .StopInteraction();

        UpdateEventsPage();

        _selectedEventIndex -= _maxDisplayingEventsCount;

        _selectedEventIndex = _selectedEventIndex < 0
            ? 0
            : _selectedEventIndex;

        _selectedEvent =
            _currentDisplayingEvents[_selectedEventIndex - _topEventIndex];

        SelectedUIItemController.SelectItem(_selectedEvent);
    }

    private void NewEventSingleUpEventTriggered()
    {
        if (IsShown == false)
            return;

        if (_selectedEventIndex == _topEventIndex &&
            _selectedEventIndex != 0) // К ПРЕДЫДУЩИМ ИВЕНТАМ
        {
            _bottomEventIndex -= _maxDisplayingEventsCount;

            _bottomEventIndex = _bottomEventIndex < 0
                ? 0
                : _bottomEventIndex;

            _currentDisplayingEvents[_selectedEventIndex - _topEventIndex]
                .StopInteraction();

            UpdateEventsPage();

            _selectedEventIndex = _bottomEventIndex;

            _selectedEvent =
                _currentDisplayingEvents[^1];

            SelectedUIItemController.SelectItem(_selectedEvent);
        }
        else
        {
            if (_isChangingToFirst)
            {
                _currentDisplayingEvents[_selectedEventIndex - _topEventIndex]
                    .StopInteraction();

                _selectedEventIndex = _topEventIndex;
                _selectedEvent = _currentDisplayingEvents[0];
                SelectedUIItemController.SelectItem(_selectedEvent);
            }
            else
            {
                _selectedEventIndex--;
            }
        }

        _isChangingToFirst = false;
    }

    #endregion
}