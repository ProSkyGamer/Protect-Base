#region

using System;

#endregion

public interface IOperationStatsDataProvider
{
    public event Action<OperationSavingStatType, object> DataChanged;
}