public interface IMeteoConditionsProvider
{
    public int TemperatureValue { get; }

    public int PressureValue { get; }

    public int MinPressureValue { get; }

    public int MaxPressureValue { get; }

    public int MinTemperatureValue { get; }

    public int MaxTemperatureValue { get; }
}