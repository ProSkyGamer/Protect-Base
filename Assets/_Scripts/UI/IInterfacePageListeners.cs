public interface IDevInterfaceListener
{
    void DevInterfaceActivated();
    void DevInterfaceDeactivated();
}

public interface IDutyInterfaceListener
{
    void DutyInterfaceActivated(FiringMachinesPageType pageType);
    void DutyInterfaceDeactivated();
}