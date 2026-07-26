#region

using System;
using UnityEngine;

#endregion

public enum MarkerType
{
    FiringMachineMarker,
    EnemyMarker
}

public enum MarkerAdditionalInfoType
{
    None,
    FiringMachine,
    Enemy
}

[Serializable]
public class MapMarkerPrefab
{
    public MarkerType MarkerType;
    public MarkerSingleInstaller MarkerSingleInstaller;
}

[Serializable]
public class MarkersAdditionalInfoUI
{
    public MarkerAdditionalInfo MarkerAdditionalInfo;
    public MarkerAdditionalInfoType MarkerAdditionalInfoType;
    public Vector2 MarkerAdditionalInfoOffset = new(20f, 20f);
}