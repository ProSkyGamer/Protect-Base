#region

using System;
using Zenject;

#endregion

public class MeteoConditionsPageObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly MeteoConditionsPageUI _meteoConditionsPageUI;
    private readonly MeteoConditionsManager _meteoConditionsManager;

    #endregion

    #region Initialization

    public MeteoConditionsPageObserver(MeteoConditionsPageUI meteoConditionsPageUI, MeteoConditionsManager meteoConditionsManager)
    {
        _meteoConditionsPageUI = meteoConditionsPageUI;
        _meteoConditionsManager = meteoConditionsManager;
    }

    public void Initialize()
    {
        _meteoConditionsPageUI.PageShown += MeteoConditionsPageUI_OnPageShown;
        _meteoConditionsPageUI.ValuesChanged += MeteoConditionsValuesPageUI_OnValuesChanged;
    }

    private void MeteoConditionsValuesPageUI_OnValuesChanged(int temperatureValue, int pressureValue)
    {
        _meteoConditionsManager.SetCurrentValues(temperatureValue, pressureValue);
    }

    private void MeteoConditionsPageUI_OnPageShown()
    {
        _meteoConditionsPageUI.UpdateVisuals();
    }

    #endregion

    public void Dispose()
    {
        _meteoConditionsPageUI.PageShown -= MeteoConditionsPageUI_OnPageShown;
        _meteoConditionsPageUI.ValuesChanged -= MeteoConditionsValuesPageUI_OnValuesChanged;
    }
}