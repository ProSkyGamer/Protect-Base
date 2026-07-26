#region

using System;

#endregion

public interface IPoVSwapper
{
    public event Action<IPovProvider> ChangePoV;

    public event Action<bool> ChangeInfraredState;

    public event Action<bool> ChangeProjectorState;

    public bool IsInfraredEnabled { get; }

    public bool IsProjectorEnabled { get; }
}