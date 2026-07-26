#region

using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Zenject;

#endregion

public class CameraSystemSingleObserver : IInitializable, IDisposable, ISceneResettable
{
    #region Variables & References

    private readonly float _activeTimePeriod = 5f;

    private readonly CameraSystemSingleUI _cameraSystemSingleUI;
    private readonly CameraSystemSingle _followingCameraSystemSingle;

    private CancellationTokenSource _leftCameraTurnOffCancellationToken = new();
    private CancellationTokenSource _rightCameraTurnOffCancellationToken = new();

    #endregion

    #region Initialization

    public CameraSystemSingleObserver(CameraSystemSingleUI cameraSystemSingleUI, CameraSystemSingle cameraSystemSingle)
    {
        _cameraSystemSingleUI = cameraSystemSingleUI;
        _followingCameraSystemSingle = cameraSystemSingle;
    }

    public void Initialize()
    {
        _followingCameraSystemSingle.Triggered += FollowingCameraSystemSingleTriggered;
        _followingCameraSystemSingle.TriggerEnded += FollowingCameraSystemSingleTriggerEnded;

        _cameraSystemSingleUI.SwitchLeftCameraState(false);
        _cameraSystemSingleUI.SwitchRightCameraState(false);
    }

    private void FollowingCameraSystemSingleTriggerEnded(bool isLeftHalf)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.CameraSystem)
            return;

        if (isLeftHalf)
        {
            _leftCameraTurnOffCancellationToken.Cancel();
            _leftCameraTurnOffCancellationToken = new();

            TurnOffCameraAfterDelay(() => _cameraSystemSingleUI.SwitchLeftCameraState(false), _activeTimePeriod,
                _leftCameraTurnOffCancellationToken.Token).Forget();
        }
        else
        {
            _rightCameraTurnOffCancellationToken.Cancel();
            _rightCameraTurnOffCancellationToken = new();

            TurnOffCameraAfterDelay(() => _cameraSystemSingleUI.SwitchRightCameraState(false), _activeTimePeriod,
                _rightCameraTurnOffCancellationToken.Token).Forget();
        }
    }

    private void FollowingCameraSystemSingleTriggered(bool isLeftHalf)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.CameraSystem)
            return;

        if (isLeftHalf)
        {
            _leftCameraTurnOffCancellationToken.Cancel();
            _leftCameraTurnOffCancellationToken = new();

            _cameraSystemSingleUI.SwitchLeftCameraState(true);
        }
        else
        {
            _rightCameraTurnOffCancellationToken.Cancel();
            _rightCameraTurnOffCancellationToken = new();

            _cameraSystemSingleUI.SwitchRightCameraState(true);
        }
    }

    private async UniTaskVoid TurnOffCameraAfterDelay(Action turnOffAction, float waitingTime,
        CancellationToken cancellationToken)
    {
        await UniTask.WaitForSeconds(waitingTime, cancellationToken: cancellationToken);

        turnOffAction();
    }

    #endregion

    public void Dispose()
    {
        _followingCameraSystemSingle.Triggered -= FollowingCameraSystemSingleTriggered;
        _followingCameraSystemSingle.TriggerEnded -= FollowingCameraSystemSingleTriggerEnded;

        _leftCameraTurnOffCancellationToken.Cancel();
        _rightCameraTurnOffCancellationToken.Cancel();
    }

    public void OnSceneReset()
    {
        _cameraSystemSingleUI.SwitchLeftCameraState(false);
        _cameraSystemSingleUI.SwitchRightCameraState(false);
    }
}