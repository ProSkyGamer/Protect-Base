#region

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

#endregion

public class FiringMachinesPageUI : MonoBehaviour, IUIPage
{
    #region Events

    public event Action<FiringMachinesPageType> InterfaceShown;

    public event Action HideRequested;

    public event Action InterfaceHidden;

    #endregion

    #region Variables & References

    private MainFiringMachinesInterfaceUI _firingMachinesInterfaceUI;

    [SerializeField] private string _modePageText;
    [SerializeField] private FiringMachinesPageType _firingMachinesPageType;
    private SelectedUIItemController _selectedUIItemController;

    public FiringMachinesPageType PageType => _firingMachinesPageType;

    public bool IsCanHide => true;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(MainFiringMachinesInterfaceUI firingMachinesInterface)
    {
        _firingMachinesInterfaceUI = firingMachinesInterface;
    }

    #endregion

    #region Visuals

    public void Show()
    {
        _firingMachinesInterfaceUI.Show();
        _firingMachinesInterfaceUI.ChangeModeText(_modePageText);

        InterfaceShown?.Invoke(_firingMachinesPageType);
    }

    public void UpdateVisuals()
    {
        _firingMachinesInterfaceUI.UpdateVisual().Forget();
    }

    public void UpdateCrosshair(bool isInfraredEnabled)
    {
        _firingMachinesInterfaceUI.UpdateCrosshair(isInfraredEnabled);
    }

    public void UpdateView()
    {
        _firingMachinesInterfaceUI.UpdateView();
    }

    public void UpdateTab(DutyModeTabType tabType)
    {
        _firingMachinesInterfaceUI.UpdateTab(tabType);
    }

    public void UpdateAlarms(IReadOnlyList<AlarmSingle> activeAlarms)
    {
        _firingMachinesInterfaceUI.UpdateAlarms(activeAlarms);
    }

    public void RequestHide()
    {
        HideRequested?.Invoke();
    }

    public void Hide()
    {
        _firingMachinesInterfaceUI.Hide();

        InterfaceHidden?.Invoke();
    }

    #endregion
}