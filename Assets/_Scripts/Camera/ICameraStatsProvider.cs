#region

using System;

#endregion

public interface ICameraStatsProvider
{
    public event Action OnCameraAnglesChanged;

    public int CameraHorizontalAngle { get; }
    public int CameraVerticalAngle { get; }
    public int CurrentZoomLevel { get; }
    public float TargetDistance { get; }
}