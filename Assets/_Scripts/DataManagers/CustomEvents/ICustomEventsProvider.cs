#region

using System;
using System.Collections.Generic;

#endregion

public interface ICustomEventsProvider
{
    public event Action ListUpdated;

    public List<CustomEvent> GetAllEventsList();
}