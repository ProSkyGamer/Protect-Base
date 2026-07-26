#region

using System;
using UnityEngine;
using Zenject;

#endregion

public class CameraSystemSingle : MonoBehaviour, IInitializable
{
    public event Action<bool> Triggered;

    public event Action<bool> TriggerEnded;

    private Camera _cameraSingle;
    public RenderTexture CameraRenderTexture => _cameraSingle.targetTexture;

    public void Initialize()
    {
        _cameraSingle = GetComponent<Camera>();

        if (ClientTypeManager.CurrentClientType is not ClientType.CameraSystem)
            _cameraSingle.enabled = false;
    }

    public void TriggerCamera(bool isLeftHalf)
    {
        Triggered?.Invoke(isLeftHalf);
    }

    public void EndCameraTrigger(bool isLeftHalf)
    {
        TriggerEnded?.Invoke(isLeftHalf);
    }
}