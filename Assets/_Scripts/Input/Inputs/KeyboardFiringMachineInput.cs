#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

#endregion

public class KeyboardFiringMachineInput : IFiringMachineInput, IInitializable, IFixedTickable, IDisposable
{
    private readonly GameInputsAM _gameInput;
    private readonly Vector2 _mouseDeltaCapPerFrame = new(10f, 10f);

    private readonly float _continuousAdditionalDistanceActivationTime = 1f;
    private readonly float _continuousAdditionalDistanceTime = .5f;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    public KeyboardFiringMachineInput(GameInputsAM gameInput)
    {
        _gameInput = gameInput;
    }

    #region Events

    public event Action HideCursor;

    public event Action ShowCursor;

    public event Action<Vector2> Rotation;

    public event Action PowerToggle;

    public event Action ChooseMainFiringBlock;

    public event Action ChooseFirstExplosiveBlock;

    public event Action ChooseSecondExplosiveBlock;

    public event Action FiringModeToggle;

    public event Action Shoot;

    public event Action Reload;

    public event Action WarningShot;

    public event Action RangeRight;

    public event Action RangeLeft;

    public event Action RangeUp;

    public event Action RangeDown;

    public event Action RangeUpDouble;

    public event Action RangeDownDouble;

    public event Action ZoomIn;

    public event Action ZoomOut;

    public event Action FocusPlus;

    public event Action FocusMinus;

    #endregion

    private bool _isShowingCursor = true;

    public void Initialize()
    {
        _gameInput.FiringMachineInput.PowerToggleButton.performed += OnPowerToggleButtonPerformed;

        _gameInput.FiringMachineInput.ZoomInButton.performed += OnZoomInButtonPerformed;
        _gameInput.FiringMachineInput.ZoomOutButton.performed += OnZoomOutButtonPerformed;

        _gameInput.FiringMachineInput.ChooseMainFiringBlockButton.performed += OnChooseMainFiringBlockButtonPerformed;

        _gameInput.FiringMachineInput.ChooseFirstExplosiveBlockButton.performed +=
            OnChooseFirstExplosiveBlockButtonPerformed;

        _gameInput.FiringMachineInput.ChooseSecondExplosiveBlockButton.performed +=
            OnChooseSecondExplosiveBlockButtonPerformed;

        _gameInput.FiringMachineInput.FiringModeToggleButton.performed += OnFiringModeToggleButtonPerformed;
        _gameInput.FiringMachineInput.ShootButton.performed += OnShootButtonPerformed;
        _gameInput.FiringMachineInput.WarningShotButton.performed += OnWarningShotButtonPerformed;
        _gameInput.FiringMachineInput.ReloadButton.performed += OnReloadButtonPerformed;

        _gameInput.FiringMachineInput.RangeRightButton.performed += OnRangeRightButtonPerformed;
        _gameInput.FiringMachineInput.RangeLeftButton.performed += OnRangeLeftButtonPerformed;
        _gameInput.FiringMachineInput.RangeUpButton.performed += OnRangeUpButtonPerformed;
        _gameInput.FiringMachineInput.RangeDownButton.performed += OnRangeDownButtonPerformed;
        _gameInput.FiringMachineInput.RangeUpDoubleButton.performed += OnRangeUpDoubleButtonPerformed;
        _gameInput.FiringMachineInput.RangeDownDoubleButton.performed += OnRangeDownDoubleButtonPerformed;

        _gameInput.FiringMachineInput.FocusPlusButton.performed += OnFocusPlusButtonPerformed;
        _gameInput.FiringMachineInput.FocusMinusButton.performed += OnFocusMinusButtonPerformed;
    }

    private void OnPowerToggleButtonPerformed(InputAction.CallbackContext _)
    {
        PowerToggle?.Invoke();
    }

    private void OnZoomInButtonPerformed(InputAction.CallbackContext _)
    {
        ZoomIn?.Invoke();
    }

    private void OnZoomOutButtonPerformed(InputAction.CallbackContext _)
    {
        ZoomOut?.Invoke();
    }

    private void OnChooseMainFiringBlockButtonPerformed(InputAction.CallbackContext _)
    {
        ChooseMainFiringBlock?.Invoke();
    }

    private void OnChooseFirstExplosiveBlockButtonPerformed(InputAction.CallbackContext _)
    {
        ChooseFirstExplosiveBlock?.Invoke();
    }

    private void OnChooseSecondExplosiveBlockButtonPerformed(InputAction.CallbackContext _)
    {
        ChooseSecondExplosiveBlock?.Invoke();
    }

    private void OnFiringModeToggleButtonPerformed(InputAction.CallbackContext _)
    {
        FiringModeToggle?.Invoke();
    }

    private void OnShootButtonPerformed(InputAction.CallbackContext _)
    {
        Shoot?.Invoke();
    }

    private void OnWarningShotButtonPerformed(InputAction.CallbackContext _)
    {
        WarningShot?.Invoke();
    }

    private void OnReloadButtonPerformed(InputAction.CallbackContext _)
    {
        Reload?.Invoke();
    }

    private void OnRangeRightButtonPerformed(InputAction.CallbackContext _)
    {
        RangeRight?.Invoke();

        StartContinuousButtonHold(_gameInput.FiringMachineInput.RangeRightButton, () => RangeRight?.Invoke());
    }

    private void OnRangeLeftButtonPerformed(InputAction.CallbackContext _)
    {
        RangeLeft?.Invoke();

        StartContinuousButtonHold(_gameInput.FiringMachineInput.RangeLeftButton, () => RangeLeft?.Invoke());
    }

    private void OnRangeUpButtonPerformed(InputAction.CallbackContext _)
    {
        RangeUp?.Invoke();

        StartContinuousButtonHold(_gameInput.FiringMachineInput.RangeUpButton, () => RangeUp?.Invoke());
    }

    private void OnRangeDownButtonPerformed(InputAction.CallbackContext _)
    {
        RangeDown?.Invoke();

        StartContinuousButtonHold(_gameInput.FiringMachineInput.RangeDownButton, () => RangeDown?.Invoke());
    }

    private void StartContinuousButtonHold(InputAction holdingButton, Action onSuccessfulHoldAction)
    {
        float checkingForHoldIntervals = .1f;

        void OnSuccessfulButtonHold()
        {
            HoldButtonAsync(_cancellationTokenSource.Token, holdingButton, _continuousAdditionalDistanceTime,
                    onSuccessfulHoldAction)
                .Forget();
        }

        CheckButtonHoldAsync(holdingButton, _continuousAdditionalDistanceActivationTime, checkingForHoldIntervals,
            OnSuccessfulButtonHold, _cancellationTokenSource.Token).Forget();
    }

    private async UniTaskVoid CheckButtonHoldAsync(InputAction holdingInputAction,
        float totalHoldTime, float deltaTime, Action onSuccessAction, CancellationToken cancellationToken)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (!holdingInputAction.IsPressed())
                break;

            totalHoldTime -= deltaTime;

            if (totalHoldTime <= 0)
            {
                onSuccessAction();

                return;
            }

            await UniTask.WaitForSeconds(deltaTime, cancellationToken: cancellationToken);
        }
    }

    private async UniTaskVoid HoldButtonAsync(CancellationToken cancellationToken, InputAction holdingInputAction,
        float deltaTime, Action onSuccessAction)
    {
        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (!holdingInputAction.IsPressed()) return;

            onSuccessAction();

            await UniTask.WaitForSeconds(deltaTime, cancellationToken: cancellationToken);
        }
    }

    private void OnRangeUpDoubleButtonPerformed(InputAction.CallbackContext _)
    {
        RangeUpDouble?.Invoke();
    }

    private void OnRangeDownDoubleButtonPerformed(InputAction.CallbackContext _)
    {
        RangeDownDouble?.Invoke();
    }

    private void OnFocusPlusButtonPerformed(InputAction.CallbackContext _)
    {
        FocusPlus?.Invoke();
    }

    private void OnFocusMinusButtonPerformed(InputAction.CallbackContext _)
    {
        FocusMinus?.Invoke();
    }

    public void FixedTick()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game) return;

        if (_gameInput.FiringMachineInput.RotationButton.IsPressed())
        {
            if (_isShowingCursor)
            {
                _isShowingCursor = false;
                HideCursor?.Invoke();
            }

            Vector2 normalizedFiringMachineRotation = GetNormalizedFiringMachineRotation();

            Rotation?.Invoke(normalizedFiringMachineRotation);
        }
        else
        {
            if (!_isShowingCursor)
            {
                _isShowingCursor = true;
                ShowCursor?.Invoke();
            }
        }
    }

    private Vector2 GetNormalizedFiringMachineRotation()
    {
        Vector2 firingMachineRotationDelta = _gameInput.FiringMachineInput.MouseDelta.ReadValue<Vector2>();

        Vector2 firingMachineRotationDeltaNormalized = new(firingMachineRotationDelta.x > 0
                ? firingMachineRotationDelta.x > _mouseDeltaCapPerFrame.x
                    ? _mouseDeltaCapPerFrame.x
                    : firingMachineRotationDelta.x
                : firingMachineRotationDelta.x < -_mouseDeltaCapPerFrame.x
                    ? -_mouseDeltaCapPerFrame.x
                    : firingMachineRotationDelta.x,
            firingMachineRotationDelta.y > 0 ? firingMachineRotationDelta.y > _mouseDeltaCapPerFrame.y
                ? _mouseDeltaCapPerFrame.y
                : firingMachineRotationDelta.y :
            firingMachineRotationDelta.y < -_mouseDeltaCapPerFrame.y ? -_mouseDeltaCapPerFrame.y :
            firingMachineRotationDelta.y);

        firingMachineRotationDeltaNormalized = new Vector2(
            firingMachineRotationDeltaNormalized.x / _mouseDeltaCapPerFrame.x,
            firingMachineRotationDeltaNormalized.y / _mouseDeltaCapPerFrame.y);

        return firingMachineRotationDeltaNormalized;
    }

    public void Dispose()
    {
        _gameInput.FiringMachineInput.PowerToggleButton.performed -= OnPowerToggleButtonPerformed;

        _gameInput.FiringMachineInput.ZoomInButton.performed -= OnZoomInButtonPerformed;
        _gameInput.FiringMachineInput.ZoomOutButton.performed -= OnZoomOutButtonPerformed;

        _gameInput.FiringMachineInput.ChooseMainFiringBlockButton.performed -= OnChooseMainFiringBlockButtonPerformed;

        _gameInput.FiringMachineInput.ChooseFirstExplosiveBlockButton.performed -=
            OnChooseFirstExplosiveBlockButtonPerformed;

        _gameInput.FiringMachineInput.ChooseSecondExplosiveBlockButton.performed -=
            OnChooseSecondExplosiveBlockButtonPerformed;

        _gameInput.FiringMachineInput.FiringModeToggleButton.performed -= OnFiringModeToggleButtonPerformed;
        _gameInput.FiringMachineInput.ShootButton.performed -= OnShootButtonPerformed;
        _gameInput.FiringMachineInput.WarningShotButton.performed -= OnWarningShotButtonPerformed;
        _gameInput.FiringMachineInput.ReloadButton.performed -= OnReloadButtonPerformed;

        _gameInput.FiringMachineInput.RangeRightButton.performed -= OnRangeRightButtonPerformed;
        _gameInput.FiringMachineInput.RangeLeftButton.performed -= OnRangeLeftButtonPerformed;
        _gameInput.FiringMachineInput.RangeUpButton.performed -= OnRangeUpButtonPerformed;
        _gameInput.FiringMachineInput.RangeDownButton.performed -= OnRangeDownButtonPerformed;
        _gameInput.FiringMachineInput.RangeUpDoubleButton.performed -= OnRangeUpDoubleButtonPerformed;
        _gameInput.FiringMachineInput.RangeDownDoubleButton.performed -= OnRangeDownDoubleButtonPerformed;

        _gameInput.FiringMachineInput.FocusPlusButton.performed -= OnFocusPlusButtonPerformed;
        _gameInput.FiringMachineInput.FocusMinusButton.performed -= OnFocusMinusButtonPerformed;
    }
}