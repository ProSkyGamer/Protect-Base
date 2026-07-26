#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class DevInputHandler : IInitializable, IDisposable
{
    private readonly List<IDevInput> _allDevInputs = new();
    private readonly ProjectDebugUI _projectDebugUI;
    private readonly AppSettingsUI _appSettingsUI;
    private readonly OperationSetupUI _operationSetupUI;
    private readonly OperationMapManagerUIObserver _operationMapManagerObserver;
    private readonly SceneWeatherManager _sceneWeatherManager;

    public DevInputHandler(List<IDevInput> allDevInputs, ProjectDebugUI projectDebugUI, AppSettingsUI appSettingsUI,
        OperationSetupUI operationSetupUI, OperationMapManagerUIObserver operationMapManagerObserver,
        SceneWeatherManager sceneWeatherManager)
    {
        _allDevInputs.AddRange(allDevInputs);
        _projectDebugUI = projectDebugUI;
        _appSettingsUI = appSettingsUI;
        _operationSetupUI = operationSetupUI;
        _operationMapManagerObserver = operationMapManagerObserver;
        _sceneWeatherManager = sceneWeatherManager;
    }

    public void Initialize()
    {
        foreach (IDevInput devInput in _allDevInputs)
        {
            devInput.LogsToggle += DevInput_OnLogsToggle;
            devInput.SettingsShow += DevInput_OnSettingsShow;
            devInput.OperationManagerToggle += DevInput_OnOperationManagerToggle;
            devInput.ChangeSkybox += DevInput_OnChangeSkybox;
            devInput.MouseClick += DevInput_OnMouseClick;
            devInput.CloseInterface += DevInput_OnCloseInterface;
            devInput.MousePositionChanged += DevInput_OnMousePositionChanged;
        }

        _operationMapManagerObserver.StartedListeningForMapPoint += OperationMapManagerObserverStartedListeningForMapPoint;

        _operationMapManagerObserver.CanceledListeningForMapPoint += OperationMapManagerObserverCanceledListeningForMapPoint;
        _operationMapManagerObserver.MapPointSet += OperationMapManagerObserverOnMapPointSet;
    }

    private void OperationMapManagerObserverOnMapPointSet(Vector2 _, Vector3 __)
    {
        foreach (IDevInput devInput in _allDevInputs)
        {
            devInput.StopListeningForMousePosition();
        }
    }

    private void OperationMapManagerObserverCanceledListeningForMapPoint()
    {
        foreach (IDevInput devInput in _allDevInputs)
        {
            devInput.StopListeningForMousePosition();
        }
    }

    private void OperationMapManagerObserverStartedListeningForMapPoint()
    {
        foreach (IDevInput devInput in _allDevInputs)
        {
            devInput.StartListeningForMousePosition();
        }
    }

    private void DevInput_OnLogsToggle()
    {
        _projectDebugUI.VisualToggle();
    }

    private void DevInput_OnSettingsShow()
    {
        Debug.Log($"[DevInputHandler.DevInput_OnSettingsShow Line 81] hey show u");

        _appSettingsUI.Show();
    }

    private void DevInput_OnOperationManagerToggle()
    {
        if (_operationSetupUI.IsShown)
            _operationSetupUI.Hide();
        else
            _operationSetupUI.Show();
    }

    private void DevInput_OnChangeSkybox()
    {
        ReadonlyWeatherActivationConditions changingWeather = _sceneWeatherManager.GetNextTimeSettings();

        _sceneWeatherManager.ChangeWeather(changingWeather);
    }

    private void DevInput_OnMouseClick()
    {
        _operationSetupUI.InteractWithInterface();
    }

    private void DevInput_OnCloseInterface()
    {
        _operationSetupUI.CloseLocalInterface();
    }

    private void DevInput_OnMousePositionChanged(Vector2 newMousePosition)
    {
        _operationMapManagerObserver.ChangeMousePosition(newMousePosition);
    }

    public void Dispose()
    {
        foreach (IDevInput devInput in _allDevInputs)
        {
            devInput.LogsToggle -= DevInput_OnLogsToggle;
            devInput.SettingsShow -= DevInput_OnSettingsShow;
            devInput.OperationManagerToggle -= DevInput_OnOperationManagerToggle;
            devInput.ChangeSkybox -= DevInput_OnChangeSkybox;
            devInput.MouseClick -= DevInput_OnMouseClick;
            devInput.CloseInterface -= DevInput_OnCloseInterface;
            devInput.MousePositionChanged -= DevInput_OnMousePositionChanged;
        }
    }
}