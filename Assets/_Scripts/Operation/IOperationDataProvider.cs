public interface IOperationDataProvider
{
    public float MaxOperationLength { get; }

    public float CurrentOperationTime { get; }

    public bool IsOperationActive { get; }

    public int TotalWavesCount { get; }

    public int SpawnedWavesCount { get; }

    public int CurrentlyAliveEnemies { get; }
}