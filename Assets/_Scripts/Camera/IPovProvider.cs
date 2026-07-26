#region

using System;
using UnityEngine;

#endregion

public interface IPovProvider
{
    public event Action PovAnglesChanged;

    public event Action PoVFocusChanged;

    public event Action PoVZoomChanged;

    public event Action PovStatusChanged;

    public bool CurrentPoVStatus { get; }

    public Vector3 CurrentPovEulerAngles { get; }

    public Vector3 CurrentPovCameraPosition { get; }

    public int CurrentPovFocusLevel { get; }

    public int CurrentPovZoomLevel { get; }

    public float CurrentPovZoomValue { get; }
}