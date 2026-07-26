#region

using System;
using UnityEngine;

#endregion

public enum PathPointType
{
    SpawnPathPoint,
    DefaultPathPoint,
    SoldiersDisembarkPathPoint,
    DisembarkedSoldiersPathPoint,
    FinalDestinationPoint,
    FinalDroneDestinationPoint,
    SelectedPathPoint
}

public enum MapListeningPointType
{
    MapPoint,
    MarkerPoint
}

[Serializable]
public class MapListeningTypeHints
{
    public MapListeningPointType MapListeningPointType;
    public Transform MapListeningTypeHintTransform;
}