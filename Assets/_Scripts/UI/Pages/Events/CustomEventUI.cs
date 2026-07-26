#region

using System;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class CustomEventUI : BaseSelectedItemSingleUI
{
    #region Variables & Referencs

    [SerializeField] private TextMeshProUGUI _eventTimeText;

    [SerializeField] private TextMeshProUGUI _eventNameText;
    [SerializeField] private TextMeshProUGUI _eventOperatorText;

    private bool _isStoppingThis;
    private StringFormatsSO _stringFormatsSO;

    #endregion

    #region Initialize

    [Inject]
    public void Construct(CustomEvent customEvent, StringFormatsSO stringFormatsSO)
    {
        _stringFormatsSO = stringFormatsSO;

        Initialize(customEvent);
    }

    private void Initialize(CustomEvent customEvent)
    {
        Initialize();

        DateTime dateTime = customEvent.EventTime;
        string eventName = customEvent.EventName;
        string eventOperator = customEvent.EventOperator;

        string hour = dateTime.Hour > 9 ? $"{dateTime.Hour}" : $"0{dateTime.Hour}";
        string minute = dateTime.Minute > 9 ? $"{dateTime.Minute}" : $"0{dateTime.Minute}";
        string second = dateTime.Second > 9 ? $"{dateTime.Second}" : $"0{dateTime.Second}";
        string eventTimeString = string.Format(_stringFormatsSO.HoursFormatString, hour, minute, second);
        _eventTimeText.text = eventTimeString;

        _eventNameText.text = eventName;
        _eventOperatorText.text = eventOperator;
    }

    #endregion

    #region Interact

    public override void InteractUp(out bool isStopping)
    {
        base.InteractUp(out isStopping);
        isStopping = _isStoppingThis;

        if (_isStoppingThis)
            _isStoppingThis = false;
    }

    public override void InteractDown(out bool isStopping)
    {
        base.InteractDown(out isStopping);
        isStopping = _isStoppingThis;

        if (_isStoppingThis)
            _isStoppingThis = false;
    }

    public override void InteractLeft(out bool isStopping)
    {
        base.InteractLeft(out isStopping);
        isStopping = _isStoppingThis;

        if (_isStoppingThis)
            _isStoppingThis = false;
    }

    public override void InteractRight(out bool isStopping)
    {
        base.InteractRight(out isStopping);
        isStopping = _isStoppingThis;

        if (_isStoppingThis)
            _isStoppingThis = false;
    }

    public void StopInteraction()
    {
        _isStoppingThis = true;
    }

    #endregion
}