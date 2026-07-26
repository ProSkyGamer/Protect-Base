#region

using System;
using Cysharp.Threading.Tasks;
using UnityEngine.Device;
using Zenject;

#endregion

public class AppSettingsObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly string _notificationText = "Требуется перезапуск приложения \nПерезапустить?";
    private readonly AppSettingsManager _appSettingsManager;
    private readonly AppSettingsUI _appSettingsUI;
    private readonly FullscreenNotificationUI _fullscreenNotificationUI;
    private readonly IDataSavingManager _dataSavingManager;

    #endregion

    #region Initialization

    public AppSettingsObserver(AppSettingsManager appSettingsManager, AppSettingsUI appSettingsUI, FullscreenNotificationUI fullscreenNotificationUI,
        IDataSavingManager dataSavingManager)
    {
        _appSettingsManager = appSettingsManager;
        _appSettingsUI = appSettingsUI;
        _fullscreenNotificationUI = fullscreenNotificationUI;
        _dataSavingManager = dataSavingManager;
    }

    public void Initialize()
    {
        _appSettingsUI.SetValues(_dataSavingManager.GetSavedAppSettings());

        _appSettingsUI.DataChanged += AppSettingsUIDataChanged;
        _appSettingsUI.CancelButtonClicked += AppSettingsUI_OnCancelButtonClicked;

        _fullscreenNotificationUI.Hide();
        _appSettingsUI.Hide();
    }

    private void AppSettingsUI_OnCancelButtonClicked()
    {
        _appSettingsUI.ResetValues();

        _appSettingsUI.Hide();
    }

    private void AppSettingsUIDataChanged(bool isNeedsRestart)
    {
        SaveDataAsync(isNeedsRestart).Forget();
    }

    #endregion

    #region Saving

    private async UniTaskVoid SaveDataAsync(bool isNeedsRestart)
    {
        ClientType selectedClientType = _appSettingsUI.GetCurrentSelectedClientType();
        WindowType selectedWindowType = _appSettingsUI.GetCurrentSelectedWindowType();
        string netcodeIP = _appSettingsUI.GetNetcodeIP();
        string tcpIP = _appSettingsUI.GetTcpIP();
        int tcpPort = _appSettingsUI.GetTCPPort();

        bool isSaving = isNeedsRestart == false;

        if (isNeedsRestart)
        {
            _fullscreenNotificationUI.Show(_notificationText);
            isSaving = await AwaitForNotificationResult();
        }

        if (isSaving)
        {
            _appSettingsUI.SaveValues();

            _appSettingsManager.SaveData(selectedClientType, selectedWindowType, netcodeIP, tcpIP, tcpPort);

            _appSettingsUI.Hide();

            if (isNeedsRestart)
                Application.Quit();
            // TODO make restart
        }
    }

    private async UniTask<bool> AwaitForNotificationResult()
    {
        UniTaskCompletionSource completionSource = new();
        bool notificationResult = false;

        _fullscreenNotificationUI.Confirmed += OnConfirm;
        _fullscreenNotificationUI.Canceled += OnCancel;

        await completionSource.Task;

        _fullscreenNotificationUI.Confirmed -= OnConfirm;
        _fullscreenNotificationUI.Canceled -= OnCancel;

        return notificationResult;

        void OnConfirm()
        {
            notificationResult = true;
            completionSource.TrySetResult();

            _fullscreenNotificationUI.Hide();
        }

        void OnCancel()
        {
            notificationResult = false;
            completionSource.TrySetResult();

            _fullscreenNotificationUI.Hide();
        }
    }

    #endregion

    public void Dispose()
    {
        _appSettingsUI.DataChanged -= AppSettingsUIDataChanged;
        _appSettingsUI.CancelButtonClicked -= AppSettingsUI_OnCancelButtonClicked;
    }
}