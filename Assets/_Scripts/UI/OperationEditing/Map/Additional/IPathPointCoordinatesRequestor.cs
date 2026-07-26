#region

using System;

#endregion

public interface IPathPointCoordinatesRequestor
{
    public event Action<MapListeningPointType, MarkerType> RequestPathPoint;
    public event Action CancelPathPointRequest;
}