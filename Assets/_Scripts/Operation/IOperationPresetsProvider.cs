#region

using System.Collections.Generic;
using Cysharp.Threading.Tasks;

#endregion

public interface IOperationPresetsProvider
{
    public SavedOperationData GetOperationSingle(int operationIndex);
    public UniTask<List<SavedOperationData>> GetCurrentSavedOperationsAsync();
}