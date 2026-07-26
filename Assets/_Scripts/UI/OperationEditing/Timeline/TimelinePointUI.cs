#region

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class TimelinePointUI : MonoBehaviour
{
    #region Events

    public event Action<IReadOnlyList<OperationWave>> Selected;
    public event Action<List<OperationWave>> DeleteRequested;

    #endregion

    #region Variables & References

    [SerializeField] private Transform _selectedSpawnedPoint;
    [SerializeField] private Transform _notSelectedSpawnedPoint;
    [SerializeField] private Transform _selectedNotSpawnedPoint;
    [SerializeField] private Transform _notSelectedNotSpawnedPoint;
    [SerializeField] private TextMeshProUGUI _timelineWaveSpawnTimeText;

    [SerializeField] private TextMeshProUGUI _timelineSimilarSpawnPointCountText;

    [SerializeField] private Button _deleteWaveButton;
    private Button _timelinePointButton;
    private readonly List<OperationWave> _linkedWavesSingle = new();
    private StringFormatsSO _stringFormatsSO;

    public IReadOnlyList<OperationWave> LinkedWaves => _linkedWavesSingle;
    public float WavesSpawnTime { get; private set; }

    #endregion

    #region Initialize

    [Inject]
    public void Construct(List<OperationWave> allTimelineLinkedWavesSingle, StringFormatsSO stringFormatsSO)
    {
        _stringFormatsSO = stringFormatsSO;

        Initialize(allTimelineLinkedWavesSingle);
    }

    private void Initialize(List<OperationWave> allTimelineLinkedWavesSingle)
    {
        if (allTimelineLinkedWavesSingle.Count == 0)
        {
            Destroy(gameObject);

            return;
        }

        _timelinePointButton = GetComponent<Button>();

        float totalWavesSpawnTime = allTimelineLinkedWavesSingle.Sum(t => t.WaveSpawnTime);
        WavesSpawnTime = totalWavesSpawnTime / allTimelineLinkedWavesSingle.Count;

        _linkedWavesSingle.AddRange(allTimelineLinkedWavesSingle);

        _timelinePointButton.onClick.AddListener(() => { Selected?.Invoke(_linkedWavesSingle); });

        _deleteWaveButton.onClick.AddListener(() => { DeleteRequested?.Invoke(_linkedWavesSingle); });

        int waveSpawnMinute = (int)Mathf.Floor(WavesSpawnTime / 60f);
        int waveSpawnSecond = (int)Mathf.Floor(WavesSpawnTime % 60);

        string waveSpawnMinuteString = GetZerosFormattedString(waveSpawnMinute);
        string waveSpawnSecondString = GetZerosFormattedString(waveSpawnSecond);

        string waveSpawnTimeString =
            string.Format(_stringFormatsSO.MinutesTimeFormatString, waveSpawnMinuteString, waveSpawnSecondString);

        _timelineWaveSpawnTimeText.text = waveSpawnTimeString;

        string timelineSimilarSpawnPointCountString = string.Format(_stringFormatsSO.TimelineSimilarSpawnPointFormatString,
            _linkedWavesSingle.Count);

        _timelineSimilarSpawnPointCountText.text = timelineSimilarSpawnPointCountString;
        _timelineSimilarSpawnPointCountText.gameObject.SetActive(_linkedWavesSingle.Count > 1);
        _deleteWaveButton.gameObject.SetActive(_linkedWavesSingle.Count <= 1);
    }

    #endregion

    #region Visual

    public void ChangeSelectedState(bool isSelected, bool isWavesSpawned)
    {
        _selectedSpawnedPoint.gameObject.SetActive(isSelected && isWavesSpawned);
        _notSelectedSpawnedPoint.gameObject.SetActive(!isSelected && isWavesSpawned);

        _selectedNotSpawnedPoint.gameObject.SetActive(isSelected && !isWavesSpawned);
        _notSelectedNotSpawnedPoint.gameObject.SetActive(!isSelected && !isWavesSpawned);
    }

    #endregion

    private string GetZerosFormattedString(int value)
    {
        return value switch
        {
            < 10 => $"0{value}",
            _ => $"{value}"
        };
    }
}