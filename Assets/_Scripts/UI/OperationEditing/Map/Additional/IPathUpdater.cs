#region

using System;
using System.Collections.Generic;

#endregion

public interface IPathUpdater
{
    public event Action<IReadOnlyList<ReadonlyPathPoint>> UpdatePathVisuals;
}