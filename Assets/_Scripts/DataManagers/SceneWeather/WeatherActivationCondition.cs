#region

using System;
using Newtonsoft.Json;
using Unity.Netcode;
using UnityEngine;

#endregion

[Serializable]
public class ReadonlyWeatherActivationConditions : INetworkCustomSerializable
{
    public ReadonlyWeatherActivationConditions()
    {
        _season = WeatherActivationSeason.Summer;
        _time = WeatherActivationTime.Day;
        _weather = WeatherActivationCondition.Normal;
    }

    [JsonConstructor]
    public ReadonlyWeatherActivationConditions(WeatherActivationSeason season,
        WeatherActivationTime time, WeatherActivationCondition weather)
    {
        _season = season;
        _time = time;
        _weather = weather;
    }

    public WeatherActivationSeason Season => _season;
    [SerializeField] private WeatherActivationSeason _season;

    public WeatherActivationTime Time => _time;
    [SerializeField] private WeatherActivationTime _time;

    public WeatherActivationCondition Weather => _weather;
    [SerializeField] private WeatherActivationCondition _weather;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref _season);
        serializer.SerializeValue(ref _time);
        serializer.SerializeValue(ref _weather);
    }

    public void PackForNetworkTransfer()
    {
    }

    public void UnpackAfterNetworkTransfer()
    {
    }
}