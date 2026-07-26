public interface IOperationsStatusListener
{
    public void OperationStarted();

    public void OperationEnded();
}

public interface IOperationUpdateListener
{
    public void UpdateOperationsVisuals();
}