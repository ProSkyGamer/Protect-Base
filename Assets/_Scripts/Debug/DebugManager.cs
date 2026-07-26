#region

using System;
using UnityEngine;
using Zenject;

#endregion

public class DebugLogSingle
{
    public DebugType DebugType;
    public string DebugMessage;
    public DateTime DebugTime;
    public Sprite LOGIcon;
}

public class DebugManager : IInitializable, IDisposable
{
    #region Event

    public event Action<DebugLogSingle> LogAdded;

    #endregion

    #region Variables & References

    private EnumTranslationValuesSO _enumTranslationValuesSO;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;
    }

    public void Initialize()
    {
        Application.logMessageReceived += Application_OnLogMessageReceived;
    }

    private void Application_OnLogMessageReceived(string condition, string stacktrace, LogType type)
    {
        if (!Application.isPlaying) return;

        AddLog(
            type is LogType.Log ? DebugType.Log :
            type is LogType.Warning ? DebugType.Warning :
            type is (LogType.Error or LogType.Exception) ? DebugType.Error : DebugType.Log, condition);
    }

    #endregion

    #region Debug

    private void AddLog(DebugType debugType, string debugMessage)
    {
        DebugLogSingle newDebugLog = new()
        {
            DebugType = debugType,
            DebugMessage = debugMessage,
            DebugTime = DateTime.Now,
            LOGIcon = _enumTranslationValuesSO.GetDebugTypeSprite(debugType)
        };

        LogAdded?.Invoke(newDebugLog);
    }

    #endregion

    public void Dispose()
    {
        Application.logMessageReceived -= Application_OnLogMessageReceived;
    }
}