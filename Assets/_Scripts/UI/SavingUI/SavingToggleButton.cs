#region

using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

[RequireComponent(typeof(Button))]
public class SavingToggleButton : MonoBehaviour, IInitializable
{
    #region Variables & References

    [SerializeField] private Transform _activeToggleTransform;
    [SerializeField] private Transform _notActiveToggleTransform;
    [SerializeField] private bool _initialState = true;

    private Button _button;

    private bool _isActive;
    private bool _lastSavedValue;
    private bool _isValueChanged;

    private const string SavingButtonValuePlayerPrefsString = "SavingButtonValue_{0}_PlayerPrefs";

    private bool _isInitialized;

    #endregion

    #region Initialization

    public void Initialize()
    {
        if (_isInitialized)
            return;

        _button = GetComponent<Button>();

        _button.onClick.AddListener(OnButtonClick);

        int savedInt = PlayerPrefs.GetInt(GetSavingToggleButtonValuePlayerPrefsAccessString());
        _isActive = savedInt == 0 ? _initialState : savedInt == 2;
        _lastSavedValue = _isActive;
        UpdateVisuals();

        _isInitialized = true;
    }

    private void OnButtonClick()
    {
        _isActive = !_isActive;

        UpdateVisuals();
    }

    #endregion

    #region Button

    public void SaveLastValue()
    {
        _lastSavedValue = _isActive;
        _isValueChanged = _isActive != _lastSavedValue;

        PlayerPrefs.SetInt(GetSavingToggleButtonValuePlayerPrefsAccessString(), _isActive ? 2 : 1);
    }

    public void ResetValue()
    {
        _isActive = _lastSavedValue;
        _isValueChanged = false;

        UpdateVisuals();
    }

    #endregion

    #region Visuals

    private void UpdateVisuals()
    {
        _activeToggleTransform.gameObject.SetActive(_isActive);
        _notActiveToggleTransform.gameObject.SetActive(!_isActive);
    }

    #endregion

    #region Get

    public bool GetCurrentValue()
    {
        if (_isInitialized == false)
            Initialize();

        return _isActive;
    }

    public bool IsValueChanged()
    {
        return _isValueChanged;
    }

    private string GetSavingToggleButtonValuePlayerPrefsAccessString()
    {
        string savingDropdownValuePlayerPrefsAccessString =
            string.Format(SavingButtonValuePlayerPrefsString, gameObject.GetInstanceID());

        return savingDropdownValuePlayerPrefsAccessString;
    }

    #endregion
}