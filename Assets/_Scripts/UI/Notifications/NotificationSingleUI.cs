#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class NotificationSingleUI : MonoBehaviour, IInitializable, IDisposable
{
    #region Events

    public event Action<NotificationSingleUI> NotificationDestroyed;

    #endregion

    #region Variables & References

    [SerializeField] private TextMeshProUGUI _notificationText;
    [SerializeField] private Image _notificationBackground;
    [SerializeField] private float _showingTime = 1.5f;
    [SerializeField] private float _hidingTime = 1f;

    private readonly CancellationTokenSource _notificationCancellationToken = new();

    #endregion

    #region Initialize

    [Inject]
    public void Construct(string notificationTextString)
    {
        _notificationText.text = notificationTextString;
    }

    public void Initialize()
    {
        Debug.Log($"[NotificationSingleUI.Initialize Line 43] {gameObject.name} notif started");
        NotificationLifeCycle(_showingTime, _hidingTime, _notificationCancellationToken.Token).Forget();
    }

    private async UniTaskVoid NotificationLifeCycle(float showingTime, float hidingTime, CancellationToken cancellationToken)
    {
        await UniTask.WaitForSeconds(showingTime, cancellationToken: cancellationToken);

        await UniTaskAsyncEnumerable.EveryUpdate().TakeUntil(UniTask.Delay(TimeSpan.FromSeconds(hidingTime), cancellationToken: cancellationToken))
            .ForEachAsync(_ =>
            {
                float deltaOpacity = -Time.deltaTime / hidingTime;
                SetTextOpacity(deltaOpacity);
            }, cancellationToken: cancellationToken);

        NotificationDestroyed?.Invoke(this);

        Destroy(gameObject);
    }

    public void SetTextOpacity(float deltaOpacity)
    {
        Color imageOriginalColor = _notificationBackground.color;
        imageOriginalColor.a += deltaOpacity;
        _notificationBackground.color = imageOriginalColor;

        Color textOriginalColor = _notificationText.color;
        textOriginalColor.a += deltaOpacity;
        _notificationText.color = textOriginalColor;
    }

    #endregion

    public void Dispose()
    {
        _notificationCancellationToken.Cancel();
    }
}