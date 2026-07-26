#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class RegistrationPageUI : BasePageUI, IInitializable, IDisposable
{
    #region Variables & References

    [SerializeField] private NumberInputFieldFilterUI _loginInputField;
    [SerializeField] private TMP_InputField _passwordInputField;
    [SerializeField] private Button _loginButton;
    [SerializeField] private ButtonSelectItemSingleUI _loginSelectedButton;
    [SerializeField] private string _successfulLoginNotificationText;
    [SerializeField] private string _unsuccessfulLoginNotificationText;

    private readonly CancellationTokenSource _notificationCancellationToken = new();

    private OperatorsLoginManager _loginManager;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(OperatorsLoginManager operatorsLoginManager)
    {
        _loginManager = operatorsLoginManager;
    }

    public void Initialize()
    {
        InitializeLimits();
        SubscribeToUIEvents();
    }

    private void InitializeLimits()
    {
        _loginInputField.SetMinValue(_loginManager.MinUserLoginIndex);
        _loginInputField.SetMaxValue(_loginManager.MaxUserLoginIndex);
    }

    private void SubscribeToUIEvents()
    {
        _loginButton.onClick.AddListener(() => { OnLoginButtonPressed().Forget(); });
    }

    private async UniTaskVoid OnLoginButtonPressed()
    {
        int login = _loginInputField.GetIntValue();
        string pin = _passwordInputField.text;

        bool isLoginedSuccessfully = _loginManager.WouldLoginSuccessful(login, pin);

        await _loginSelectedButton.DisplayNotification(isLoginedSuccessfully ? _successfulLoginNotificationText : _unsuccessfulLoginNotificationText);

        _loginInputField.SetAndFilterText("0");
        _passwordInputField.text = "";

        _loginManager.Login(login, pin);
    }

    #endregion

    public void Dispose()
    {
        _notificationCancellationToken.Cancel();
    }
}