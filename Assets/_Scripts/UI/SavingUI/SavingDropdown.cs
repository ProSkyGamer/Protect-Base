#region

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

[RequireComponent(typeof(TMP_Dropdown))]
public class SavingDropdown : MonoBehaviour, IInitializable
{
    #region Variables & References

    [SerializeField] private int _defaultValue;
    [SerializeField] private SavingToggleButton _saveWindowTypeButton;

    private TMP_Dropdown _dropdown;
    private int _lastSavedValue;

    private const string SavingDropdownValuePlayerPrefsString = "SavingDropdownValue_{0}_PlayerPrefs";

    private const string IsSavingDropdownDefaultValueSetPlayerPrefsString =
        "IsSavingDropdownDefaultValueSet_{0}_PlayerPrefs";

    private bool _isInitialized;
    private bool _isValueChanged;

    #endregion

    #region Initialization

    private void OnValidate()
    {
        _dropdown = GetComponent<TMP_Dropdown>();
    }

    public void Initialize()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        _lastSavedValue = PlayerPrefs.GetInt(GetIsDefaultValueSaved(), 0) == 0
            ? _defaultValue
            : PlayerPrefs.GetInt(GetSavingDropdownValuePlayerPrefsAccessString());

        _dropdown.onValueChanged.AddListener(value => _isValueChanged = value != _lastSavedValue);

        _dropdown.value = _lastSavedValue;
    }

    #endregion

    #region Dropdown

    public void SetValue(int value)
    {
        _dropdown.SetValueWithoutNotify(value);
    }

    public void SetDropdownValues(List<string> dropdownValues)
    {
        _lastSavedValue = 0;
        _dropdown.ClearOptions();

        foreach (string dropdownValue in dropdownValues)
        {
            _dropdown.options.Add(new TMP_Dropdown.OptionData(dropdownValue));
        }
    }

    public void SaveLastValue()
    {
        _lastSavedValue = _dropdown.value;

        if (_saveWindowTypeButton != null)
            _saveWindowTypeButton.SaveLastValue();

        if (_saveWindowTypeButton == null || _saveWindowTypeButton != null && _saveWindowTypeButton.GetCurrentValue())
        {
            PlayerPrefs.SetInt(GetIsDefaultValueSaved(), 1);
            PlayerPrefs.SetInt(GetSavingDropdownValuePlayerPrefsAccessString(), _lastSavedValue);
        }
    }

    public void ResetValue()
    {
        _isValueChanged = false;
        _dropdown.value = _lastSavedValue;

        if (_saveWindowTypeButton != null)
            _saveWindowTypeButton.ResetValue();
    }

    #endregion

    #region Get

    public int GetCurrentValue()
    {
        if (_isInitialized == false)
            Initialize();

        return _dropdown.value;
    }

    public bool IsValueChanged()
    {
        return _isValueChanged;
    }

    private string GetSavingDropdownValuePlayerPrefsAccessString()
    {
        return GetPlayerPrefsAccessString(SavingDropdownValuePlayerPrefsString);
    }

    private string GetIsDefaultValueSaved()
    {
        return GetPlayerPrefsAccessString(IsSavingDropdownDefaultValueSetPlayerPrefsString);
    }

    private string GetPlayerPrefsAccessString(string basePlayerPrefsAccessString)
    {
        string playerPrefsAccessString = string.Format(basePlayerPrefsAccessString, gameObject.GetInstanceID());

        return playerPrefsAccessString;
    }

    #endregion
}