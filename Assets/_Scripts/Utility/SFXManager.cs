#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance { get; private set; }

    #region Enum

    public enum SFXType
    {
        ShootSFX
    }

    #endregion

    #region Created Classes

    [Serializable]
    public class SFXSingle
    {
        public SFXType sfxType;
        public AudioClip sfxAudioClip;
    }

    #endregion

    #region Variables & References

    [SerializeField] private List<SFXSingle> allSFXSingles;
    [SerializeField] private AudioSource audioSourcePrefab;
    private readonly Dictionary<SFXType, AudioSource> allSFXTypesAudioSources = new();

    #endregion

    #region Initialization

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        foreach (SFXSingle sfxSingle in allSFXSingles)
        {
            if (allSFXTypesAudioSources.ContainsKey(sfxSingle.sfxType)) continue;

            AudioSource newSFXSingleAudioSource = Instantiate(audioSourcePrefab, transform);
            newSFXSingleAudioSource.clip = sfxSingle.sfxAudioClip;
            allSFXTypesAudioSources.Add(sfxSingle.sfxType, newSFXSingleAudioSource);
        }
    }

    #endregion

    #region Play SFX

    public void PlaySFX(SFXType sfxType)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game) return;

        if (!allSFXTypesAudioSources.ContainsKey(sfxType)) return;

        allSFXTypesAudioSources[sfxType].Stop();
        allSFXTypesAudioSources[sfxType].Play();
    }

    #endregion
}