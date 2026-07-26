#region

using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class SingleWaveMainInfoUI : MonoBehaviour, IInitializable
{
    #region Variables & References

    [SerializeField] private TextMeshProUGUI _waveSpawningTimeText;
    [SerializeField] private string _waveSpawningTimeStringFormat = "{0}:{1}";
    [SerializeField] private TextMeshProUGUI _waveSpawningEnemyTypeText;
    [SerializeField] private TextMeshProUGUI _waveSpawningEnemyCountText;
    [SerializeField] private TextMeshProUGUI _waveEnemyStatsValueText;

    private EnumTranslationValuesSO _enumTranslationValuesSO;
    protected OperationWave OperationWave;
    private StringFormatsSO _stringFormatsSO;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO, OperationWave waveSingle, StringFormatsSO stringFormatsSO)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;
        _stringFormatsSO = stringFormatsSO;
        OperationWave = waveSingle;
    }

    public virtual void Initialize()
    {
        float waveSpawningMinutes = Mathf.Floor(OperationWave.WaveSpawnTime / 60);
        float waveSpawningSeconds = OperationWave.WaveSpawnTime % 60;

        string waveSpawningMinutesString = waveSpawningMinutes switch
        {
            < 10 => $"0{waveSpawningMinutes}",
            var _ => $"{waveSpawningMinutes}"
        };

        string waveSpawningSecondsString = waveSpawningSeconds switch
        {
            < 10 => $"0{waveSpawningSeconds}",
            var _ => $"{waveSpawningSeconds}"
        };

        string waveSpawningTimeTextString =
            string.Format(_waveSpawningTimeStringFormat, waveSpawningMinutesString, waveSpawningSecondsString);

        _waveSpawningTimeText.text = waveSpawningTimeTextString;
        _waveSpawningEnemyTypeText.text = _enumTranslationValuesSO.GetEnemyTypeString(OperationWave.SpawningEnemyType);
        _waveSpawningEnemyCountText.text = OperationWave.SpawningEnemyCount.ToString();

        string waveEnemyStatsValueTextString = string.Format(_stringFormatsSO.WaveEnemyStatsValueFormatString,
            OperationWave.ReadonlyEnemyInitializationStats.MaxHealth, OperationWave.ReadonlyEnemyInitializationStats.BaseAtk,
            OperationWave.ReadonlyEnemyInitializationStats.BaseSpeed);

        _waveEnemyStatsValueText.text = waveEnemyStatsValueTextString;
    }

    #endregion
}