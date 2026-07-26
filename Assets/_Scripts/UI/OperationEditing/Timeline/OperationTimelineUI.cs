#region

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class OperationTimelineUI : MonoBehaviour, IInitializable, IOperationsStatusListener, IOperationUpdateListener
{
    #region Events

    public event Action<IReadOnlyList<OperationWave>> TimelinePointSelected;
    public event Action<int> WaveDeleted;

    #endregion

    #region Veriables & References

    [SerializeField] private TextMeshProUGUI _currentWavesCountText;

    private int _wavesCount;
    [SerializeField] private Transform _timelinePointsContainer;
    [SerializeField] private Transform _phantomPoint;
    [SerializeField] private Transform _topTimelinePointPosition;
    [SerializeField] private Transform _bottomTimelinePointPosition;
    [SerializeField] private float _minTimeForFullScroll = 60f;
    [SerializeField] private float _maxTimeForFullScroll = 100f;
    [SerializeField] private int _linesCountPerFullScroll = 20;
    [SerializeField] private TextMeshProUGUI _timelineLinesText;
    [SerializeField] private Color _normalTimelineColor;
    [SerializeField] private Color _completedTimelineColor;
    private float _currentTimeForFullScroll;
    private float _latestWaveSpawnTime;
    [SerializeField] private Scrollbar _timelineScrollbar;
    [SerializeField] private TextMeshProUGUI _minCurrentTimelinePointText;
    [SerializeField] private TextMeshProUGUI _maxTimelinePointText;

    private readonly List<TimelinePointUI> _allCurrentTimelinePoints = new();
    [SerializeField] private Transform _baseTimelinePointPosition;
    private TimelinePointUI _selectedTimelinePoint;

    private bool _isOperationActive;

    private IOperationDataProvider _operationDataProvider;
    private TimelinePointsUIFactory _timelinePointsUIFactory;
    private StringFormatsSO _stringFormatsSO;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(IOperationDataProvider operationDataProvider, TimelinePointsUIFactory timelinePointsUIFactory,
        StringFormatsSO stringFormatsSO)
    {
        _operationDataProvider = operationDataProvider;
        _timelinePointsUIFactory = timelinePointsUIFactory;
        _stringFormatsSO = stringFormatsSO;
    }

    public void OperationStarted()
    {
        _isOperationActive = true;
    }

    public void OperationEnded()
    {
        _isOperationActive = false;
    }

    public void UpdateOperationsVisuals()
    {
        UpdateTimelineText();

        UpdateTimelinePointSelections();
    }

    public void Initialize()
    {
        _phantomPoint.gameObject.SetActive(false);

        _timelineScrollbar.onValueChanged.AddListener(UpdateTimelineScroll);
    }

    private void UpdateTimelineScroll(float scrollValue)
    {
        if (scrollValue < 0)
            scrollValue = 0f;
        else if (scrollValue > 1f)
            scrollValue = 1f;

        float latestTimePoint = _latestWaveSpawnTime > 0 ? _latestWaveSpawnTime : _currentTimeForFullScroll;

        float topPointValue = (latestTimePoint - _currentTimeForFullScroll) * (1 - scrollValue);

        float bottomPointValue =
            (latestTimePoint - _currentTimeForFullScroll) * (1 - scrollValue) + _currentTimeForFullScroll;

        if (topPointValue < 0)
            topPointValue = 0;

        if (bottomPointValue < _currentTimeForFullScroll)
            bottomPointValue = _currentTimeForFullScroll;

        float topPointSpawnMinute = Mathf.Floor(topPointValue / 60f);
        float topPointSpawnSecond = Mathf.Floor(topPointValue % 60);

        string topPointSpawnMinuteString = GetZerosFormattedStringString(topPointSpawnMinute);
        string topPointSpawnSecondString = GetZerosFormattedStringString(topPointSpawnSecond);

        float bottomPointSpawnMinute = Mathf.Floor(bottomPointValue / 60f);
        float bottomPointSpawnSecond = Mathf.Floor(bottomPointValue % 60);

        string bottomPointSpawnMinuteString = GetZerosFormattedStringString(bottomPointSpawnMinute);
        string bottomPointSpawnSecondString = GetZerosFormattedStringString(bottomPointSpawnSecond);

        string topPointSpawnTimeString = string.Format(_stringFormatsSO.MinutesTimeFormatString, topPointSpawnMinuteString,
            topPointSpawnSecondString);

        string bottomPointSpawnTimeString = string.Format(_stringFormatsSO.MinutesTimeFormatString,
            bottomPointSpawnMinuteString,
            bottomPointSpawnSecondString);

        _minCurrentTimelinePointText.text = topPointSpawnTimeString;
        _maxTimelinePointText.text = bottomPointSpawnTimeString;
    }

    #endregion

    #region Visual

    public void TurnOffAllActivePoints()
    {
        _selectedTimelinePoint = null;
        UpdateTimelinePointSelections();
    }

    private void SetWavesCount(int wavesCount)
    {
        _wavesCount = wavesCount;

        _currentWavesCountText.text = wavesCount.ToString();
    }

    public void ShowPhantomTimelinePoint()
    {
        _phantomPoint.gameObject.SetActive(true);

        string currentWaveCountTextString = string.Format(_stringFormatsSO.CurrentWavesWithAdditionalFormatString, _wavesCount);

        _currentWavesCountText.text = currentWaveCountTextString;
    }

    public void HidePhantomTimelinePoint()
    {
        _phantomPoint.gameObject.SetActive(false);

        _currentWavesCountText.text = _wavesCount.ToString();
    }

    private void ClearTimeline()
    {
        foreach (TimelinePointUI timelinePoint in _allCurrentTimelinePoints)
        {
            timelinePoint.Selected -= OperationSingleUISelected;
            timelinePoint.DeleteRequested -= NewTimelinePoint_OnDeleteRequested;
        }

        foreach (Transform toDelete in _timelinePointsContainer.GetComponentsInChildren<Transform>())
        {
            if (toDelete == _timelinePointsContainer || toDelete == _baseTimelinePointPosition) continue;

            Destroy(toDelete.gameObject);
        }

        _allCurrentTimelinePoints.Clear();
    }

    #endregion

    #region Timeline

    private void UpdateTimelinePointSelections()
    {
        foreach (TimelinePointUI timelinePoint in _allCurrentTimelinePoints)
        {
            bool isSelected = _selectedTimelinePoint == timelinePoint;
            bool isSpawned = _isOperationActive && _operationDataProvider.CurrentOperationTime >= timelinePoint.WavesSpawnTime;
            timelinePoint.ChangeSelectedState(isSelected, isSpawned);
        }
    }

    public void UpdateTimeline(ReadonlyOperationData currentOperation)
    {
        ClearTimeline();

        _latestWaveSpawnTime = -1;

        if (currentOperation.AllOperationWaves.Count > 0)
            _latestWaveSpawnTime = currentOperation.AllOperationWaves.Max(wave => wave.WaveSpawnTime);

        _currentTimeForFullScroll = _latestWaveSpawnTime < _minTimeForFullScroll ? _minTimeForFullScroll :
            _latestWaveSpawnTime > _maxTimeForFullScroll ? _maxTimeForFullScroll :
            _latestWaveSpawnTime;

        SetWavesCount(currentOperation.AllOperationWaves.Count);
        UpdateTimelineText();

        float deltaAllowedTimeDifference = _currentTimeForFullScroll / _linesCountPerFullScroll;

        List<List<OperationWave>> allGroupedWavesSingle = currentOperation.AllOperationWaves.GroupBy(
            operationWave => Mathf.Floor(operationWave.WaveSpawnTime / deltaAllowedTimeDifference)).Select(group => group.ToList()).ToList();

        _timelineScrollbar.value = 1f;

        foreach (List<OperationWave> groupedWaveSingle in allGroupedWavesSingle)
        {
            AddTimelinePoint(groupedWaveSingle);
        }

        UpdateTimelinePointSelections();
    }

    private void UpdateTimelineText()
    {
        float timelineVisualTextLinesCount = _latestWaveSpawnTime / _currentTimeForFullScroll * _linesCountPerFullScroll;

        timelineVisualTextLinesCount = timelineVisualTextLinesCount < _linesCountPerFullScroll
            ? _linesCountPerFullScroll
            : timelineVisualTextLinesCount;

        float completedLinesCount = _operationDataProvider.CurrentOperationTime / _currentTimeForFullScroll *
                                    _linesCountPerFullScroll;

        string timelineVisualTextString = "";

        if (_isOperationActive)
            timelineVisualTextString += $"<color=#{ColorUtility.ToHtmlStringRGB(_completedTimelineColor)}>";

        for (int i = 0; i < timelineVisualTextLinesCount; i++)
        {
            if (_isOperationActive && i >= completedLinesCount)
                timelineVisualTextString += "</color>";

            timelineVisualTextString += "|\n";
        }

        _timelineLinesText.text = timelineVisualTextString;
        _timelineLinesText.color = _normalTimelineColor;

        _currentWavesCountText.text = _wavesCount.ToString();
    }

    private void AddTimelinePoint(IReadOnlyList<OperationWave> linkedWavesSingle)
    {
        float timelinePointYPosition =
            (_bottomTimelinePointPosition.position.y - _topTimelinePointPosition.position.y) *
            (linkedWavesSingle[0].WaveSpawnTime / _currentTimeForFullScroll) + _topTimelinePointPosition.position.y;

        Vector3 timelinePointPosition =
            new(_baseTimelinePointPosition.position.x, timelinePointYPosition, _baseTimelinePointPosition.position.z);

        TimelinePointUI newTimelinePoint = _timelinePointsUIFactory.Create(linkedWavesSingle);
        newTimelinePoint.transform.position = timelinePointPosition;
        newTimelinePoint.Selected += OperationSingleUISelected;
        newTimelinePoint.DeleteRequested += NewTimelinePoint_OnDeleteRequested;

        _allCurrentTimelinePoints.Add(newTimelinePoint);
    }

    private void OperationSingleUISelected(
        IReadOnlyList<OperationWave> timelinePointLinkedWaves)
    {
        TimelinePointSelected?.Invoke(timelinePointLinkedWaves);

        _selectedTimelinePoint = _allCurrentTimelinePoints.Find(timelinePointUI => Equals(timelinePointUI.LinkedWaves, timelinePointLinkedWaves));

        UpdateTimelinePointSelections();
    }

    private void NewTimelinePoint_OnDeleteRequested(List<OperationWave> deletingWaves)
    {
        foreach (OperationWave deletingWave in deletingWaves)
        {
            WaveDeleted?.Invoke(deletingWave.WaveIndex);
        }
    }

    #endregion

    private string GetZerosFormattedStringString(float value)
    {
        return value switch
        {
            < 10 => $"0{value}",
            _ => $"{value}"
        };
    }
}