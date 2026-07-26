#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

public class MapPointDirectionLineUI : MonoBehaviour, IInitializable, IDisposable
{
    #region Veriables & References

    [SerializeField] private float _cyclingSymbolInterval = .5f;
    [SerializeField] private TextMeshProUGUI _mapPointDirectionLineText;
    [SerializeField] private bool _isDirectionRight = true;

    private string _pointDirectionOriginalText;
    private int _currentCyclingIndex;
    private readonly CancellationTokenSource _cyclingCancellationToken = new();

    #endregion

    #region Initialization

    [Inject]
    public void Construct(string cyclingText)
    {
        _mapPointDirectionLineText.text = cyclingText;
    }

    public void Initialize()
    {
        SetCurrentTextAsBase();

        ConstantlyCycleText(_cyclingSymbolInterval, _cyclingCancellationToken.Token).Forget();
    }

    private void SetCurrentTextAsBase()
    {
        _pointDirectionOriginalText = _mapPointDirectionLineText.text;
    }

    #endregion

    #region Update

    private async UniTaskVoid ConstantlyCycleText(float cyclingInterval, CancellationToken cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            CycleNextSymbol();

            await UniTask.WaitForSeconds(cyclingInterval, cancellationToken: cancellationToken);
        }
    }

    #endregion

    #region Replace

    private void CycleNextSymbol()
    {
        string cyclingTextString =
            GetCyclicSubstring(_pointDirectionOriginalText, _currentCyclingIndex, _pointDirectionOriginalText.Length);

        string fullDisplayString =
            _isDirectionRight ? $"{cyclingTextString}>" : $"<{cyclingTextString}";

        _mapPointDirectionLineText.text = fullDisplayString;
        _currentCyclingIndex = _isDirectionRight ? _currentCyclingIndex - 1 : _currentCyclingIndex + 1;
    }

    private string GetCyclicSubstring(string text, int startIndex, int length)
    {
        if (string.IsNullOrEmpty(text) || length <= 0)
            return string.Empty;

        string result = "";

        for (int i = 0; i < length; i++)
        {
            int stringIndex = (startIndex + i) % text.Length;

            if (stringIndex < 0)
                stringIndex += text.Length;

            result += text[stringIndex];
        }

        return result;
    }

    #endregion

    public void Dispose()
    {
        _cyclingCancellationToken.Cancel();
    }
}