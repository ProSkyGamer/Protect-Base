#region

using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Zenject;

#endregion

public class SceneWeatherManager : NetworkBehaviour, ISceneResettable
{
    #region Variables & References

    [SerializeField] private List<SceneWeatherParams> _allSceneWeatherConditions;
    [SerializeField] private ReadonlyWeatherActivationConditions _defaultWeatherConditions;
    private SceneWeatherParams _weatherParams;
    [SerializeField] private List<SceneWeatherTerrain> _allSeasonsTerrains;
    [SerializeField] private Light _globalLight;

    private readonly int _tint = Shader.PropertyToID("_Tint");
    private WeatherEffectsManager _weatherEffectsManager;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(WeatherEffectsManager weatherEffectsManager)
    {
        _weatherEffectsManager = weatherEffectsManager;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer == false)
            return;

        NetworkManager.Singleton.OnConnectionEvent += NetworkManager_OnConnectionEvent;

        ChangeWeather(_defaultWeatherConditions);
    }

    private void NetworkManager_OnConnectionEvent(NetworkManager networkManager,
        ConnectionEventData connectionEventData)
    {
        if (IsServer == false)
            return;

        if (connectionEventData.EventType is not ConnectionEvent.ClientConnected)
            return;

        ReadonlyWeatherActivationConditions initializingWeather =
            _weatherParams == null ? _defaultWeatherConditions : _weatherParams.ActivationConditions;

        ChangeWeather(initializingWeather);
    }

    #endregion

    #region Change Time

    public void ChangeWeather(ReadonlyWeatherActivationConditions weatherActivationCondition)
    {
        if (IsServer == false)
            return;

        ChangeSceneWeatherClientRpc(weatherActivationCondition.Season, weatherActivationCondition.Time,
            weatherActivationCondition.Weather);
    }

    [ClientRpc]
    private void ChangeSceneWeatherClientRpc(WeatherActivationSeason weatherActivationSeason,
        WeatherActivationTime weatherActivationTime,
        WeatherActivationCondition weatherActivationCondition)
    {
        if (ClientTypeManager.CurrentClientType is not (ClientType.Game
            or ClientType.CameraSystem))
            return;

        ChangeCurrentWeather(weatherActivationSeason, weatherActivationTime, weatherActivationCondition);
    }

    private void ChangeCurrentWeather(WeatherActivationSeason weatherActivationSeason,
        WeatherActivationTime weatherActivationTime,
        WeatherActivationCondition weatherActivationCondition)
    {
        if (ClientTypeManager.CurrentClientType is not (ClientType.Game
            or ClientType.CameraSystem))
            return;

        bool isChangingTerrain = _weatherParams == null ||
                                 _weatherParams.ActivationConditions.Season !=
                                 weatherActivationSeason;

        if (isChangingTerrain)
        {
            foreach (SceneWeatherTerrain seasonTerrain in _allSeasonsTerrains)
            {
                seasonTerrain.TerrainTransform.gameObject.SetActive(false);
            }

            SceneWeatherTerrain changingTerrain = _allSeasonsTerrains.Find(seasonTerrain =>
                seasonTerrain.WeatherActivationSeason == weatherActivationSeason);

            changingTerrain.TerrainTransform.gameObject.SetActive(true);
        }

        SceneWeatherParams changingWeatherParams = _allSceneWeatherConditions.FirstOrDefault(weatherParams =>
            weatherParams.ActivationConditions.Season == weatherActivationSeason &&
            weatherParams.ActivationConditions.Time == weatherActivationTime &&
            weatherParams.ActivationConditions.Weather == weatherActivationCondition);

        if (changingWeatherParams == null)
            return;

        _weatherParams = changingWeatherParams;

        RenderSettings.skybox = _weatherParams.SkyboxMaterial;
        RenderSettings.skybox.SetColor(_tint, _weatherParams.SkyboxTintColor);
        RenderSettings.ambientSkyColor = _weatherParams.SkyboxSkyColor;
        RenderSettings.ambientEquatorColor = _weatherParams.SkyboxEquatorColor;
        RenderSettings.ambientGroundColor = _weatherParams.SkyboxGroundColor;
        RenderSettings.fogColor = _weatherParams.FogColor;
        RenderSettings.fogDensity = _weatherParams.FogDensityValue;
        _globalLight.color = _weatherParams.DirectionalLightingColor;

        _weatherEffectsManager.ChangeWeatherEffects(_weatherParams);
    }

    #endregion

    #region Get

    public ReadonlyWeatherActivationConditions GetNextTimeSettings()
    {
        int currentWeatherConditionsIndex = _allSceneWeatherConditions.IndexOf(_weatherParams);
        int nextWeatherConditionsIndex = currentWeatherConditionsIndex + 1;

        nextWeatherConditionsIndex = nextWeatherConditionsIndex >= _allSceneWeatherConditions.Count
            ? 0
            : nextWeatherConditionsIndex;

        SceneWeatherParams nextWeatherConditions = _allSceneWeatherConditions[nextWeatherConditionsIndex];

        return nextWeatherConditions.ActivationConditions;
    }

    public List<ReadonlyWeatherActivationConditions> GetAllWeatherActivationCondition()
    {
        List<ReadonlyWeatherActivationConditions> allWeatherCondition = _allSceneWeatherConditions
            .Select(weatherConditions => weatherConditions.ActivationConditions).ToList();

        return allWeatherCondition;
    }

    #endregion

    public void OnSceneReset()
    {
        if (IsServer == false)
            return;

        ChangeWeather(_defaultWeatherConditions);
    }
}