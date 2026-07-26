#region

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class OperationWeatherSettingsUI : MonoBehaviour, IInitializable
{
    #region Events

    public event Action AnyWeatherConditionsChanged;
    public event Action MainWeatherConditionsChanged;

    #endregion

    #region Variables & References

    [SerializeField] private TMP_Dropdown _weatherSeasonDropdown;
    [SerializeField] private TMP_Dropdown _weatherTimeDropdown;
    private readonly Dictionary<int, WeatherActivationTime> _weatherTimeDropdownRealValues = new();
    [SerializeField] private TMP_Dropdown _weatherConditionDropdown;
    private readonly Dictionary<int, WeatherActivationCondition> _weatherConditionsDropdownRealValues = new();

    private EnumTranslationValuesSO _enumTranslationValuesSO;
    private SceneWeatherManager _weatherManager;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO, SceneWeatherManager weatherManager)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;
        _weatherManager = weatherManager;
    }

    public void Initialize()
    {
        InitializeDropdownValues();

        _weatherSeasonDropdown.onValueChanged.AddListener(_ => WeatherSeasonChanged());

        _weatherTimeDropdown.onValueChanged.AddListener(_ => { WeatherTimeChanged(); });

        _weatherConditionDropdown.onValueChanged.AddListener(_ => { WeatherConditionsChanged(); });
    }

    private void InitializeDropdownValues()
    {
        _weatherSeasonDropdown.options.Clear();
        _weatherTimeDropdown.options.Clear();
        _weatherConditionDropdown.options.Clear();

        List<TMP_Dropdown.OptionData> allSeasonsActivations = Enum.GetValues(typeof(WeatherActivationSeason)).Cast<WeatherActivationSeason>()
            .Select(weatherSeason => new TMP_Dropdown.OptionData(_enumTranslationValuesSO.GetWeatherActivationSeasonStringTranslation(weatherSeason)))
            .ToList();

        _weatherSeasonDropdown.AddOptions(allSeasonsActivations);
    }

    private void WeatherConditionsChanged()
    {
        AnyWeatherConditionsChanged?.Invoke();
    }

    private void WeatherTimeChanged()
    {
        AnyWeatherConditionsChanged?.Invoke();
    }

    private void WeatherSeasonChanged()
    {
        MainWeatherConditionsChanged?.Invoke();
        AnyWeatherConditionsChanged?.Invoke();
    }

    #endregion

    #region Visuals

    public void UpdateWeatherActivationDropdowns()
    {
        List<ReadonlyWeatherActivationConditions> allWeatherConditions = _weatherManager.GetAllWeatherActivationCondition();

        List<WeatherActivationTime> addingWeatherActivationTime = allWeatherConditions
            .Where(conditions => conditions.Season == CurrentActivationSeason).Select(conditions => conditions.Time).Distinct().ToList();

        List<WeatherActivationCondition> addingWeatherActivationConditions = allWeatherConditions
            .Where(conditions => conditions.Season == CurrentActivationSeason).Select(conditions => conditions.Weather).Distinct().ToList();

        int previousSceneTimeDropdownValue = _weatherTimeDropdown.value;
        int previousWeatherConditionDropdownValue = _weatherConditionDropdown.value;

        _weatherTimeDropdown.options.Clear();
        _weatherTimeDropdownRealValues.Clear();

        _weatherConditionDropdown.options.Clear();
        _weatherConditionsDropdownRealValues.Clear();

        for (int i = 0; i < addingWeatherActivationTime.Count; i++)
        {
            WeatherActivationTime activationTime = addingWeatherActivationTime[i];

            _weatherTimeDropdown.options.Add(new TMP_Dropdown.OptionData(
                _enumTranslationValuesSO.GetWeatherActivationTimeStringTranslation(activationTime)));

            _weatherTimeDropdownRealValues.Add(i, activationTime);
        }

        for (int i = 0; i < addingWeatherActivationConditions.Count; i++)
        {
            WeatherActivationCondition weatherCondition = addingWeatherActivationConditions[i];

            _weatherConditionDropdown.options.Add(new TMP_Dropdown.OptionData(
                _enumTranslationValuesSO
                    .GetWeatherActivationConditionStringTranslation(weatherCondition)));

            _weatherConditionsDropdownRealValues.Add(i, weatherCondition);
        }

        _weatherTimeDropdown.value = previousSceneTimeDropdownValue;
        _weatherConditionDropdown.value = previousWeatherConditionDropdownValue;
    }

    #endregion

    #region Get

    public WeatherActivationSeason CurrentActivationSeason => (WeatherActivationSeason)_weatherSeasonDropdown.value;

    public WeatherActivationTime CurrentActivationTime => _weatherTimeDropdownRealValues[_weatherTimeDropdown.value];

    public WeatherActivationCondition CurrentWeatherActivation => _weatherConditionsDropdownRealValues[_weatherConditionDropdown.value];

    #endregion
}