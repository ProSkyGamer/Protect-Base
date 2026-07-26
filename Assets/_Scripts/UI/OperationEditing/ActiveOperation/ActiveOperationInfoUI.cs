#region

using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class ActiveOperationInfoUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _currentOperationTimeValueText;
    [SerializeField] private TextMeshProUGUI _wavesSpawnedCountText;
    [SerializeField] private TextMeshProUGUI _currentAliveEnemiesCountText;

    private IOperationDataProvider _operationDataProvider;
    private StringFormatsSO _stringFormatsSO;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(IOperationDataProvider operationDataProvider, StringFormatsSO stringFormatsSO)
    {
        _operationDataProvider = operationDataProvider;
        _stringFormatsSO = stringFormatsSO;
    }

    #endregion

    #region Visual

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public void UpdateVisual()
    {
        float currentOperationTime = _operationDataProvider.CurrentOperationTime;
        float operationTimeMinute = Mathf.Floor(currentOperationTime / 60f);
        float operationTimeSecond = Mathf.Floor(currentOperationTime % 60);

        string waveSpawnMinuteString = operationTimeMinute switch
        {
            < 10 => $"0{operationTimeMinute}",
            _ => $"{operationTimeMinute}"
        };

        string waveSpawnSecondString = operationTimeSecond switch
        {
            < 10 => $"0{operationTimeSecond}",
            _ => $"{operationTimeSecond}"
        };

        string currentOperationTimeString =
            string.Format(_stringFormatsSO.MinutesTimeFormatString, waveSpawnMinuteString, waveSpawnSecondString);

        _currentOperationTimeValueText.text = currentOperationTimeString;

        int wavesTotalCount = _operationDataProvider.TotalWavesCount;
        int wavesSpawnedCount = _operationDataProvider.SpawnedWavesCount;

        string wavesSpawnedCountString =
            string.Format(_stringFormatsSO.WavesSpawnedCountFormatString, wavesSpawnedCount, wavesTotalCount);

        _wavesSpawnedCountText.text = wavesSpawnedCountString;

        int enemiesAlive = _operationDataProvider.CurrentlyAliveEnemies;
        string enemiesAliveString = string.Format(_stringFormatsSO.CurrentAliveEnemiesCountFormatString, enemiesAlive);
        _currentAliveEnemiesCountText.text = enemiesAliveString;
    }

    #endregion
}