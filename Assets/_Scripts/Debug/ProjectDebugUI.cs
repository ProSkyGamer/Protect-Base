#region

using UnityEngine;
using Zenject;

#endregion

public class ProjectDebugUI : MonoBehaviour
{
    #region Variables & Refernces

    [SerializeField] private Transform _debugTransform;

    private bool _isVisible;

    private DebugLogsUIFactory _debugLogsUIFactory;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(DebugLogsUIFactory debugLogsUIFactory)
    {
        _debugLogsUIFactory = debugLogsUIFactory;
    }

    #endregion

    #region Visual

    public void AddLog(DebugLogSingle logSingle)
    {
        DebugLogSingleUI logSingleUI = _debugLogsUIFactory.Create(logSingle);
    }

    public void VisualToggle()
    {
        if (_isVisible)
            Hide();
        else
            Show();
    }

    public void Show()
    {
        _debugTransform.gameObject.SetActive(true);
        _isVisible = true;
    }

    public void Hide()
    {
        _debugTransform.gameObject.SetActive(false);
        _isVisible = false;
    }

    #endregion
}