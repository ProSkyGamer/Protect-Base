#region

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using Unity.Netcode;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

#endregion

public class WeatherEffectsManager : NetworkBehaviour, ISceneResettable, IDisposable
{
    [SerializeField] private List<AdditionalWeatherVFX> _allAdditionalWeatherVFX;

    private AdditionalWeatherVFX _currentAdditionalWeatherVFX;
    [SerializeField] private List<LightningSingle> _allLightningVariations;
    [SerializeField] private float _minLightningIntervals = 5f;
    [SerializeField] private float _maxLightingIntervals = 10f;

    private readonly NetworkVariable<bool> _isLightningActivationNetwork = new();

    private CancellationTokenSource _lightningCancellationToken = new();
    [SerializeField] private bool _isHasVFX;

    private LightningVFXSpawner _lightningVFXSpawner;

    private readonly NetworkVariable<int> _currentActiveLightingVariationIndex = new();

    private IPoVSwapper _povSwapper;
    private IPovProvider _currentPoVProvider;

    private readonly int _tint = Shader.PropertyToID("_Tint");
    private SceneWeatherParams _currentSceneWeatherParams;

    [Inject]
    public void Construct(IPoVSwapper poVSwapper, LightningVFXSpawner lightningVFXSpawner)
    {
        _povSwapper = poVSwapper;
        _lightningVFXSpawner = lightningVFXSpawner;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        _isLightningActivationNetwork.OnValueChanged += IsLightingActivationNetwork_OnValueChanged;

        foreach (AdditionalWeatherVFX additionalWeatherVFX in _allAdditionalWeatherVFX)
        {
            additionalWeatherVFX.VFXTransform.gameObject.SetActive(false);
        }
    }

    private void IsLightingActivationNetwork_OnValueChanged(bool previousValue, bool newValue)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game
            or ClientType.CameraSystem)
            return;

        if (IsServer)
            return;

        LightningSingle showingLightning = _allLightningVariations[_currentActiveLightingVariationIndex.Value];

        ShowLightningAsync(showingLightning, _lightningCancellationToken.Token).Forget();
    }

    private async UniTaskVoid CycleLightningsAsync(CancellationToken cancellationToken)
    {
        if (IsServer == false)
            return;

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            _currentActiveLightingVariationIndex.Value = Random.Range(0, _allLightningVariations.Count);

            float lightningCooldown = Random.Range(_minLightningIntervals, _maxLightingIntervals);

            await CooldownLightningAsync(lightningCooldown, cancellationToken);

            _isLightningActivationNetwork.Value = true;

            LightningSingle showingLightning = _allLightningVariations[_currentActiveLightingVariationIndex.Value];

            await ShowLightningAsync(showingLightning, cancellationToken);

            _isLightningActivationNetwork.Value = false;
        }
    }

    private async UniTask ShowLightningAsync(LightningSingle showingLightning, CancellationToken cancellationToken)
    {
        int lightningStep = 0;

        RenderSettings.fogDensity = showingLightning.FogDensity;

        Color previousFogColor = _currentSceneWeatherParams.FogColor;
        Color previousSkyboxColor = _currentSceneWeatherParams.SkyboxTintColor;

        while (lightningStep <= showingLightning.AllLightningStrikesSingle.Count)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            float currentActiveLightingVariationStepTime =
                showingLightning.AllLightningStrikesSingle[lightningStep].ColorChangingTransition;

            Color nextFogColor = lightningStep >= showingLightning.AllLightningStrikesSingle.Count
                ? _currentSceneWeatherParams.FogColor
                : showingLightning.AllLightningStrikesSingle[lightningStep].LightingColor;

            Color nextSkyboxColor = lightningStep >= showingLightning.AllLightningStrikesSingle.Count
                ? _currentSceneWeatherParams.SkyboxTintColor
                : showingLightning.AllLightningStrikesSingle[lightningStep].SkyboxTintColor;

            Gradient currentFogGradient = GetGradient(previousFogColor, nextFogColor);
            Gradient currentSkyboxTintGradient = GetGradient(previousSkyboxColor, nextSkyboxColor);

            if (IsHasVFX)
            {
                Transform lightningVFXPrefab =
                    showingLightning.AllLightningStrikesSingle[lightningStep].LightningVFXPrefab;

                if (lightningVFXPrefab != null)
                {
                    float lightingVFXLifetime =
                        showingLightning.AllLightningStrikesSingle[lightningStep].LightingVFXLifetime;

                    _lightningVFXSpawner.ShowLightning(lightningVFXPrefab, lightingVFXLifetime);
                }
            }

            // Плавное изменение цвета
            await ShowLightningStepAsync(currentActiveLightingVariationStepTime, currentFogGradient,
                currentSkyboxTintGradient, cancellationToken);

            previousFogColor = nextFogColor;
            previousSkyboxColor = nextSkyboxColor;

            lightningStep += 1;

            // Показ измененного цвета
            if (currentActiveLightingVariationStepTime > 0)
                await UniTask.WaitForSeconds(
                    showingLightning.AllLightningStrikesSingle[lightningStep].ColorShowingDuration,
                    cancellationToken: cancellationToken);
            else
                await UniTask.NextFrame();
        }

        RenderSettings.fogDensity = _currentSceneWeatherParams.FogDensityValue;
    }

    private bool IsHasVFX => ClientTypeManager.CurrentClientType is ClientType.Game && _isHasVFX;

    private async UniTask ShowLightningStepAsync(float stepTime, Gradient fogGradient, Gradient skyboxGradient,
        CancellationToken cancellationToken)
    {
        float stepTimer = stepTime;

        while (stepTimer > 0)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (ClientTypeManager.CurrentClientType is ClientType.Game or ClientType.CameraSystem)
            {
                RenderSettings.fogColor = GetGradientColor(fogGradient, stepTime, stepTimer);
                RenderSettings.skybox.SetColor(_tint, GetGradientColor(skyboxGradient, stepTime, stepTimer));
            }

            stepTimer -= Time.deltaTime;

            await UniTask.NextFrame();
        }
    }

    private Color GetGradientColor(Gradient gradient, float fullTime, float timeLeft)
    {
        return gradient.Evaluate(Mathf.Clamp(1 - timeLeft / fullTime, 0f, 1f));
    }

    private async UniTask CooldownLightningAsync(float cooldownTime, CancellationToken cancellationToken)
    {
        _isLightningActivationNetwork.Value = true;

        await UniTask.WaitForSeconds(cooldownTime, cancellationToken: cancellationToken);
    }

    private void Start()
    {
        if (ClientTypeManager.CurrentClientType is not (ClientType.Game
            or ClientType.CameraSystem))
            return;

        _povSwapper.ChangePoV += PovSwapper_OnChangePoV;
    }

    private void PovSwapper_OnChangePoV(IPovProvider swappedPoV)
    {
        _currentPoVProvider = swappedPoV;

        ChangeAdditionalVFXPositionToCurrentFiringMachine(swappedPoV);
    }

    private void ChangeAdditionalVFXPositionToCurrentFiringMachine(IPovProvider povProvider)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
        {
            foreach (AdditionalWeatherVFX additionalWeatherVFX in _allAdditionalWeatherVFX)
            {
                additionalWeatherVFX.VFXTransform.gameObject.SetActive(false);
            }

            return;
        }

        if (_currentAdditionalWeatherVFX == null)
            return;

        if (povProvider == null)
            return;

        Vector3 newRainVFXPosition =
            povProvider.CurrentPovCameraPosition + _currentAdditionalWeatherVFX.VFXOffset;

        _currentAdditionalWeatherVFX.VFXTransform.position = newRainVFXPosition;
    }

    public void ChangeWeatherEffects(SceneWeatherParams weatherParams)
    {
        _currentSceneWeatherParams = weatherParams;

        _lightningVFXSpawner.ClearCurrentLightnings();

        _lightningCancellationToken.Cancel();
        _lightningCancellationToken = new();

        if (IsServer)
            if (weatherParams.ActivationConditions.Weather is WeatherActivationCondition.Rain)
                CycleLightningsAsync(_lightningCancellationToken.Token).Forget();

        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        _currentAdditionalWeatherVFX?.VFXTransform.gameObject.SetActive(false);

        _currentAdditionalWeatherVFX = null;

        _currentAdditionalWeatherVFX = _allAdditionalWeatherVFX.FirstOrDefault(weatherVFX =>
            weatherVFX.WeatherConditions.Find(weatherConditions =>
                weatherConditions.Season == weatherParams.ActivationConditions.Season &&
                weatherConditions.Time == weatherParams.ActivationConditions.Time &&
                weatherConditions.Weather == weatherParams.ActivationConditions.Weather) != null);

        if (_currentAdditionalWeatherVFX == null)
            return;

        _currentAdditionalWeatherVFX.VFXTransform.gameObject.SetActive(true);
        ChangeAdditionalVFXPositionToCurrentFiringMachine(_currentPoVProvider);
    }

    private Gradient GetGradient(Color originalColor, Color finalColor)
    {
        return new Gradient
        {
            colorKeys = new[]
            {
                new GradientColorKey(originalColor, 0f), new GradientColorKey(finalColor, 1f)
            }
        };
    }

    public void OnSceneReset()
    {
        _lightningCancellationToken.Cancel();
        _lightningCancellationToken = new();
        _lightningVFXSpawner.ClearCurrentLightnings();
    }

    public void Dispose()
    {
        _lightningCancellationToken.Cancel();
        _lightningVFXSpawner.ClearCurrentLightnings();
    }
}