#region

using System;

#endregion

public interface ICurrentEditingOperationDataProvider
{
    public event Action CurrentOperationUpdated;

    public ReadonlyOperationData GetCurrentEditingOperationSingle();
    public int GetTotalCurrentOperationWavesCount();
}