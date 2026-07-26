#region

using System;
using UnityEngine.InputSystem;
using Zenject;

#endregion

public class KeyboardUIInput : IUIInput, IInitializable, IDisposable
{
    private readonly GameInputsAM _gameInput;

    public KeyboardUIInput(GameInputsAM gameInput)
    {
        _gameInput = gameInput;
    }

    #region Events

    public event Action Up;

    public event Action Down;

    public event Action Left;

    public event Action Right;

    public event Action Interact;

    public event Action Input0;

    public event Action Input1;

    public event Action Input2;

    public event Action Input3;

    public event Action Input4;

    public event Action Input5;

    public event Action Input6;

    public event Action Input7;

    public event Action Input8;

    public event Action Input9;

    public event Action Backspace;

    public event Action PreSettingStartInput;

    public event Action PreSettingSaveButton;

    public event Action Minus;

    public event Action Clear;

    public event Action LockToggle;

    public event Action Reset;

    #endregion

    public void Initialize()
    {
        _gameInput.UIInput.Input0Button.performed += OnInput0ButtonPerformed;
        _gameInput.UIInput.Input1Button.performed += OnInput1ButtonPerformed;
        _gameInput.UIInput.Input2Button.performed += OnInput2ButtonPerformed;
        _gameInput.UIInput.Input3Button.performed += OnInput3ButtonPerformed;
        _gameInput.UIInput.Input4Button.performed += OnInput4ButtonPerformed;
        _gameInput.UIInput.Input5Button.performed += OnInput5ButtonPerformed;
        _gameInput.UIInput.Input6Button.performed += OnInput6ButtonPerformed;
        _gameInput.UIInput.Input7Button.performed += OnInput7ButtonPerformed;
        _gameInput.UIInput.Input8Button.performed += OnInput8ButtonPerformed;
        _gameInput.UIInput.Input9Button.performed += OnInput9ButtonPerformed;
        _gameInput.UIInput.PreSettingStartInputButton.performed += PreSettingStartInputButtonPerformed;
        _gameInput.UIInput.PreSettingSaveButton.performed += PreSettingSaveButtonPerformed;

        _gameInput.UIInput.UpButton.performed += OnUpButtonPerformed;
        _gameInput.UIInput.DownButton.performed += OnDownButtonPerformed;
        _gameInput.UIInput.LeftButton.performed += OnLeftButtonPerformed;
        _gameInput.UIInput.RightButton.performed += OnRightButtonPerformed;

        _gameInput.UIInput.InteractButton.performed += OnInteractButtonPerformed;
        _gameInput.UIInput.BackspaceButton.performed += OnBackspaceButtonPerformed;
        _gameInput.UIInput.MinusButton.performed += OnMinusButtonPerformed;
        _gameInput.UIInput.ClearButton.performed += OnClearButtonPerformed;
        _gameInput.UIInput.LockButton.performed += OnLockButtonPerformed;
        _gameInput.UIInput.ResetButton.performed += OnResetButtonPerformed;
        _gameInput.UIInput.ExitButton.performed += OnExitButtonPerformed;
    }

    private void OnExitButtonPerformed(InputAction.CallbackContext obj)
    {
        Reset?.Invoke();
    }

    private void OnInput0ButtonPerformed(InputAction.CallbackContext _)
    {
        Input0?.Invoke();
    }

    private void OnInput1ButtonPerformed(InputAction.CallbackContext _)
    {
        Input1?.Invoke();
    }

    private void OnInput2ButtonPerformed(InputAction.CallbackContext _)
    {
        Input2?.Invoke();
    }

    private void OnInput3ButtonPerformed(InputAction.CallbackContext _)
    {
        Input3?.Invoke();
    }

    private void OnInput4ButtonPerformed(InputAction.CallbackContext _)
    {
        Input4?.Invoke();
    }

    private void OnInput5ButtonPerformed(InputAction.CallbackContext _)
    {
        Input5?.Invoke();
    }

    private void OnInput6ButtonPerformed(InputAction.CallbackContext _)
    {
        Input6?.Invoke();
    }

    private void OnInput7ButtonPerformed(InputAction.CallbackContext _)
    {
        Input7?.Invoke();
    }

    private void OnInput8ButtonPerformed(InputAction.CallbackContext _)
    {
        Input8?.Invoke();
    }

    private void OnInput9ButtonPerformed(InputAction.CallbackContext _)
    {
        Input9?.Invoke();
    }

    private void PreSettingStartInputButtonPerformed(InputAction.CallbackContext obj)
    {
        PreSettingStartInput?.Invoke();
    }

    private void PreSettingSaveButtonPerformed(InputAction.CallbackContext obj)
    {
        PreSettingSaveButton?.Invoke();
    }

    private void OnUpButtonPerformed(InputAction.CallbackContext _)
    {
        Up?.Invoke();
    }

    private void OnDownButtonPerformed(InputAction.CallbackContext _)
    {
        Down?.Invoke();
    }

    private void OnLeftButtonPerformed(InputAction.CallbackContext _)
    {
        Left?.Invoke();
    }

    private void OnRightButtonPerformed(InputAction.CallbackContext _)
    {
        Right?.Invoke();
    }

    private void OnInteractButtonPerformed(InputAction.CallbackContext _)
    {
        Interact?.Invoke();
    }

    private void OnBackspaceButtonPerformed(InputAction.CallbackContext _)
    {
        Backspace?.Invoke();
    }

    private void OnMinusButtonPerformed(InputAction.CallbackContext _)
    {
        Minus?.Invoke();
    }

    private void OnClearButtonPerformed(InputAction.CallbackContext _)
    {
        Clear?.Invoke();
    }

    private void OnLockButtonPerformed(InputAction.CallbackContext _)
    {
        LockToggle?.Invoke();
    }

    private void OnResetButtonPerformed(InputAction.CallbackContext _)
    {
        Reset?.Invoke();
    }

    public void Dispose()
    {
        _gameInput.UIInput.Input0Button.performed -= OnInput0ButtonPerformed;
        _gameInput.UIInput.Input1Button.performed -= OnInput1ButtonPerformed;
        _gameInput.UIInput.Input2Button.performed -= OnInput2ButtonPerformed;
        _gameInput.UIInput.Input3Button.performed -= OnInput3ButtonPerformed;
        _gameInput.UIInput.Input4Button.performed -= OnInput4ButtonPerformed;
        _gameInput.UIInput.Input5Button.performed -= OnInput5ButtonPerformed;
        _gameInput.UIInput.Input6Button.performed -= OnInput6ButtonPerformed;
        _gameInput.UIInput.Input7Button.performed -= OnInput7ButtonPerformed;
        _gameInput.UIInput.Input8Button.performed -= OnInput8ButtonPerformed;
        _gameInput.UIInput.Input9Button.performed -= OnInput9ButtonPerformed;

        _gameInput.UIInput.UpButton.performed -= OnUpButtonPerformed;
        _gameInput.UIInput.DownButton.performed -= OnDownButtonPerformed;
        _gameInput.UIInput.LeftButton.performed -= OnLeftButtonPerformed;
        _gameInput.UIInput.RightButton.performed -= OnRightButtonPerformed;

        _gameInput.UIInput.InteractButton.performed -= OnInteractButtonPerformed;
        _gameInput.UIInput.BackspaceButton.performed -= OnBackspaceButtonPerformed;
        _gameInput.UIInput.MinusButton.performed -= OnMinusButtonPerformed;
        _gameInput.UIInput.ClearButton.performed -= OnClearButtonPerformed;
        _gameInput.UIInput.LockButton.performed -= OnLockButtonPerformed;
        _gameInput.UIInput.ResetButton.performed -= OnResetButtonPerformed;
    }
}