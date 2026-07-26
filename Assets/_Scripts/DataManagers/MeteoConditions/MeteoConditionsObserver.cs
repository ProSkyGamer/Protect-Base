#region

using System;
using Zenject;

#endregion

public class MeteoConditionsObserver : IInitializable, IDisposable
{
    private readonly MeteoConditionsManager _meteoConditionsManager;
    private readonly CustomEventsManager _customEventsManager;

    public MeteoConditionsObserver(MeteoConditionsManager meteoConditionsManager,
        CustomEventsManager customEventsManager)
    {
        _meteoConditionsManager = meteoConditionsManager;
        _customEventsManager = customEventsManager;
    }

    public void Initialize()
    {
        _meteoConditionsManager.MeteoConditionsChanged += MeteoConditionsManager_OnMeteoConditionsChanged;
    }

    private void MeteoConditionsManager_OnMeteoConditionsChanged()
    {
        _customEventsManager.AddEvent(
            $"Метеоусл.: {_meteoConditionsManager.TemperatureValue}*C {_meteoConditionsManager.PressureValue}мм");
    }

    public void Dispose()
    {
        _meteoConditionsManager.MeteoConditionsChanged -= MeteoConditionsManager_OnMeteoConditionsChanged;
    }
}