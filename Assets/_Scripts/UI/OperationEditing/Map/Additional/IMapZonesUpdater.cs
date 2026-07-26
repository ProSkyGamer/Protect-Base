#region

using System;
using System.Collections.Generic;

#endregion

public interface IMapZonesUpdater
{
    public event Action<List<CustomDisplayingZones>> UpdateMapZones;
}