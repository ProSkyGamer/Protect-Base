#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

#endregion

public class KeyboardDevInput : IDevInput, IInitializable, IDisposable
{
    private readonly GameInputsAM _gameInput;
    private IDevInput _devInputImplementation;
    private CancellationTokenSource _cancellationTokenSource = new();
    private bool _isListeningForMouseInput;

    public KeyboardDevInput(GameInputsAM gameInput)
    {
        _gameInput = gameInput;
    }

    #region Events

    public event Action LogsToggle;

    public event Action SettingsShow;

    public event Action OperationManagerToggle;

    public event Action ChangeSkybox;

    public event Action MouseClick;

    public event Action CloseInterface;

    public event Action<Vector2> MousePositionChanged;

    #endregion

    public void Initialize()
    {
        _gameInput.DevInput.LogsButton.performed += OnLogsButtonPerformed;
        _gameInput.DevInput.SettingsButton.performed += OnSettingsButtonPerformed;
        _gameInput.DevInput.OperationManagerButton.performed += OnOperationManagerButtonPerformed;
        _gameInput.DevInput.ChangeSkybox.performed += OnChangeSkyboxPerformed;
        _gameInput.DevInput.MouseClick.performed += MouseClick_Performed;
        _gameInput.DevInput.CloseInterface.performed += CloseInterface_Performed;
    }

    private void OnLogsButtonPerformed(InputAction.CallbackContext _)
    {
        if (_gameInput.DevInput.AdditionalButton.IsPressed() == false)
            return;

        LogsToggle?.Invoke();
    }

    private void OnSettingsButtonPerformed(InputAction.CallbackContext _)
    {
        if (_gameInput.DevInput.AdditionalButton.IsPressed() == false)
            return;

        SettingsShow?.Invoke();
    }

    private void OnOperationManagerButtonPerformed(InputAction.CallbackContext _)
    {
        OperationManagerToggle?.Invoke();
    }

    private void OnChangeSkyboxPerformed(InputAction.CallbackContext _)
    {
        ChangeSkybox?.Invoke();
    }

    private void MouseClick_Performed(InputAction.CallbackContext _)
    {
        MouseClick?.Invoke();
    }

    private void CloseInterface_Performed(InputAction.CallbackContext _)
    {
        CloseInterface?.Invoke();
    }

    public void StartListeningForMousePosition()
    {
        if (_isListeningForMouseInput)
            return;

        ListenForMousePositionChangeAsync(_cancellationTokenSource.Token).Forget();
    }

    private async UniTaskVoid ListenForMousePositionChangeAsync(CancellationToken cancellationToken)
    {
        Vector2 previousMousePosition = _gameInput.DevInput.MousePosition.ReadValue<Vector2>();

        _isListeningForMouseInput = true;

        while (true)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _isListeningForMouseInput = false;

                return;
            }

            Vector2 currentMousePosition = _gameInput.DevInput.MousePosition.ReadValue<Vector2>();

            if (currentMousePosition != previousMousePosition)
            {
                previousMousePosition = currentMousePosition;
                MousePositionChanged?.Invoke(currentMousePosition);
            }

            await UniTask.NextFrame();
        }
    }

    public void StopListeningForMousePosition()
    {
        _cancellationTokenSource.Cancel();
        _cancellationTokenSource = new();
    }

    public void Dispose()
    {
        _gameInput.DevInput.LogsButton.performed -= OnLogsButtonPerformed;
        _gameInput.DevInput.SettingsButton.performed -= OnSettingsButtonPerformed;
        _gameInput.DevInput.OperationManagerButton.performed -= OnOperationManagerButtonPerformed;
        _gameInput.DevInput.ChangeSkybox.performed -= OnChangeSkyboxPerformed;
        _gameInput.DevInput.MouseClick.performed -= MouseClick_Performed;
        _gameInput.DevInput.CloseInterface.performed -= CloseInterface_Performed;

        _cancellationTokenSource.Cancel();
    }
}