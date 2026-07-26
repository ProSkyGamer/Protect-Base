#region

using System;
using Zenject;

#endregion

public class OperationWeatherSettingsObserver : IInitializable, IDisposable
{
    private readonly OperationWeatherSettingsUI _operationWeatherSettingsUI;
    private CurrentEditingOperationManager _currentEditingOperationManager;

    public OperationWeatherSettingsObserver(OperationWeatherSettingsUI operationWeatherSettingsUI)
    {
        _operationWeatherSettingsUI = operationWeatherSettingsUI;
    }

    public void Initialize()
    {
        _operationWeatherSettingsUI.AnyWeatherConditionsChanged += OperationWeatherSettingsUI_OnAnyWeatherConditionsChanged;
        _operationWeatherSettingsUI.MainWeatherConditionsChanged += OperationWeatherSettingsUI_OnMainWeatherConditionsChanged;

        _operationWeatherSettingsUI.UpdateWeatherActivationDropdowns();
    }

    private void OperationWeatherSettingsUI_OnMainWeatherConditionsChanged()
    {
        _operationWeatherSettingsUI.UpdateWeatherActivationDropdowns();
    }

    private void OperationWeatherSettingsUI_OnAnyWeatherConditionsChanged()
    {
        ReadonlyWeatherActivationConditions newWeatherSettings = new(_operationWeatherSettingsUI.CurrentActivationSeason,
            _operationWeatherSettingsUI.CurrentActivationTime, _operationWeatherSettingsUI.CurrentWeatherActivation);

        _currentEditingOperationManager.SetWeatherConditions(newWeatherSettings);
    }

    public void Dispose()
    {
        _operationWeatherSettingsUI.AnyWeatherConditionsChanged -= OperationWeatherSettingsUI_OnAnyWeatherConditionsChanged;
        _operationWeatherSettingsUI.MainWeatherConditionsChanged -= OperationWeatherSettingsUI_OnMainWeatherConditionsChanged;
    }
}