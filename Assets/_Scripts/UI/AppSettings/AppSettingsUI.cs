#region

using System;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class AppSettingsUI : MonoBehaviour, IInitializable
{
    #region Events

    public event Action<bool> DataChanged;
    public event Action CancelButtonClicked;

    #endregion

    #region Variables & References

    [SerializeField] private SavingDropdown _clientTypeDropdown;
    [SerializeField] private SavingDropdown _windowTypeDropdown;
    [SerializeField] private SavingInputField _netcodeIPInputField;
    [SerializeField] private SavingInputField _tcpIPInputField;
    [SerializeField] private SavingInputField _tcpPortInputField;
    [SerializeField] private SavingDropdown _displayingScreenDropdown;
    [SerializeField] private Button _applyButton;
    [SerializeField] private Button _cancelButton;

    #endregion

    #region Initialization

    public void Initialize()
    {
        _applyButton.onClick.AddListener(OnApplyButtonClick);
        _cancelButton.onClick.AddListener(OnCancelButtonClick);

        _clientTypeDropdown.SetDropdownValues(Enum.GetNames(typeof(ClientType)).ToList());
        _windowTypeDropdown.SetDropdownValues(Enum.GetNames(typeof(WindowType)).ToList());
        // TODO screen choosing dropdown
    }

    public void SetValues(AppSettingsData appSettingsData)
    {
        _clientTypeDropdown.SetValue((int)appSettingsData.ClientType);
        _windowTypeDropdown.SetValue((int)appSettingsData.WindowType);
        _netcodeIPInputField.SetValue(appSettingsData.NetcodeIP);
        _tcpIPInputField.SetValue(appSettingsData.TCPIP);
        _tcpPortInputField.SetValue(appSettingsData.TCPPort.ToString());
    }

    #endregion

    #region On Click

    private void OnApplyButtonClick()
    {
        DataChanged?.Invoke(IsNeedsRestart());
    }

    private void OnCancelButtonClick()
    {
        CancelButtonClicked?.Invoke();
    }

    public void SaveValues()
    {
        _clientTypeDropdown.SaveLastValue();
        _windowTypeDropdown.SaveLastValue();
        _netcodeIPInputField.SaveLastValue();
        _tcpIPInputField.SaveLastValue();
        _tcpPortInputField.SaveLastValue();
        _displayingScreenDropdown.SaveLastValue();
    }

    public void ResetValues()
    {
        _clientTypeDropdown.ResetValue();
        _windowTypeDropdown.ResetValue();
        _netcodeIPInputField.ResetValue();
        _tcpIPInputField.ResetValue();
        _tcpPortInputField.ResetValue();
        _displayingScreenDropdown.ResetValue();
    }

    #endregion

    #region Visuals

    public void Show()
    {
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion

    #region Get

    public ClientType GetCurrentSelectedClientType()
    {
        return (ClientType)_clientTypeDropdown.GetCurrentValue();
    }

    public WindowType GetCurrentSelectedWindowType()
    {
        return (WindowType)_windowTypeDropdown.GetCurrentValue();
    }

    public string GetNetcodeIP()
    {
        return _netcodeIPInputField.GetCurrentValue();
    }

    public string GetTcpIP()
    {
        return _tcpIPInputField.GetCurrentValue();
    }

    public int GetTCPPort()
    {
        return int.Parse(_tcpPortInputField.GetCurrentValue());
    }

    private bool IsNeedsRestart()
    {
        bool isNeedsRestart = _clientTypeDropdown.IsValueChanged() || _netcodeIPInputField.IsValueChanged() ||
                              _tcpIPInputField.IsValueChanged() || _tcpPortInputField.IsValueChanged();

        return isNeedsRestart;
    }

    #endregion
}