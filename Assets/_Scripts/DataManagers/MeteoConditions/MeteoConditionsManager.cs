#region

using System;
using UnityEngine;

#endregion

public class MeteoConditions
{
    public readonly int TemperatureValue;
    public readonly int PressureValue;

    public MeteoConditions(int temperatureValue, int pressureValue)
    {
        TemperatureValue = temperatureValue;
        PressureValue = pressureValue;
    }
}

public class MeteoConditionsManager : IMeteoConditionsProvider
{
    #region Events

    public event Action MeteoConditionsChanged;

    #endregion

    #region Variables & References

    private readonly MeteoConditions _baseMeteoConditions = new MeteoConditions(15, 741);

    private readonly IDataSavingManager _dataSavingManager;

    private const string IsFirstTimeSetPlayerPrefs = "IsFirstTimeSetPlayerPrefs";

    #endregion

    #region Properties

    public int TemperatureValue => _dataSavingManager.GetSavedMeteoConditions().TemperatureValue;

    public int PressureValue => _dataSavingManager.GetSavedMeteoConditions().PressureValue;

    public int MinPressureValue { get; } = 450;

    public int MaxPressureValue { get; } = 820;

    public int MinTemperatureValue { get; } = -40;

    public int MaxTemperatureValue { get; } = 50;

    #endregion

    #region Initialization

    public MeteoConditionsManager(IDataSavingManager dataSavingManager)
    {
        _dataSavingManager = dataSavingManager;

        if (PlayerPrefs.GetInt(IsFirstTimeSetPlayerPrefs, 0) == 0)
        {
            PlayerPrefs.SetInt(IsFirstTimeSetPlayerPrefs, 1);

            _dataSavingManager.SaveMeteoConditions(_baseMeteoConditions);
        }
    }

    #endregion

    #region Setting Values

    public void SetCurrentValues(int newTemperatureValue, int newPressureValue)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        if (newTemperatureValue > MaxTemperatureValue || newTemperatureValue < MinPressureValue || newPressureValue < MinPressureValue ||
            newPressureValue > MaxPressureValue)
            return;

        MeteoConditions newMeteoConditions = new MeteoConditions(newTemperatureValue, newPressureValue);

        _dataSavingManager.SaveMeteoConditions(newMeteoConditions);

        Debug.Log($"Changed temperature to {newTemperatureValue} and " +
                  $"pressure to {newPressureValue}");

        MeteoConditionsChanged?.Invoke();
    }

    #endregion
}