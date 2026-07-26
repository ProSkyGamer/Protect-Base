#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class GameKernel : MonoKernel
{
    [Inject] private TCPSceneResetManager _tcpSceneResetManager;
    [Inject] private DevInterfaceManager _devInterfaceManager;
    [Inject] private DutyModePageObserver _dutyModePageObserver;
    [Inject] private OperationsManager _operationsManager;
    [Inject] private OperationUpdateManager _operationUpdateManager;
    [Inject] private OperationMapManagerUIObserver _operationMapManagerUI;

    [Inject] private DynamicInjector _dynamicInjector;

    [Inject(Optional = true, Source = InjectSources.Local)]
    private List<ILateInitializable> _allLateInitiailizables;

    [Inject(Optional = true, Source = InjectSources.Local)]
    private List<ISceneResettable> _allSceneResettables;

    [Inject(Optional = true, Source = InjectSources.Local)]
    private List<IDevInterfaceListener> _allDevInterfaceListeners;

    [Inject(Optional = true, Source = InjectSources.Local)]
    private List<IDutyInterfaceListener> _allDutyInterfaceListeners;

    [Inject(Optional = true, Source = InjectSources.Local)]
    private List<IOperationsStatusListener> _allOperationStatusListeners;

    [Inject(Optional = true, Source = InjectSources.Local)]
    private List<IOperationUpdateListener> _allOperationsUpdateListeners;

    [Inject(Optional = true, Source = InjectSources.Local)]
    private List<IPathPointCoordinatesListener> _allPathPointsCoordinatesListeners;

    public override void Start()
    {
        base.Start();

        _dynamicInjector.InterfaceInjected += DynamicInjector_OnInterfaceInjected;

        _tcpSceneResetManager.SceneReset += TcpSceneResetManager_OnSceneReset;
        _devInterfaceManager.VisibilityChanged += DevInterfaceManager_OnVisibilityChanged;
        _dutyModePageObserver.DutyModeActivated += DutyModePageObserver_OnDutyModeActivated;
        _dutyModePageObserver.DutyModeDeactivated += DutyModePageObserver_OnDutyModeDeactivated;
        _operationsManager.OperationStarted += OperationsManager_OnOperationStarted;
        _operationsManager.OperationStopped += OperationsManager_OnOperationStopped;
        _operationsManager.OperationStopped += OperationsManager_OnOperationStopped;
        _operationUpdateManager.ActiveOperationUpdated += OperationUpdatedManagerActiveOperationUpdated;
        _operationMapManagerUI.MapPointSet += OperationMapManagerUI_OnMapPointSet;
        _operationMapManagerUI.CanceledListeningForMapPoint += OperationMapManagerUIOnCanceledListeningForMapPoint;

        TriggerAllLateInitializables();
    }

    private void DynamicInjector_OnInterfaceInjected(Type injectingType, object injectingObject)
    {
        if (injectingType == typeof(ISceneResettable))
            _allSceneResettables.Add(injectingObject as ISceneResettable);
        else if (injectingType == typeof(IDevInterfaceListener))
            _allDevInterfaceListeners.Add(injectingObject as IDevInterfaceListener);
        else if (injectingType == typeof(IDutyInterfaceListener))
            _allDutyInterfaceListeners.Add(injectingObject as IDutyInterfaceListener);
        else if (injectingType == typeof(IOperationsStatusListener))
            _allOperationStatusListeners.Add(injectingObject as IOperationsStatusListener);
        else if (injectingType == typeof(IOperationUpdateListener))
            _allOperationsUpdateListeners.Add(injectingObject as IOperationUpdateListener);
        else if (injectingType == typeof(IPathPointCoordinatesListener))
            _allPathPointsCoordinatesListeners.Add(injectingObject as IPathPointCoordinatesListener);
    }

    private void OperationMapManagerUIOnCanceledListeningForMapPoint()
    {
        foreach (IPathPointCoordinatesListener pathPointCoordinatesListener in _allPathPointsCoordinatesListeners)
        {
            pathPointCoordinatesListener?.PathPointCoordinatesSelectionCanceled();
        }
    }

    private void OperationMapManagerUI_OnMapPointSet(Vector2 mapPointPosition, Vector3 worldPointPosition)
    {
        foreach (IPathPointCoordinatesListener pathPointCoordinatesListener in _allPathPointsCoordinatesListeners)
        {
            pathPointCoordinatesListener?.PathPointCoordinatesSelected(mapPointPosition, worldPointPosition);
        }
    }

    private void TriggerAllLateInitializables()
    {
        foreach (ILateInitializable iLateInitiailizable in _allLateInitiailizables)
        {
            iLateInitiailizable?.LateInitialize();
        }
    }

    private void OperationUpdatedManagerActiveOperationUpdated()
    {
        foreach (IOperationUpdateListener operationsStatusListener in _allOperationsUpdateListeners)
        {
            operationsStatusListener?.UpdateOperationsVisuals();
        }
    }

    private void OperationsManager_OnOperationStarted(ReadonlyOperationData _)
    {
        foreach (IOperationsStatusListener operationsStatusListener in _allOperationStatusListeners)
        {
            operationsStatusListener?.OperationStarted();
        }
    }

    private void OperationsManager_OnOperationStopped()
    {
        foreach (IOperationsStatusListener operationsStatusListener in _allOperationStatusListeners)
        {
            operationsStatusListener?.OperationEnded();
        }
    }

    private void DutyModePageObserver_OnDutyModeDeactivated()
    {
        foreach (IDutyInterfaceListener dutyInterfaceListener in _allDutyInterfaceListeners)
        {
            dutyInterfaceListener?.DutyInterfaceDeactivated();
        }
    }

    private void DutyModePageObserver_OnDutyModeActivated(FiringMachinesPageType pageType)
    {
        foreach (IDutyInterfaceListener dutyInterfaceListener in _allDutyInterfaceListeners)
        {
            dutyInterfaceListener?.DutyInterfaceActivated(pageType);
        }
    }

    private void DevInterfaceManager_OnVisibilityChanged(bool isActive)
    {
        foreach (IDevInterfaceListener devInterfaceListener in _allDevInterfaceListeners)
        {
            if (isActive)
                devInterfaceListener?.DevInterfaceActivated();
            else
                devInterfaceListener?.DevInterfaceDeactivated();
        }
    }

    private void TcpSceneResetManager_OnSceneReset()
    {
        foreach (ISceneResettable sceneResettable in _allSceneResettables)
        {
            sceneResettable?.OnSceneReset();
        }
    }

    public override void OnDestroy()
    {
        base.OnDestroy();

        _dynamicInjector.InterfaceInjected -= DynamicInjector_OnInterfaceInjected;

        _tcpSceneResetManager.SceneReset -= TcpSceneResetManager_OnSceneReset;
        _devInterfaceManager.VisibilityChanged -= DevInterfaceManager_OnVisibilityChanged;
        _dutyModePageObserver.DutyModeActivated -= DutyModePageObserver_OnDutyModeActivated;
        _dutyModePageObserver.DutyModeDeactivated -= DutyModePageObserver_OnDutyModeDeactivated;
        _operationsManager.OperationStarted -= OperationsManager_OnOperationStarted;
        _operationsManager.OperationStopped -= OperationsManager_OnOperationStopped;
        _operationsManager.OperationStopped -= OperationsManager_OnOperationStopped;
        _operationUpdateManager.ActiveOperationUpdated -= OperationUpdatedManagerActiveOperationUpdated;
        _operationMapManagerUI.MapPointSet -= OperationMapManagerUI_OnMapPointSet;
        _operationMapManagerUI.CanceledListeningForMapPoint -= OperationMapManagerUIOnCanceledListeningForMapPoint;
    }
}