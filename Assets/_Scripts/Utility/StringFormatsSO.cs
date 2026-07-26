#region

using UnityEngine;

#endregion

//[CreateAssetMenu()]
public class StringFormatsSO : ScriptableObject
{
    #region Varibles & References

    [SerializeField] private string _currentDateFormatString = "{0}.{1}.{2} {3}";
    [SerializeField] private string _hoursFormatString = "{0}:{1}:{2}";
    [SerializeField] private string _minutesTimeFormatString = "{0}:{1}";

    [SerializeField] private string _temperatureFormatString = "{0} *C";
    [SerializeField] private string _pressureFormatString = "{0} мм";
    [SerializeField] private string _anglesFormatString = "{0}*";

    [SerializeField] private string _eventTextFormatString = "Тревога {0}{1} СУ {2}";
    [SerializeField] private string _firingMachineNameFormatString = "СУ  #{0}";
    [SerializeField] private string _distanceFormatString = "{0} м";
    [SerializeField] private string _currentHealthFormatString = "{0}/{1}";

    [SerializeField] private string _wavesSpawnedCountFormatString = "{0}/{1}";
    [SerializeField] private string _timelineSimilarSpawnPointFormatString = "x{0}";
    [SerializeField] private string _currentAliveEnemiesCountFormatString = "{0}";
    [SerializeField] private string _currentWavesWithAdditionalFormatString = "{0} + 1";
    [SerializeField] private string _waveEnemyStatsValueFormatString = "{0}/{1}/{2}";

    #endregion

    #region Properties

    public string CurrentDateFormatString => _currentDateFormatString;
    public string HoursFormatString => _hoursFormatString;
    public string MinutesTimeFormatString => _minutesTimeFormatString;

    public string TemperatureFormatString => _temperatureFormatString;
    public string PressureFormatString => _pressureFormatString;
    public string AnglesFormatString => _anglesFormatString;

    public string EventTextFormatString => _eventTextFormatString;
    public string FiringMachineNameFormatString => _firingMachineNameFormatString;
    public string DistanceFormatString => _distanceFormatString;
    public string CurrentHealthFormatString => _currentHealthFormatString;

    public string WavesSpawnedCountFormatString => _wavesSpawnedCountFormatString;
    public string TimelineSimilarSpawnPointFormatString => _timelineSimilarSpawnPointFormatString;
    public string CurrentAliveEnemiesCountFormatString => _currentAliveEnemiesCountFormatString;
    public string CurrentWavesWithAdditionalFormatString => _currentWavesWithAdditionalFormatString;
    public string WaveEnemyStatsValueFormatString => _waveEnemyStatsValueFormatString;

    #endregion
}