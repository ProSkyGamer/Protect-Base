#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

[Serializable]
public class SceneWeatherParams
{
    public ReadonlyWeatherActivationConditions ActivationConditions;
    public Material SkyboxMaterial;
    public Color SkyboxTintColor;
    public Color DirectionalLightingColor;
    public Color SkyboxSkyColor;
    public Color SkyboxEquatorColor;
    public Color SkyboxGroundColor;
    public Color FogColor;
    public float FogDensityValue;
}

[Serializable]
public class SceneWeatherTerrain
{
    public WeatherActivationSeason WeatherActivationSeason;
    public Transform TerrainTransform;
}

[Serializable]
public class AdditionalWeatherVFX
{
    public List<ReadonlyWeatherActivationConditions> WeatherConditions;
    public Transform VFXTransform;
    public Vector3 VFXOffset;
}

[Serializable]
public class LightningSingle
{
    public List<LightingStrikeSingle> AllLightningStrikesSingle;
    public float FogDensity = .025f;
}

[Serializable]
public class LightingStrikeSingle
{
    public Transform LightningVFXPrefab;
    public float LightingVFXLifetime;
    public Color LightingColor;
    public Color SkyboxTintColor;
    public float ColorChangingTransition;
    public float ColorShowingDuration;
}