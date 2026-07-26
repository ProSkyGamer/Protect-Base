#region

using System;
using Unity.Netcode;
using Zenject;

#endregion

public class OperationsManagerObserver : IInitializable, IOperationStatsDataProvider
{
    #region Events

    public event Action<OperationSavingStatType, object> DataChanged;

    #endregion

    #region Variables & References

    private readonly OperationsManager _operationsManager;
    private readonly CurrentEditingOperationManager _currentEditingOperationManager;
    private readonly SceneWeatherManager _sceneWeatherManager;

    #endregion

    #region Initialize

    public OperationsManagerObserver(OperationsManager operationsManager,
        CurrentEditingOperationManager currentEditingOperationManager, SceneWeatherManager sceneWeatherManager)
    {
        _operationsManager = operationsManager;
        _currentEditingOperationManager = currentEditingOperationManager;
        _sceneWeatherManager = sceneWeatherManager;
    }

    public void Initialize()
    {
        _operationsManager.OperationStarted += OperationsManager_OnOperationStarted;
    }

    private void OperationsManager_OnOperationStarted(ReadonlyOperationData startedOperation)
    {
        _currentEditingOperationManager.SetCurrentEditingOperationSingle(startedOperation);

        if (NetworkManager.Singleton.IsServer && startedOperation.OperationWeather != null)
        {
            _sceneWeatherManager.ChangeWeather(startedOperation.OperationWeather);

            DataChanged?.Invoke(OperationSavingStatType.WeatherSeason, startedOperation.OperationWeather.Season);
            DataChanged?.Invoke(OperationSavingStatType.WeatherTime, startedOperation.OperationWeather.Time);
            DataChanged?.Invoke(OperationSavingStatType.WeatherConditions, startedOperation.OperationWeather.Weather);
        }
    }

    #endregion
}