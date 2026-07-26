#region

using System;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

[RequireComponent(typeof(TMP_InputField))]
public class NumberInputFieldFilterUI : MonoBehaviour, IInitializable, ILateInitializable
{
    #region Events

    public event Action<string> TextChanged;

    #endregion

    #region Variables & References

    [SerializeField] private string _allowedNumbers = "0123456789";
    [SerializeField] private bool _isDotAllowed;
    [SerializeField] private int _maxCharsNumberAfterDot = 2;
    [SerializeField] private bool _isMinusAllowed;
    [SerializeField] private bool _isHasMaxValue;
    [SerializeField] private float _maxValue;
    [SerializeField] private bool _isHasMinValue;
    [SerializeField] private float _minValue;
    [SerializeField] private TextMeshProUGUI _minMaxText;
    [SerializeField] private string _minMaxFormatString = "({0}...{1})";
    private TMP_InputField _currentInputField;

    #endregion

    #region Initialization

    private void OnValidate()
    {
        if (_isHasMaxValue || _isHasMinValue)
        {
            string minValueString = "";
            string maxValueString = "";

            if (_isHasMinValue)
                minValueString = _isMinusAllowed ? _minValue > 0 ? $"+{_minValue}" : $"{_minValue}" : $"{_minValue}";

            if (_isHasMaxValue)
                maxValueString = _isMinusAllowed ? _maxValue > 0 ? $"+{_maxValue}" : $"{_maxValue}" : $"{_maxValue}";

            string minMaxString = string.Format(_minMaxFormatString, minValueString, maxValueString);

            if (_minMaxText != null)
                _minMaxText.text = minMaxString;
        }
    }

    public void Initialize()
    {
        _currentInputField = GetComponent<TMP_InputField>();

        SubscribeToInputFieldEvents();
    }

    public void LateInitialize()
    {
        SetAndFilterText(_currentInputField.text);
    }

    private void SubscribeToInputFieldEvents()
    {
        _currentInputField.onValueChanged.AddListener(OnCurrentInputFieldTextChanged);
        _currentInputField.onSubmit.AddListener(SetAndFilterText);
        _currentInputField.onDeselect.AddListener(SetAndFilterText);
    }

    private void OnCurrentInputFieldTextChanged(string newValue)
    {
        string filteredString = FilterStringSymbols(newValue);
        _currentInputField.SetTextWithoutNotify(filteredString);

        TextChanged?.Invoke(filteredString);

        _currentInputField.caretPosition = filteredString.Length;
        _currentInputField.caretWidth = 1;
    }

    #endregion

    #region Set

    public void SetAndFilterText(string unfilteredText)
    {
        string filteredString = FilterString(unfilteredText);

        _currentInputField.SetTextWithoutNotify(filteredString);

        TextChanged?.Invoke(filteredString);

        _currentInputField.caretPosition = filteredString.Length;
        _currentInputField.caretWidth = 1;
    }

    public void SetMaxValue(float newMaxValue)
    {
        _isHasMaxValue = true;
        _maxValue = newMaxValue;
    }

    public void SetMinValue(float newMinValue)
    {
        _isHasMinValue = true;
        _minValue = newMinValue;
    }

    #endregion

    #region Filter

    private string FilterStringSymbols(string fullText)
    {
        bool isContainsDot = false;
        bool isContainsMinus = false;
        int charsAfterDot = 0;
        string filteredString = "";

        foreach (char newStringChar in fullText)
        {
            if (newStringChar == '.' || newStringChar == ',')
            {
                if (_isDotAllowed && !isContainsDot)
                {
                    if (filteredString == "")
                        filteredString += '0';

                    filteredString += ",";
                    isContainsDot = true;
                }

                continue;
            }

            if (newStringChar == '-' || newStringChar == '+')
                if (_isMinusAllowed && !isContainsMinus)
                    if (filteredString == "")
                    {
                        filteredString += newStringChar;
                        isContainsMinus = true;

                        continue;
                    }
                    else
                    {
                        continue;
                    }

            if (_allowedNumbers.Contains(newStringChar))
            {
                if (filteredString == "" && newStringChar == '0' && fullText != "0")
                    continue;

                if (isContainsDot && charsAfterDot >= _maxCharsNumberAfterDot)
                    break;

                filteredString += newStringChar;

                if (isContainsDot)
                    charsAfterDot++;
            }
        }

        return filteredString;
    }

    private string FilterString(string fullText)
    {
        if (_currentInputField == null)
            _currentInputField = GetComponent<TMP_InputField>();

        string filteredString = FilterStringSymbols(fullText);

        if (filteredString == "")
        {
            _currentInputField.SetTextWithoutNotify("0");
            filteredString = "0";
        }

        if (_isHasMaxValue)
        {
            if (_isDotAllowed)
            {
                if (GetFloatValue() > _maxValue)
                    filteredString = _maxValue.ToString();
            }
            else
            {
                if (GetIntValue() > _maxValue)
                    filteredString = ((int)_maxValue).ToString();
            }
        }

        if (_isHasMinValue)
        {
            if (_isDotAllowed)
            {
                if (GetFloatValue() < _minValue)
                    filteredString = _minValue.ToString();
            }
            else
            {
                if (GetIntValue() < _minValue)
                    filteredString = ((int)_minValue).ToString();
            }
        }

        if (_isMinusAllowed)
            if (GetIntValue() > 0 && filteredString[0] != '+')
                filteredString = "+" + filteredString;

        return filteredString;
    }

    #endregion

    #region Interactable

    public void SetInteractability(bool isInteractable)
    {
        if (_currentInputField == null)
            Initialize();

        _currentInputField.interactable = isInteractable;
    }

    #endregion

    #region Get

    public bool IsMinusAllowed()
    {
        return _isMinusAllowed;
    }

    public int GetIntValue()
    {
        if (_currentInputField == null)
            Initialize();

        if (int.TryParse(_currentInputField.text, out int parsedInt))
            return parsedInt;

        return -1;
    }

    public float GetFloatValue()
    {
        if (_currentInputField == null)
            Initialize();

        if (float.TryParse(_currentInputField.text, out float parsedFloat))
            return parsedFloat;

        return -1f;
    }

    public TMP_InputField GetInputField()
    {
        return _currentInputField;
    }

    #endregion
}