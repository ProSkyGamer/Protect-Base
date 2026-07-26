/*#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

public class GameInput : MonoBehaviour
{
    public static GameInput Instance { get; private set; }

    #region Enums

    public enum InputBindings
    {
        CameraRotationButton,
        InterfaceUpButton,
        InterfaceDownButton,
        InterfaceLeftButton,
        InterfaceRightButton,
        InteractButton,
        InterfaceBackButton,
        NextFiringMachineButton,
        PreviousFiringMachineButton,
        FiringMachinePowerButton,
        ZoomInFiringMachineButton,
        ZoomOutFiringMachineButton,
        PreSettingStartInputButton,
        PreSettingSaveButton,
        Input1Button,
        Input2Button,
        Input3Button,
        Input4Button,
        Input5Button,
        Input6Button,
        Input7Button,
        Input8Button,
        Input9Button,
        Input0Button,
        FiringMachineChooseMainFiringBlockButton,
        FiringMachineChooseFirstExplosiveBlockButton,
        FiringMachineChooseSecondExplosiveBlock,
        FiringMachineSingleShootingTypeButton,
        FiringMachineMultiShootingTypeButton,
        FiringMachineShootButton,
        FiringMachineReloadButton,
        FiringMachineFiringModeToggleButton,
        FiringMachineWarningShotButton,
        FiringMachineRangeRightButton,
        FiringMachineRangeLeftButton,
        FiringMachineRangeUpButton,
        FiringMachineRangeDownButton,
        FiringMachineRangeDownDoubleButton,
        FiringMachineRangeUpDoubleButton,
        FiringMachineProjectorToggleButton,
        FiringMachineInfraredToggleButton,
        InputBackspaceButton,
        InputMinusButton,
        InputClearButton,
        InputLockButton,
        FiringMachineFocusPlusButton,
        FiringMachineFocusMinusButton,
        FiringMachineMicrophoneButton,
        InputResetButton,
        FiringMachineFirstFiringStageToggleKeyButton,
        TEMP_ChangeSkybox, //TODO delete
        OperationManagerInterfaceButton,
        DYMNetworkToggleButton,
        DevLogsButton,
        DevSettingsButton,
        DevAdditionalButton1,
        DevAdditionalButton2,
        DevShowLogsButton,
        DevShowSettingsButton
    }

    #endregion

    #region Created Classes

    [Serializable]
    private class InputCombinations
    {
        public List<InputBindings> allRequiredPressedBindings;
        public InputBindings combinedBinding;
    }

    #endregion

    #region Events & Event Args

    public event EventHandler<OnInputBindingPressedEventArgs> OnInputBindingPressed;

    public class OnInputBindingPressedEventArgs : EventArgs
    {
        public InputBindings pressedInputBinding;
    }

    #endregion

    #region Variables

    private GameInputsAM inputActions;

    [SerializeField] private List<InputCombinations> allInputCombinations;
    private readonly Dictionary<InputBindings, bool> holdingInputBindings = new();
    private bool isKeypadLocked;
    private bool isInteractionUnlocked;
    private Vector2 currentSavedFiringMachineRotationDelta;

    #endregion

    #region Initialization

    private void Awake()
    {
        if (Instance != null)
            Destroy(gameObject);
        else
            Instance = this;

        inputActions = new GameInputsAM();
        inputActions.Enable();
    }

    #endregion

    #region Imitate

    public void ImitateButtonClick(InputBindings inputBinding)
    {
        inputBinding = TryGetAvailableBinding(inputBinding);

        if (inputBinding == InputBindings.InputLockButton) isKeypadLocked = !isKeypadLocked;

        // TODO combo
        if (!IsBindingAvailableToPress(inputBinding)) return;

        OnInputBindingPressed?.Invoke(this, new OnInputBindingPressedEventArgs
        {
            pressedInputBinding = inputBinding
        });
    }

    public void UnlockInteraction()
    {
        isInteractionUnlocked = true;
    }

    public void LockInteraction()
    {
        isInteractionUnlocked = false;
    }

    public void StartButtonHold(InputBindings holdingBinding)
    {
        if (!holdingInputBindings.ContainsKey(holdingBinding))
            holdingInputBindings.Add(holdingBinding, true);
        else
            holdingInputBindings[holdingBinding] = true;

        Debug.Log($"Start hold {holdingBinding}");

        OnInputBindingPressed?.Invoke(this, new OnInputBindingPressedEventArgs
        {
            pressedInputBinding = holdingBinding
        });
    }

    public void StopButtonHold(InputBindings holdingBinding)
    {
        if (!holdingInputBindings.ContainsKey(holdingBinding))
            holdingInputBindings.Add(holdingBinding, false);
        else
            holdingInputBindings[holdingBinding] = false;

        Debug.Log($"Stop hold {holdingBinding}");
    }

    public void SetCurrentFiringMachineRotationDelta(Vector2 newNormalizedRotation)
    {
        currentSavedFiringMachineRotationDelta = newNormalizedRotation;
    }

    #endregion

    #region Get

    private InputBindings TryGetAvailableBinding(InputBindings checkingBinding)
    {
        switch (checkingBinding)
        {
            default:
                return checkingBinding;
            case InputBindings.Input2Button:
                return isInteractionUnlocked
                    ? checkingBinding
                    : CurrentSelectedUIItemController.Instance.IsCurrentlyInteracting()
                        ? checkingBinding
                        : InputBindings.InterfaceUpButton;
            case InputBindings.Input4Button:
                return isInteractionUnlocked
                    ? checkingBinding
                    : CurrentSelectedUIItemController.Instance.IsCurrentlyInteracting()
                        ? checkingBinding
                        : InputBindings.InterfaceLeftButton;
            case InputBindings.Input6Button:
                return isInteractionUnlocked
                    ? checkingBinding
                    : CurrentSelectedUIItemController.Instance.IsCurrentlyInteracting()
                        ? checkingBinding
                        : InputBindings.InterfaceRightButton;
            case InputBindings.Input8Button:
                return isInteractionUnlocked
                    ? checkingBinding
                    : CurrentSelectedUIItemController.Instance.IsCurrentlyInteracting()
                        ? checkingBinding
                        : InputBindings.InterfaceDownButton;
        }
    }

    private bool IsBindingAvailableToPress(InputBindings checkingBinding)
    {
        if (!isKeypadLocked)
            return true;

        switch (checkingBinding)
        {
            default:
                return true;
            case InputBindings.Input0Button:
            case InputBindings.Input1Button:
            case InputBindings.Input2Button:
            case InputBindings.Input3Button:
            case InputBindings.Input4Button:
            case InputBindings.Input5Button:
            case InputBindings.Input6Button:
            case InputBindings.Input7Button:
            case InputBindings.Input8Button:
            case InputBindings.Input9Button:
            case InputBindings.PreSettingStartInputButton:
            case InputBindings.PreSettingSaveButton:
            case InputBindings.InputResetButton:
            case InputBindings.InputBackspaceButton:
                return !isKeypadLocked;
        }
    }

    public Vector2 GetMousePositionDelta()
    {
        //var mousePositionDelta = inputActions.FiringMachine.MouseDelta.ReadValue<Vector2>();

        //return mousePositionDelta;
        return Vector2.zero;
    }

    public Vector2 GetMousePosition()
    {
        //var mousePosition = inputActions.FiringMachine.MousePosition.ReadValue<Vector2>();

        //return mousePosition;
        return Vector2.zero;
    }

    public bool IsBindingPressed(InputBindings inputBinding)
    {
        if (holdingInputBindings.ContainsKey(inputBinding))
            if (holdingInputBindings[inputBinding])
                return true;


        return false;
    }

    public Vector2 GetCurrentSavedFiringMachineRotationDelta()
    {
        return currentSavedFiringMachineRotationDelta;
    }

    #endregion

    private void OnDestroy()
    {
        inputActions.Dispose();

        OnInputBindingPressed = null;
    }
}*/

