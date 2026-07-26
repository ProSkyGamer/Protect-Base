#region

using System;
using Zenject;

#endregion

public class AlarmsPageObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly AlarmsManager _alarmsManager;
    private readonly AlarmsPageUI _alarmsPageUI;

    #endregion

    #region Initialization

    public AlarmsPageObserver(AlarmsManager alarmsManager, AlarmsPageUI alarmsPageUI)
    {
        _alarmsManager = alarmsManager;
        _alarmsPageUI = alarmsPageUI;
    }

    public void Initialize()
    {
        _alarmsPageUI.AlarmAdded += AlarmsPageUI_OnAlarmAdded;
        _alarmsPageUI.RemovedAllAlarms += AlarmsPageUI_OnRemovedAllAlarms;
        _alarmsPageUI.AnyInputFieldValueChanged += _alarmsPageUI_OnAnyInputFieldValueChanged;

        _alarmsPageUI.PageShown += AlarmsPageUI_OnPageShown;
    }

    private void AlarmsPageUI_OnPageShown()
    {
        _alarmsPageUI.UpdateVisual();
    }

    private void AlarmsPageUI_OnAlarmAdded(int deviceNumber, int alarmNumber, int firingMachineNumber, int preSettingNumber,
        FiringMachineEnableType firingMachineEnableType, AlarmType alarmType)
    {
        _alarmsManager.AddAlarmSingle(deviceNumber, alarmNumber, firingMachineNumber, preSettingNumber, firingMachineEnableType, alarmType);

        _alarmsPageUI.UpdateVisual();
    }

    private void AlarmsPageUI_OnRemovedAllAlarms()
    {
        _alarmsManager.RemoveAllAlarms();

        _alarmsPageUI.UpdateVisual();
    }

    private void _alarmsPageUI_OnAnyInputFieldValueChanged()
    {
        _alarmsPageUI.UpdateVisual();
    }

    #endregion

    public void Dispose()
    {
        _alarmsPageUI.AlarmAdded -= AlarmsPageUI_OnAlarmAdded;
        _alarmsPageUI.RemovedAllAlarms -= AlarmsPageUI_OnRemovedAllAlarms;
        _alarmsPageUI.AnyInputFieldValueChanged -= _alarmsPageUI_OnAnyInputFieldValueChanged;

        _alarmsPageUI.PageShown -= AlarmsPageUI_OnPageShown;
    }
}