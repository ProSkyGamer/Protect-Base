#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class UIInputHandler : IInitializable, IDutyInterfaceListener, IDisposable
{
    private readonly List<IUIInput> _uiInputs = new();
    private readonly SelectedUIItemController _selectedUIItemController;
    private readonly IPreSettingSaver _preSettingsSaver;
    private readonly UIManager _uiManager;
    private bool _isInteractionLocked;
    private bool _isDutyModeShoving;

    private bool IsCanInteract => _isInteractionLocked == false;

    public UIInputHandler(List<IUIInput> uiInputs, SelectedUIItemController selectedUIItemController,
        IPreSettingSaver preSettingsSaver, UIManager uiManager)
    {
        _uiInputs.AddRange(uiInputs);
        _selectedUIItemController = selectedUIItemController;
        _preSettingsSaver = preSettingsSaver;
        _uiManager = uiManager;
    }

    public void DutyInterfaceActivated(FiringMachinesPageType pageType)
    {
        _isDutyModeShoving = true;
    }

    public void DutyInterfaceDeactivated()
    {
        _isDutyModeShoving = false;
    }

    public void Initialize()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game) return;

        foreach (IUIInput uiInput in _uiInputs)
        {
            uiInput.Up += UIInput_OnUp;
            uiInput.Down += UIInput_OnDown;
            uiInput.Left += UIInput_OnLeft;
            uiInput.Right += UIInput_OnRight;
            uiInput.Interact += UIInput_OnInteract;
            uiInput.Input0 += UIInput_OnInput0;
            uiInput.Input1 += UIInput_OnInput1;
            uiInput.Input2 += UIInput_OnInput2;
            uiInput.Input3 += UIInput_OnInput3;
            uiInput.Input4 += UIInput_OnInput4;
            uiInput.Input5 += UIInput_OnInput5;
            uiInput.Input6 += UIInput_OnInput6;
            uiInput.Input7 += UIInput_OnInput7;
            uiInput.Input8 += UIInput_OnInput8;
            uiInput.Input9 += UIInput_OnInput9;
            uiInput.Backspace += UIInput_OnBackspace;
            uiInput.Minus += UIInput_OnMinus;
            uiInput.Clear += UIInput_OnClear;
            uiInput.LockToggle += UIInput_OnLockToggle;
            uiInput.Reset += UIInput_OnReset;
            uiInput.PreSettingStartInput += UIInput_OnPreSettingStartInput;
            uiInput.PreSettingSaveButton += UIInput_OnPreSettingSaveButton;
        }
    }

    private void UIInput_OnUp()
    {
        if (IsCanInteract == false) return;

        _selectedUIItemController.InterfaceUp();
    }

    private void UIInput_OnDown()
    {
        if (IsCanInteract == false) return;

        _selectedUIItemController.InterfaceDown();
    }

    private void UIInput_OnLeft()
    {
        if (IsCanInteract == false) return;

        _selectedUIItemController.InterfaceLeft();
    }

    private void UIInput_OnRight()
    {
        if (IsCanInteract == false) return;

        _selectedUIItemController.InterfaceRight();
    }

    private void UIInput_OnInteract()
    {
        if (IsCanInteract == false) return;

        if (_preSettingsSaver.IsEnteringPreSetting)
            _preSettingsSaver.FinishEnteringPreSettingNumber();
        else
            _selectedUIItemController.InterfaceInteract();
    }

    private void UIInput_OnInput0()
    {
        if (IsCanInteract == false) return;

        ProcessKeypadNumbers('0');
    }

    private void UIInput_OnInput1()
    {
        if (IsCanInteract == false) return;

        ProcessKeypadNumbers('1');
    }

    private void UIInput_OnInput2()
    {
        if (IsCanInteract == false) return;

        if (_selectedUIItemController.IsCurrentlyInteracting)
            ProcessKeypadNumbers('2');
        else
            _selectedUIItemController.InterfaceUp();
    }

    private void UIInput_OnInput3()
    {
        if (IsCanInteract == false) return;

        ProcessKeypadNumbers('3');
    }

    private void UIInput_OnInput4()
    {
        if (IsCanInteract == false) return;

        if (_selectedUIItemController.IsCurrentlyInteracting)
            ProcessKeypadNumbers('4');
        else
            _selectedUIItemController.InterfaceLeft();
    }

    private void UIInput_OnInput5()
    {
        if (IsCanInteract == false) return;

        ProcessKeypadNumbers('5');
    }

    private void UIInput_OnInput6()
    {
        if (IsCanInteract == false) return;

        if (_selectedUIItemController.IsCurrentlyInteracting)
            ProcessKeypadNumbers('6');
        else
            _selectedUIItemController.InterfaceRight();
    }

    private void UIInput_OnInput7()
    {
        if (IsCanInteract == false) return;

        ProcessKeypadNumbers('7');
    }

    private void UIInput_OnInput8()
    {
        if (IsCanInteract == false) return;

        if (_selectedUIItemController.IsCurrentlyInteracting)
            ProcessKeypadNumbers('8');
        else
            _selectedUIItemController.InterfaceDown();
    }

    private void UIInput_OnInput9()
    {
        if (IsCanInteract == false) return;

        ProcessKeypadNumbers('9');
    }

    private void ProcessKeypadNumbers(char processingNumber)
    {
        string allowedNumbers = "0123456789";

        if (!allowedNumbers.Contains(processingNumber)) return;

        if (_preSettingsSaver.IsEnteringPreSetting || _isDutyModeShoving)
            _preSettingsSaver.ProcessPreSettingNumberInput(processingNumber);
        else
            _selectedUIItemController.InteractNumbers(processingNumber);
    }

    private void UIInput_OnBackspace()
    {
        if (IsCanInteract == false) return;

        _selectedUIItemController.InterfaceBackspace();
    }

    private void UIInput_OnMinus()
    {
        if (IsCanInteract == false) return;

        _selectedUIItemController.InteractNumbers('-');
    }

    private void UIInput_OnClear()
    {
        if (IsCanInteract == false) return;

        if (_selectedUIItemController.IsCurrentlyInteracting)
        {
            _selectedUIItemController.InterfaceClear();
        }
        else
        {
            Debug.Log("ever here?");
            _uiManager.HideCurrentInterface();
        }
    }

    private void UIInput_OnLockToggle()
    {
        _isInteractionLocked = !_isInteractionLocked;
    }

    private void UIInput_OnReset()
    {
        if (_preSettingsSaver.IsEnteringPreSetting)
            _preSettingsSaver.ResetInteraction();
        else if (_selectedUIItemController.IsCurrentlyInteracting == false) _uiManager.HideCurrentInterface();
    }

    private void UIInput_OnPreSettingStartInput()
    {
        _preSettingsSaver.StartEnteringPreSettingNumber();
    }

    private void UIInput_OnPreSettingSaveButton()
    {
        _preSettingsSaver.StartSavingPreSettingNumber();
    }

    public void Dispose()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game) return;

        foreach (IUIInput uiInput in _uiInputs)
        {
            uiInput.Up -= UIInput_OnUp;
            uiInput.Down -= UIInput_OnDown;
            uiInput.Left -= UIInput_OnLeft;
            uiInput.Right -= UIInput_OnRight;
            uiInput.Interact -= UIInput_OnInteract;
            uiInput.Input0 -= UIInput_OnInput0;
            uiInput.Input1 -= UIInput_OnInput1;
            uiInput.Input2 -= UIInput_OnInput2;
            uiInput.Input3 -= UIInput_OnInput3;
            uiInput.Input4 -= UIInput_OnInput4;
            uiInput.Input5 -= UIInput_OnInput5;
            uiInput.Input6 -= UIInput_OnInput6;
            uiInput.Input7 -= UIInput_OnInput7;
            uiInput.Input8 -= UIInput_OnInput8;
            uiInput.Input9 -= UIInput_OnInput9;
            uiInput.Backspace -= UIInput_OnBackspace;
            uiInput.Minus -= UIInput_OnMinus;
            uiInput.Clear -= UIInput_OnClear;
            uiInput.LockToggle -= UIInput_OnLockToggle;
            uiInput.Reset -= UIInput_OnReset;
            uiInput.PreSettingStartInput -= UIInput_OnPreSettingStartInput;
            uiInput.PreSettingSaveButton -= UIInput_OnPreSettingSaveButton;
        }
    }
}