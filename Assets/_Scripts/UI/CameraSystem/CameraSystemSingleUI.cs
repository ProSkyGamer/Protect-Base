#region

using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class CameraSystemSingleUI : MonoBehaviour, IInitializable
{
    #region Variables & References

    [SerializeField] private RawImage _leftCameraRawImage;
    [SerializeField] private RawImage _rightCameraRawImage;
    [SerializeField] private Transform _leftCameraSelectionTransform;
    [SerializeField] private Transform _rightCameraSelectionTransform;

    private CameraSystemSingle _followingCameraSystemSingle;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(CameraSystemSingle newFollowingCameraSystemSingle)
    {
        _followingCameraSystemSingle = newFollowingCameraSystemSingle;
    }

    public void Initialize()
    {
        _leftCameraRawImage.texture = _followingCameraSystemSingle.CameraRenderTexture;
        _rightCameraRawImage.texture = _followingCameraSystemSingle.CameraRenderTexture;
    }

    #endregion

    #region Turn Off

    public void SwitchLeftCameraState(bool newState)
    {
        _leftCameraSelectionTransform.gameObject.SetActive(newState);
    }

    public void SwitchRightCameraState(bool newState)
    {
        _rightCameraSelectionTransform.gameObject.SetActive(newState);
    }

    #endregion
}