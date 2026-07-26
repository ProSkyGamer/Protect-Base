#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

[Serializable]
public class AlarmTypeText
{
    public AlarmType AlarmType;
    public string AlarmTypeFullName;
    public string AlarmTypeShortName;
}

[Serializable]
public class EnemyStatSizeText
{
    public EnemyStatSize EnemyStatSize;
    public string StatSizeText;
}

[Serializable]
public class EnemiesIconSprites
{
    public EnemyType EnemyType;
    public Sprite EnemyIconSprite;
}

[Serializable]
public class EnemyTypeString
{
    public EnemyType EnemyType;
    public string EnemyTypeText;
}

[Serializable]
public class DebugTypeSprite
{
    public DebugType DebugType;
    public Sprite DebugLogSprite;
}

[Serializable]
public class FiringMachineAmmoTypeText
{
    public AmmoType AmmoType;
    public string FiringMachineAmmoTypeName;
}

[Serializable]
public class FiringMachineEnableTypeText
{
    public FiringMachineEnableType FiringMechanismEnableType;
    public string FiringMechanismEnableTypeFullName;
    public string FiringMechanismEnableTypeShortName;
}

[Serializable]
public class WeatherActivationTimeText
{
    public WeatherActivationTime WeatherActivationTime;
    public string ActivationTimeText;
}

[Serializable]
public class WeatherActivationSeasonText
{
    public WeatherActivationSeason WeatherActivationSeason;
    public string ActivationSeasonText;
}

[Serializable]
public class WeatherActivationConditionText
{
    public WeatherActivationCondition WeatherActivationCondition;
    public string WeatherConditionText;
}

//[CreateAssetMenu()]
public class EnumTranslationValuesSO : ScriptableObject
{
    #region Firing Machine Enable Type

    [SerializeField] private List<FiringMachineEnableTypeText> _allFiringMechanismEnableTypesText;

    public string GetFiringMachineEnableTypeFullText(FiringMachineEnableType firingMachineEnableType)
    {
        FiringMachineEnableTypeText enableTypeText = _allFiringMechanismEnableTypesText.Find(firingMachineEnableTypeText =>
            firingMachineEnableTypeText.FiringMechanismEnableType == firingMachineEnableType);

        return enableTypeText == null ? "" : enableTypeText.FiringMechanismEnableTypeFullName;
    }

    public string GetFiringMachineEnableTypeShortText(FiringMachineEnableType firingMachineEnableType)
    {
        FiringMachineEnableTypeText enableTypeText = _allFiringMechanismEnableTypesText.Find(firingMachineEnableTypeText =>
            firingMachineEnableTypeText.FiringMechanismEnableType == firingMachineEnableType);

        return enableTypeText == null ? "" : enableTypeText.FiringMechanismEnableTypeFullName;
    }

    #endregion

    #region Alarm Type

    [SerializeField] private List<AlarmTypeText> _allAlarmTypesText;

    public string GetAlarmTypeFullString(AlarmType alarmType)
    {
        AlarmTypeText alarmTypeText = _allAlarmTypesText.Find(alarmTypeText => alarmTypeText.AlarmType == alarmType);

        return alarmTypeText == null ? "" : alarmTypeText.AlarmTypeFullName;
    }

    public string GetAlarmTypeShortString(AlarmType alarmType)
    {
        AlarmTypeText alarmTypeText = _allAlarmTypesText.Find(alarmTypeText => alarmTypeText.AlarmType == alarmType);

        return alarmTypeText == null ? "" : alarmTypeText.AlarmTypeShortName;
    }

    #endregion

    #region Firing Machine Ammo Type

    [SerializeField] private List<FiringMachineAmmoTypeText> _allFiringMachineAmmoTypesText;

    public string GetFiringMachineAmmoTypeString(AmmoType ammoType)
    {
        FiringMachineAmmoTypeText firingMachineAmmoTypeText = _allFiringMachineAmmoTypesText.Find(firingMachineAmmoTypeText =>
            firingMachineAmmoTypeText.AmmoType == ammoType);

        return firingMachineAmmoTypeText == null ? "" : firingMachineAmmoTypeText.FiringMachineAmmoTypeName;
    }

    #endregion

    #region Enemy Type

    [SerializeField] private List<EnemyTypeString> _allEnemyTypesText;

    public string GetEnemyTypeString(EnemyType enemyType)
    {
        EnemyTypeString enemyTypeString = _allEnemyTypesText.Find(enemyTypeText => enemyTypeText.EnemyType == enemyType);

        return enemyTypeString == null ? "" : enemyTypeString.EnemyTypeText;
    }

    #endregion

    #region Enemy Stat Size

    [SerializeField] private List<EnemyStatSizeText> _allEnemyStatSizesText;

    public string GetEnemyStatSizeString(EnemyStatSize enemyStatSize)
    {
        EnemyStatSizeText enemyStatSizeText =
            _allEnemyStatSizesText.Find(enemyStatSizeText => enemyStatSizeText.EnemyStatSize == enemyStatSize);

        return enemyStatSizeText == null ? "" : enemyStatSizeText.StatSizeText;
    }

    #endregion

    #region Weather Activation Conditions

    [SerializeField] private List<WeatherActivationTimeText> _allWeatherActivationTimesText;
    [SerializeField] private List<WeatherActivationSeasonText> _allWeatherActivationSeasonsText;
    [SerializeField] private List<WeatherActivationConditionText> _allWeatherActivationConditionsText;

    public string GetWeatherActivationTimeStringTranslation(WeatherActivationTime weatherActivationTime)
    {
        WeatherActivationTimeText weatherActivationTimeText =
            _allWeatherActivationTimesText.Find(weatherActivationTimeText =>
                weatherActivationTimeText.WeatherActivationTime == weatherActivationTime);

        return weatherActivationTimeText == null ? "" : weatherActivationTimeText.ActivationTimeText;
    }

    public string GetWeatherActivationSeasonStringTranslation(WeatherActivationSeason weatherActivationSeason)
    {
        WeatherActivationSeasonText weatherActivationTimeText =
            _allWeatherActivationSeasonsText.Find(weatherActivationSeasonText =>
                weatherActivationSeasonText.WeatherActivationSeason == weatherActivationSeason);

        return weatherActivationTimeText == null ? "" : weatherActivationTimeText.ActivationSeasonText;
    }

    public string GetWeatherActivationConditionStringTranslation(WeatherActivationCondition weatherActivationCondition)
    {
        WeatherActivationConditionText weatherActivationTimeText =
            _allWeatherActivationConditionsText.Find(weatherActivationConditionText =>
                weatherActivationConditionText.WeatherActivationCondition == weatherActivationCondition);

        return weatherActivationTimeText == null ? "" : weatherActivationTimeText.WeatherConditionText;
    }

    #endregion

    #region Enemy Type

    [SerializeField] private List<EnemiesIconSprites> _allEnemyIconSprites;

    public Sprite GetEnemyTypeSprite(EnemyType enemyType)
    {
        EnemiesIconSprites enemyTypeSprite =
            _allEnemyIconSprites.Find(enemyTypeSprite => enemyTypeSprite.EnemyType == enemyType);

        return enemyTypeSprite.EnemyIconSprite;
    }

    #endregion

    #region Debug Type

    [SerializeField] private List<DebugTypeSprite> _allDebugTypeIconSprites;

    public Sprite GetDebugTypeSprite(DebugType debugType)
    {
        DebugTypeSprite enemyTypeSprite =
            _allDebugTypeIconSprites.Find(debugTypeSprite => debugTypeSprite.DebugType == debugType);

        return enemyTypeSprite.DebugLogSprite;
    }

    #endregion
}