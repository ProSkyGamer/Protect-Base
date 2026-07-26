#region

using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class StaticDutyModeTab : DutyModeTab
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _currentOperatorText;
    [SerializeField] private TextMeshProUGUI _currentTemperatureText;
    [SerializeField] private TextMeshProUGUI _currentPressureText;

    private ILoginDataProvider _loginDataProvider;
    private StringFormatsSO _stringFormatsSO;
    private IMeteoConditionsProvider _meteoConditionsProvider;

    public override DutyModeTabType DutyModeTabType => DutyModeTabType.Static;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ILoginDataProvider loginDataProvider, StringFormatsSO stringFormatsSO,
        IMeteoConditionsProvider meteoConditionsProvider)
    {
        _loginDataProvider = loginDataProvider;
        _stringFormatsSO = stringFormatsSO;
        _meteoConditionsProvider = meteoConditionsProvider;
    }

    public override void Initialize()
    {
    }

    #endregion

    #region Visuals

    public override void UpdateTabVisual(IFiringMachineDataProvider currentFiringMachineDataProvider)
    {
        UpdateCurrentOperator();
        UpdateCurrentTemperature();
        UpdateCurrentPressure();
    }

    private void UpdateCurrentPressure()
    {
        string pressureStringFormat = _stringFormatsSO.PressureFormatString;
        string pressureString = string.Format(pressureStringFormat, _meteoConditionsProvider.PressureValue);
        _currentPressureText.text = pressureString;
    }

    private void UpdateCurrentTemperature()
    {
        string temperatureStringFormat = _stringFormatsSO.TemperatureFormatString;

        string temperatureString =
            string.Format(temperatureStringFormat, _meteoConditionsProvider.TemperatureValue);

        _currentTemperatureText.text = temperatureString;
    }

    private void UpdateCurrentOperator()
    {
        _currentOperatorText.text = _loginDataProvider.LoginedUser.CurrentLoginedUserIndex.ToString();
    }

    #endregion

    public override void Dispose()
    {
    }
}