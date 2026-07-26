public interface ISelectedUIItem
{
    bool IsCurrentlyInteracting { get; }
    BaseSelectedItemSingleUI CurrentSelectedItem { get; }
}