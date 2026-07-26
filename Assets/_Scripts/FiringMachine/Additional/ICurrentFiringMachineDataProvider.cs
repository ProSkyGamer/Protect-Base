#region

using UnityEngine;

#endregion

public interface ICurrentFiringMachineDataProvider
{
    public int CurrentActive { get; }

    public bool IsAnySelected { get; }

    public bool IsSelectedActive { get; }

    public Vector3 CurrentEulerAngles { get; }

    public int CurrentZoomLevel { get; }

    public int TotalCount { get; }
}