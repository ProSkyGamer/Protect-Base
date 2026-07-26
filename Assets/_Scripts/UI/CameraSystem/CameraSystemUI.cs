#region

using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class CameraSystemUI : MonoBehaviour, IInitializable
{
    #region Variables & References

    private readonly List<CameraSystemSingle> _allCameraSystemSingle = new();
    private CameraSystemUISpawner _cameraSystemUISpawner;

    #endregion

    #region Iniialization

    [Inject]
    public void Construct(List<CameraSystemSingle> allCameraSystemSingles, CameraSystemUISpawner cameraSystemUISpawner)
    {
        _allCameraSystemSingle.AddRange(allCameraSystemSingles);
        _cameraSystemUISpawner = cameraSystemUISpawner;
    }

    public void Initialize()
    {
        foreach (CameraSystemSingle cameraSystemSingle in _allCameraSystemSingle)
        {
            CameraSystemSingleUI newRenderingCameraSingle = _cameraSystemUISpawner.Create(cameraSystemSingle);
        }

        if (ClientTypeManager.CurrentClientType is not ClientType.CameraSystem)
            Hide();
    }

    #endregion

    #region Visuals

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion
}