#region

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class EnemyStatsWaveTabInfo : WaveTabInfo
{
    #region Variables & References

    [SerializeField] private TMP_Dropdown _enemyHealthValueDropdown;
    [SerializeField] private TMP_Dropdown _enemyAtkValueDropdown;
    [SerializeField] private TMP_Dropdown _enemySpeedValueDropdown;

    private EnumTranslationValuesSO _enumTranslationValuesSO;
    private EnemyType _currentEnemyType;
    private EnemyBaseStatsSO _enemyBaseStatsSO;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO, EnemyBaseStatsSO enemyBaseStatsSO)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;
        _enemyBaseStatsSO = enemyBaseStatsSO;
    }

    public override void OperationStarted()
    {
        _enemyHealthValueDropdown.interactable = false;
        _enemyAtkValueDropdown.interactable = false;
        _enemySpeedValueDropdown.interactable = false;
    }

    public override void OperationEnded()
    {
        _enemyHealthValueDropdown.interactable = true;
        _enemyAtkValueDropdown.interactable = true;
        _enemySpeedValueDropdown.interactable = true;
    }

    public override void Initialize()
    {
        base.Initialize();

        InitializeDropdownValues();
    }

    private void InitializeDropdownValues()
    {
        _enemyHealthValueDropdown.options.Clear();
        _enemyAtkValueDropdown.options.Clear();
        _enemySpeedValueDropdown.options.Clear();

        List<TMP_Dropdown.OptionData> optionsData =
            Enum.GetValues(typeof(EnemyStatSize)).Cast<EnemyStatSize>().Select(enemyStateSize =>
                new TMP_Dropdown.OptionData(_enumTranslationValuesSO.GetEnemyStatSizeString(enemyStateSize))).ToList();

        _enemyHealthValueDropdown.AddOptions(optionsData);
        _enemyAtkValueDropdown.AddOptions(optionsData);
        _enemySpeedValueDropdown.AddOptions(optionsData);
    }

    #endregion

    #region Reset

    public override void SoftResetTabInfo(EnemyType enemyType)
    {
        _currentEnemyType = enemyType;

        _enemyHealthValueDropdown.value = 0;
        _enemyAtkValueDropdown.value = 0;
        _enemySpeedValueDropdown.value = 0;

        if (enemyType is (EnemyType.Drone or EnemyType.BigSlowDrone or EnemyType.SmallSpeedDrone))
            FullyHideTab();
        else
            ShowTabButton();

        base.SoftResetTabInfo(enemyType);
    }

    public override void CancelCurrentActions()
    {
    }

    public override void HardResetTabInfo()
    {
        SoftResetTabInfo(_currentEnemyType);
    }

    public override void SetWaveData(OperationWave operationWave)
    {
        _enemyHealthValueDropdown.value =
            (int)_enemyBaseStatsSO.GetEnemyHealthStatIndexFromValue(operationWave.SpawningEnemyType,
                operationWave.ReadonlyEnemyInitializationStats.MaxHealth);

        _enemyAtkValueDropdown.value =
            (int)_enemyBaseStatsSO.GetEnemyAtkStatIndexFromValue(operationWave.SpawningEnemyType,
                operationWave.ReadonlyEnemyInitializationStats.BaseAtk);

        _enemySpeedValueDropdown.value =
            (int)_enemyBaseStatsSO.GetEnemySpeedStatIndexFromValue(operationWave.SpawningEnemyType,
                operationWave.ReadonlyEnemyInitializationStats.BaseSpeed);
    }

    #endregion

    #region Get

    public override Dictionary<OperationStatSingle, object> GetAllTabOperationStats()
    {
        Dictionary<OperationStatSingle, object> allTabOperationState = new()
        {
            { OperationStatSingle.EnemyHealth, GetCurrentSetEnemyHealth() },
            { OperationStatSingle.EnemyAtk, GetCurrentSetEnemyAtk() },
            { OperationStatSingle.EnemySpeed, GetCurrentSetEnemySpeed() }
        };

        return allTabOperationState;
    }

    private float GetCurrentSetEnemyHealth()
    {
        EnemyStatSize currentSetEnemyHealthStatSize = (EnemyStatSize)_enemyHealthValueDropdown.value;

        float enemyHealth = _enemyBaseStatsSO.GetEnemyHealthStat(_currentEnemyType, currentSetEnemyHealthStatSize);

        return enemyHealth;
    }

    private float GetCurrentSetEnemyAtk()
    {
        EnemyStatSize currentSetEnemyAtkStatSize = (EnemyStatSize)_enemyAtkValueDropdown.value;

        float enemyAtk = _enemyBaseStatsSO.GetEnemyAtkStat(_currentEnemyType, currentSetEnemyAtkStatSize);

        return enemyAtk;
    }

    private float GetCurrentSetEnemySpeed()
    {
        EnemyStatSize currentSetEnemySpeedStatSize = (EnemyStatSize)_enemySpeedValueDropdown.value;

        float enemySpeed = _enemyBaseStatsSO.GetEnemySpeedStat(_currentEnemyType, currentSetEnemySpeedStatSize);

        return enemySpeed;
    }

    #endregion

    public override void Dispose()
    {
    }
}