#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

public enum ZoneAvailabilityType
{
    Available,
    AvailableWithRestrictions,
    NotAvailable
}

public class CustomDisplayingZones
{
    public List<OperationTerritoryType> DisplayingZoneTypes = new();
    public ZoneAvailabilityType ZoneAvailabilityType;
}

[Serializable]
public class BaseDisplayingZones
{
    public OperationTerritoryType ZoneType;
    public Texture ZoneRenderTexture;
}

[Serializable]
public class CustomDisplayingZoneColors
{
    public ZoneAvailabilityType ZoneAvailabilityType;
    public Color ZoneColor;
}

[Serializable]
public class CustomDisplayingZoneHints
{
    public ZoneAvailabilityType ZoneAvailabilityType;
    public Transform ZoneAvailabilityHint;
}