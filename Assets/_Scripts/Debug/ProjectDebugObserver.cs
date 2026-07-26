#region

using System;
using Zenject;

#endregion

public class ProjectDebugObserver : IInitializable, IDisposable
{
    #region Variables & References

    private ProjectDebugUI _projectDebugUI;
    private DebugManager _debugManager;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ProjectDebugUI projectDebugUI, DebugManager debugManager)
    {
        _projectDebugUI = projectDebugUI;
        _debugManager = debugManager;
    }

    public void Initialize()
    {
        _debugManager.LogAdded += DebugManager_OnLogAdded;

        _projectDebugUI.Hide();
    }

    private void DebugManager_OnLogAdded(DebugLogSingle logSingle)
    {
        _projectDebugUI.AddLog(logSingle);
    }

    #endregion

    public void Dispose()
    {
        _debugManager.LogAdded -= DebugManager_OnLogAdded;
    }
}