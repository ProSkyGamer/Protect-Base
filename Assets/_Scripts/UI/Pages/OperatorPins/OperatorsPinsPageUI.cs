#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class OperatorsPinsPageUI : BasePageUI, IInitializable, IDisposable
{
    #region Events

    public event Action<int, string> PinChanged;

    #endregion

    #region Variables & References

    [SerializeField] private NumberInputFieldFilterUI _operatorLoginInputFieldFilterUI;
    [SerializeField] private TMP_InputField _operatorPasswordInputFieldFilterUI;
    [SerializeField] private TMP_InputField _operatorPasswordConfirmationInputFieldFilterUI;
    [SerializeField] private ButtonSelectItemSingleUI _applyPasswordSelectableButton;
    private Button _applyPasswordButton;
    [SerializeField] private string _successfulPasswordNotificationText;
    [SerializeField] private string _notSuccessfulPasswordNotificationText;
    [SerializeField] private Button _quitButton;
    private readonly CancellationTokenSource _notificationCancellationToken = new();

    private ILoginDataProvider _loginDataProvider;

    public override bool IsCanHide => true;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ILoginDataProvider loginDataProvider)
    {
        _loginDataProvider = loginDataProvider;
    }

    public void Initialize()
    {
        _applyPasswordButton = _applyPasswordSelectableButton.GetButtonComponent();

        InitializeLimits();
        SubscribeToUIEvents();
    }

    private void SubscribeToUIEvents()
    {
        _applyPasswordButton.onClick.AddListener(() => { OnApplyPasswordButtonPressed(_notificationCancellationToken.Token).Forget(); });
        _quitButton.onClick.AddListener(OnQuitButtonPressed);
    }

    private async UniTaskVoid OnApplyPasswordButtonPressed(CancellationToken cancellationToken)
    {
        int loginId = _operatorLoginInputFieldFilterUI.GetIntValue();
        string pin = _operatorPasswordInputFieldFilterUI.text;

        bool isCorrect = pin !=
                         _operatorPasswordConfirmationInputFieldFilterUI.text;

        await _applyPasswordSelectableButton.DisplayNotification(isCorrect
            ? _successfulPasswordNotificationText
            : _notSuccessfulPasswordNotificationText);

        if (cancellationToken.IsCancellationRequested)
            return;

        _operatorPasswordInputFieldFilterUI.text = "";
        _operatorPasswordConfirmationInputFieldFilterUI.text = "";

        if (isCorrect)
            PinChanged?.Invoke(loginId, pin);
    }

    private void OnQuitButtonPressed()
    {
        RequestHide();
    }

    private void InitializeLimits()
    {
        _operatorLoginInputFieldFilterUI.SetMinValue(_loginDataProvider.MinUserLoginIndex);
        _operatorLoginInputFieldFilterUI.SetMaxValue(_loginDataProvider.MaxUserLoginIndex);
    }

    #endregion

    public void Dispose()
    {
        _notificationCancellationToken.Cancel();
    }
}