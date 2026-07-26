#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class PreSettingsManager : IPreSettingSaver, IPreSettingsProvider, IPreSettingTriggerer, IDutyInterfaceListener, IInitializable
{
    #region Events

    public event Action<int> PreSettingTriggered;

    #endregion

    #region Enums

    #endregion

    #region Variables & References

    private readonly int _specialNormalLightPreSettingNumber = 81;
    private readonly int _specialInfraredLightPreSettingNumber = 82;

    private bool _isCanAddPreSettings;
    private string _enteredPreSettingNumberString = "";
    private bool _isAddingPreSetting;

    private readonly List<SavedPreSetting> _allSavedPreSettings = new();

    private ICurrentFiringMachineDataProvider _currentFiringMachineDataProvider;
    private IDataSavingManager _dataSavingManager;

    public bool IsEnteringPreSetting { get; private set; }

    public int PreSettingMaxIndex { get; } = 80;

    public int PreSettingMinIndex => 1;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ICurrentFiringMachineDataProvider currentFiringMachineDataProvider, IDataSavingManager dataSavingManager)
    {
        _currentFiringMachineDataProvider = currentFiringMachineDataProvider;
        _dataSavingManager = dataSavingManager;
    }

    public void DutyInterfaceActivated(FiringMachinesPageType pageType)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        _isCanAddPreSettings = pageType == FiringMachinesPageType.PreSettingsMode;
    }

    public void DutyInterfaceDeactivated()
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        _isCanAddPreSettings = false;
    }

    public void Initialize()
    {
        UpdateCurrentSavedPreSettingsList();
    }

    private void UpdateCurrentSavedPreSettingsList()
    {
        _allSavedPreSettings.Clear();
        List<SavedPreSetting> allSavedPreSettings = _dataSavingManager.GetAllSavedPreSettings();

        if (allSavedPreSettings != null)
            _allSavedPreSettings.AddRange(allSavedPreSettings);
    }

    #endregion

    #region Interaction

    public void StartEnteringPreSettingNumber()
    {
        if (_currentFiringMachineDataProvider.IsAnySelected == false || _currentFiringMachineDataProvider.IsSelectedActive)
            return;

        Debug.Log("started input");
        IsEnteringPreSetting = true;
    }

    public void StartSavingPreSettingNumber()
    {
        if (_isCanAddPreSettings == false)
            return;

        if (IsEnteringPreSetting && _enteredPreSettingNumberString == "")
        {
            Debug.Log("started listening new");
            _isAddingPreSetting = true;
        }
    }

    public void ProcessPreSettingNumberInput(char addingNumber)
    {
        string allowedInput = "1234567890";

        if (IsEnteringPreSetting == false)
            if (allowedInput.Contains(addingNumber))
                if (int.TryParse(addingNumber.ToString(), out int triggeringPreSettingIndex) &&
                    triggeringPreSettingIndex != 0)
                    PreSettingTriggered?.Invoke(triggeringPreSettingIndex);

        if (IsEnteringPreSetting == false)
            return;

        if (allowedInput.Contains(addingNumber) == false)
            return;

        _enteredPreSettingNumberString += addingNumber;
    }

    public void FinishEnteringPreSettingNumber()
    {
        if (IsEnteringPreSetting == false)
            return;

        if (_enteredPreSettingNumberString == "")
        {
            IsEnteringPreSetting = false;
            _isAddingPreSetting = false;

            return;
        }

        Debug.Log(_enteredPreSettingNumberString);

        if (int.TryParse(_enteredPreSettingNumberString, out int preSettingNumber))
        {
            if (_isAddingPreSetting)
            {
                int firingMachineNumber = _currentFiringMachineDataProvider.CurrentActive;
                Vector3 firingMachineEulerAngles = _currentFiringMachineDataProvider.CurrentEulerAngles;
                int firingMachineZoom = _currentFiringMachineDataProvider.CurrentZoomLevel;

                SetPreSetting(firingMachineNumber, preSettingNumber, firingMachineEulerAngles, firingMachineZoom);
            }
            else
            {
                PreSettingTriggered?.Invoke(preSettingNumber);
            }
        }

        _enteredPreSettingNumberString = "";
        IsEnteringPreSetting = false;
        _isAddingPreSetting = false;
    }

    public void ResetInteraction()
    {
        _enteredPreSettingNumberString = "";
        IsEnteringPreSetting = false;
        _isAddingPreSetting = false;
    }

    #endregion

    #region Set

    private void SetPreSetting(int firingMachineNumber, int firingMachinePreSettingNumber,
        Vector3 preSettingEulerAngles, int preSettingZoom)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        SavedPreSetting savingPreSetting = new SavedPreSetting(firingMachineNumber, firingMachinePreSettingNumber,
            new PreSettingSingle(preSettingEulerAngles, preSettingZoom));

        _dataSavingManager.SavePreSetting(savingPreSetting);

        Debug.Log($"Changed pre-setting for {firingMachineNumber} for index {firingMachinePreSettingNumber}");
    }

    #endregion

    #region Get

    private bool IsIndexValid(int index)
    {
        return index > 0 && (index <= PreSettingMaxIndex ||
                             index == _specialNormalLightPreSettingNumber ||
                             index == _specialInfraredLightPreSettingNumber);
    }

    public PreSettingSingle GetPreSettingSingle(int firingMachineNumber, int firingMachinePreSettingNumber)
    {
        if (firingMachineNumber > _currentFiringMachineDataProvider.TotalCount)
            return null;

        if (IsIndexValid(firingMachinePreSettingNumber) == false)
            return null;

        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return null;

        PreSettingSingle preSettingSingle = _allSavedPreSettings.Find(savedPreSetting =>
                savedPreSetting.FiringMachineNumber == firingMachineNumber && savedPreSetting.PreSettingNumber == firingMachinePreSettingNumber)
            ?.PreSettingSingle ?? null;

        return preSettingSingle;
    }

    #endregion
}