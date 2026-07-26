public interface ILoginDataProvider
{
    public ReadonlyLoginedUser LoginedUser { get; }
    public int MaxUserLoginIndex { get; }
    public int MinUserLoginIndex { get; }
}