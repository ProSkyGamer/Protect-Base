#region

using TMPro;
using UnityEngine;
using Zenject;

#endregion

[RequireComponent(typeof(TMP_InputField))]
public class SavingInputField : MonoBehaviour, IInitializable
{
    #region Variables & References

    [SerializeField] private string _defaultValue;

    private TMP_InputField _inputField;
    private string _lastSavedValue;

    private const string SavingInputFieldValuePlayerPrefsString = "SavingInputFieldValue_{0}_PlayerPrefs";

    private const string IsSavingInputFieldDefaultValueSetPlayerPrefsString =
        "IsSavingInputFieldDefaultValueSet_{0}_PlayerPrefs";

    private bool _isInitialized;
    private bool _isValueChanged;

    #endregion

    #region Initialization

    public void Initialize()
    {
        if (_isInitialized)
            return;

        _isInitialized = true;

        _inputField = GetComponent<TMP_InputField>();

        _lastSavedValue = PlayerPrefs.GetInt(GetIsDefaultValueSaved(), 0) == 0
            ? _defaultValue
            : PlayerPrefs.GetString(GetSavingInputFieldValuePlayerPrefsAccessString(), _lastSavedValue);

        _inputField.text = _lastSavedValue;

        _inputField.onValueChanged.AddListener(value => _isValueChanged = value != _lastSavedValue);
    }

    #endregion

    #region Input Field

    public void SetValue(string value)
    {
        _inputField.SetTextWithoutNotify(value);
    }

    public void SaveLastValue()
    {
        _lastSavedValue = _inputField.text;

        PlayerPrefs.SetString(GetSavingInputFieldValuePlayerPrefsAccessString(), _lastSavedValue);
    }

    public void ResetValue()
    {
        _isValueChanged = false;
        _inputField.text = _lastSavedValue;
    }

    #endregion

    #region Get

    public string GetCurrentValue()
    {
        if (_isInitialized == false)
            Initialize();

        return _inputField.text;
    }

    public bool IsValueChanged()
    {
        return _isValueChanged;
    }

    private string GetSavingInputFieldValuePlayerPrefsAccessString()
    {
        return GetPlayerPrefsAccessString(SavingInputFieldValuePlayerPrefsString);
    }

    private string GetIsDefaultValueSaved()
    {
        return GetPlayerPrefsAccessString(IsSavingInputFieldDefaultValueSetPlayerPrefsString);
    }

    private string GetPlayerPrefsAccessString(string basePlayerPrefsAccessString)
    {
        string playerPrefsAccessString = string.Format(basePlayerPrefsAccessString, gameObject.GetInstanceID());

        return playerPrefsAccessString;
    }

    #endregion
}