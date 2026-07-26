#region

using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class DebugLogSingleUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private Image _logIconImage;
    [SerializeField] private TextMeshProUGUI _logText;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(DebugLogSingle logSingle)
    {
        SetLog(logSingle);
    }

    private void SetLog(DebugLogSingle logSingle)
    {
        _logIconImage.sprite = logSingle.LOGIcon;

        _logText.text =
            $"[{logSingle.DebugTime.Hour}:{logSingle.DebugTime.Minute}:{logSingle.DebugTime.Second}]: {logSingle.DebugMessage}";
    }

    #endregion
}