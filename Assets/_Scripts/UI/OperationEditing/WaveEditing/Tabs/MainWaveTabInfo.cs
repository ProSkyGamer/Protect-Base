#region

using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class MainWaveTabInfo : WaveTabInfo
{
    #region Variables & References

    [SerializeField] private NumberInputFieldFilterUI _spawnMinuteIF;
    [SerializeField] private NumberInputFieldFilterUI _spawnSecondIF;
    [SerializeField] private TMP_Dropdown _enemyTypeDropdown;
    [SerializeField] private NumberInputFieldFilterUI _enemySpawnCountIF;
    [SerializeField] private NumberInputFieldFilterUI _summonsSpawnCountIF;
    [SerializeField] private Transform _summonsFieldTransform;
    private float _maxOperationTime;

    private bool _isControllingTimeValue;

    private EnumTranslationValuesSO _enumTranslationValuesSO;
    private IOperationDataProvider _operationDataProvider;
    private EnemyBaseStatsSO _enemyBaseStatsSO;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO, IOperationDataProvider operationDataProvider,
        EnemyBaseStatsSO enemyBaseStatsSO)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;
        _operationDataProvider = operationDataProvider;
        _enemyBaseStatsSO = enemyBaseStatsSO;
    }

    public override void OperationStarted()
    {
        _spawnMinuteIF.SetInteractability(false);
        _spawnSecondIF.SetInteractability(false);
        _enemyTypeDropdown.interactable = false;
        _enemySpawnCountIF.SetInteractability(false);
        _summonsSpawnCountIF.SetInteractability(false);
    }

    public override void OperationEnded()
    {
        _spawnMinuteIF.SetInteractability(true);
        _spawnSecondIF.SetInteractability(true);
        _enemyTypeDropdown.interactable = true;
        _enemySpawnCountIF.SetInteractability(true);
        _summonsSpawnCountIF.SetInteractability(true);
    }

    public override void Initialize()
    {
        base.Initialize();

        InitializeDropdowns();

        _spawnMinuteIF.TextChanged += SpawnTimeAnyIF_OnTextChanged;
        _spawnSecondIF.TextChanged += SpawnTimeAnyIF_OnTextChanged;

        _enemyTypeDropdown.onValueChanged.AddListener(EnemyTypeDropdownValueChanged);

        _maxOperationTime = _operationDataProvider.MaxOperationLength;
        _maxOperationTime = Mathf.Floor(_maxOperationTime / 60) * 60f + 59f;

        float maxMinutesValue = Mathf.Floor(_maxOperationTime / 60f);
        _spawnMinuteIF.SetMaxValue(maxMinutesValue);

        EnemyType enemyType = (EnemyType)_enemyTypeDropdown.value;
        RequestTabReset(enemyType);
    }

    private void EnemyTypeDropdownValueChanged(int newEnemyTypeInt)
    {
        EnemyType newSelectedEnemyType = (EnemyType)newEnemyTypeInt;

        ChangeAdditionalTabVisibility(newSelectedEnemyType);

        _enemySpawnCountIF.SetMaxValue(
            _enemyBaseStatsSO.GetMaxEnemyWaveCount(newSelectedEnemyType));

        _enemySpawnCountIF.SetMinValue(1);

        _summonsSpawnCountIF.SetMinValue(1);

        _summonsSpawnCountIF.SetMaxValue(
            _enemyBaseStatsSO.GetMaxEnemySummonsCount(newSelectedEnemyType));

        RequestTabReset(newSelectedEnemyType);
    }

    private void InitializeDropdowns()
    {
        _enemyTypeDropdown.options.Clear();

        List<TMP_Dropdown.OptionData> allEnemyTypes = Enum.GetValues(typeof(EnemyType)).Cast<EnemyType>()
            .Select(enemyType => new TMP_Dropdown.OptionData(_enumTranslationValuesSO.GetEnemyTypeString(enemyType))).ToList();

        _enemyTypeDropdown.AddOptions(allEnemyTypes);
    }

    private void SpawnTimeAnyIF_OnTextChanged(string newValue)
    {
        if (_isControllingTimeValue)
            return;

        int currentSetSpawnTime = GetCurrentSetSpawnTime();

        if (currentSetSpawnTime > _maxOperationTime)
        {
            _isControllingTimeValue = true;

            _spawnMinuteIF.SetAndFilterText((currentSetSpawnTime / 60).ToString());
            _spawnSecondIF.SetAndFilterText((currentSetSpawnTime % 60).ToString());

            _isControllingTimeValue = false;
        }
    }

    #endregion

    #region Reset

    public override void SetWaveData(OperationWave operationWave)
    {
        int operationMinutes = (int)(operationWave.WaveSpawnTime / 60f);
        int operationSeconds = (int)(operationWave.WaveSpawnTime % 60f);

        _spawnMinuteIF.SetAndFilterText(operationMinutes.ToString());
        _spawnSecondIF.SetAndFilterText(operationSeconds.ToString());

        _enemyTypeDropdown.value = (int)operationWave.SpawningEnemyType;

        _enemySpawnCountIF.SetAndFilterText(operationWave.SpawningEnemyCount.ToString());
        int enemySummonsCount = 0;

        if (operationWave.ReadonlyEnemyInitializationStats is VehicleInitializationStats vehicleInitializationStats)
            enemySummonsCount = vehicleInitializationStats.SpawningSoldiersCount;

        _summonsSpawnCountIF.SetAndFilterText(enemySummonsCount.ToString());
    }

    private void ChangeAdditionalTabVisibility(EnemyType enemyType)
    {
        bool isShowingAdditionalTab = IsShowingAdditionalTab(enemyType);

        _summonsFieldTransform.gameObject.SetActive(isShowingAdditionalTab);
    }

    public override void SoftResetTabInfo(EnemyType enemyType)
    {
        ChangeAdditionalTabVisibility(enemyType);

        base.SoftResetTabInfo(enemyType);
    }

    public override void CancelCurrentActions()
    {
    }

    public override void HardResetTabInfo()
    {
        _isControllingTimeValue = true;

        _spawnMinuteIF.SetAndFilterText(0.ToString());
        _spawnSecondIF.SetAndFilterText(0.ToString());

        _isControllingTimeValue = false;

        _enemyTypeDropdown.value = 0;
        _enemySpawnCountIF.SetAndFilterText(1.ToString());

        _summonsSpawnCountIF.SetAndFilterText(1.ToString());
    }

    #endregion

    #region Get

    private bool IsShowingAdditionalTab(EnemyType enemyType)
    {
        bool isShowingAdditionalTab = enemyType is EnemyType.Vehicle;

        return isShowingAdditionalTab;
    }

    public override Dictionary<OperationStatSingle, object> GetAllTabOperationStats()
    {
        Dictionary<OperationStatSingle, object> allTabOperationState = new()
        {
            { OperationStatSingle.WaveSpawnTime, GetCurrentSetSpawnTime() },
            { OperationStatSingle.SpawnEnemyType, GetCurrentSetEnemyType() },
            { OperationStatSingle.SpawnEnemyCount, GetCurrentSetSpawnCount() },
            { OperationStatSingle.EnemySummonsCount, GetCurrentSetSummonsCount() }
        };

        return allTabOperationState;
    }

    private int GetCurrentSetSpawnTime()
    {
        int currentSetOperationMinutes = _spawnMinuteIF.GetIntValue();
        int currentSetOperationSeconds = _spawnSecondIF.GetIntValue();

        int currentSetSpawnTime = currentSetOperationMinutes * 60 + currentSetOperationSeconds;

        return currentSetSpawnTime;
    }

    private EnemyType GetCurrentSetEnemyType()
    {
        EnemyType currentSetEnemyType = (EnemyType)_enemyTypeDropdown.value;

        return currentSetEnemyType;
    }

    private int GetCurrentSetSpawnCount()
    {
        int currentSetSpawnCount = _enemySpawnCountIF.GetIntValue();

        return currentSetSpawnCount;
    }

    private int GetCurrentSetSummonsCount()
    {
        int currentSetSummonsCount = _summonsSpawnCountIF.GetIntValue();

        return currentSetSummonsCount;
    }

    #endregion

    public override void Dispose()
    {
        _spawnMinuteIF.TextChanged -= SpawnTimeAnyIF_OnTextChanged;
        _spawnSecondIF.TextChanged -= SpawnTimeAnyIF_OnTextChanged;
    }
}