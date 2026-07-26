#region

using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class MeteoConditionsPageUI : BasePageUI, IInitializable
{
    #region Events

    public event Action<int, int> ValuesChanged;

    #endregion

    #region Variables & References

    [SerializeField] private NumberInputFieldFilterUI _temperatureInputFieldFilter;
    [SerializeField] private NumberInputFieldFilterUI _pressureInputFieldFilter;
    [SerializeField] private Button _quitButton;

    private IMeteoConditionsProvider _meteoConditionsProvider;

    public override bool IsCanHide => true;

    #endregion

    #region Initializaton

    [Inject]
    public void Construct(IMeteoConditionsProvider meteoConditionsProvider)
    {
        _meteoConditionsProvider = meteoConditionsProvider;
    }

    public void Initialize()
    {
        InitializeInputFieldLimits();

        SubscribeToUIEvents();
    }

    private void InitializeInputFieldLimits()
    {
        _pressureInputFieldFilter.SetMinValue(_meteoConditionsProvider.MinPressureValue);
        _pressureInputFieldFilter.SetMaxValue(_meteoConditionsProvider.MaxPressureValue);

        _temperatureInputFieldFilter.SetMinValue(_meteoConditionsProvider.MinTemperatureValue);
        _temperatureInputFieldFilter.SetMaxValue(_meteoConditionsProvider.MaxTemperatureValue);
    }

    private void SubscribeToUIEvents()
    {
        _quitButton.onClick.AddListener(OnQuitButtonPressed);
    }

    private void OnQuitButtonPressed()
    {
        int temperatureValue = _temperatureInputFieldFilter.GetIntValue();
        int pressureValue = _pressureInputFieldFilter.GetIntValue();

        ValuesChanged?.Invoke(temperatureValue, pressureValue);

        RequestHide();
    }

    #endregion

    #region Visual

    public override void Show()
    {
        base.Show();

        UpdateVisuals();
    }

    public override void UpdateVisuals()
    {
        _temperatureInputFieldFilter.SetAndFilterText(_meteoConditionsProvider.TemperatureValue.ToString());
        _pressureInputFieldFilter.SetAndFilterText(_meteoConditionsProvider.PressureValue.ToString());
    }

    #endregion
}