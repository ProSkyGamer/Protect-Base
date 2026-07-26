#region

using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class SettingsPageUI : BasePageUI, IInitializable
{
    #region Events

    public event Action OperatorPinsButtonPressed;

    public event Action DateTimeButtonPressed;

    public event Action PreSettingsButtonPressed;

    public event Action AlarmsButtonPressed;

    public event Action AmmoButtonPressed;

    #endregion

    #region Variables & References

    [SerializeField] private Button _operatorsPins;
    [SerializeField] private Button _dateTimeButton;
    [SerializeField] private Button _preSettingsButton;
    [SerializeField] private Button _alarmsButton;
    [SerializeField] private Button _ammunitionButton;
    [SerializeField] private Button _quitButton;

    #endregion

    #region Properties

    public override bool IsCanHide => true;

    #endregion

    #region Initialization

    public void Initialize()
    {
        SubscribeToUIEvents();
    }

    private void SubscribeToUIEvents()
    {
        _operatorsPins.onClick.AddListener(OnOperatorPinsButtonPressed);
        _dateTimeButton.onClick.AddListener(OnDateTimeButtonPressed);
        _preSettingsButton.onClick.AddListener(OnPreSettingsButtonPressed);
        _alarmsButton.onClick.AddListener(OnAlarmsButtonPressed);
        _ammunitionButton.onClick.AddListener(OnAmmunitionButtonPressed);
        _quitButton.onClick.AddListener(OnQuitButtonPressed);
    }

    private void OnOperatorPinsButtonPressed()
    {
        OperatorPinsButtonPressed?.Invoke();
    }

    private void OnDateTimeButtonPressed()
    {
        DateTimeButtonPressed?.Invoke();
    }

    private void OnPreSettingsButtonPressed()
    {
        PreSettingsButtonPressed?.Invoke();
    }

    private void OnAlarmsButtonPressed()
    {
        AlarmsButtonPressed?.Invoke();
    }

    private void OnAmmunitionButtonPressed()
    {
        AmmoButtonPressed?.Invoke();
    }

    private void OnQuitButtonPressed()
    {
        RequestHide();
    }

    #endregion
}