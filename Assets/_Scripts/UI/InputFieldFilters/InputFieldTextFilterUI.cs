#region

using System;
using System.Linq;
using TMPro;
using UnityEngine;
using Zenject;

#endregion

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldTextFilterUI : MonoBehaviour, IInitializable
{
    #region Events & Event Args

    public event Action<string> TextChanged;

    #endregion

    #region Variables & References

    private TMP_InputField _filteringInputField;

    [SerializeField] private string _blockedSymbols;
    [SerializeField] private bool _isAllOtherSymbolsAllowed;
    [SerializeField] private string _allowedSymbols;
    [SerializeField] private SelectableItemsNotificationDisplayer _inputFieldSelectableItemsNotificationDisplayer;
    [SerializeField] private string _forbiddenSymbolNotificationString = "Использование данного символа запрещено! Он был УДАЛЕН!";

    #endregion

    #region Initialization

    public void Initialize()
    {
        _filteringInputField = GetComponent<TMP_InputField>();

        _filteringInputField.onValueChanged.AddListener(newString =>
        {
            string filteredString = FilterText(newString, out bool isHasForbiddenSymbol);

            if (isHasForbiddenSymbol)
                _inputFieldSelectableItemsNotificationDisplayer.ShowNotification(_forbiddenSymbolNotificationString);

            _filteringInputField.SetTextWithoutNotify(filteredString);

            TextChanged?.Invoke(filteredString);
        });
    }

    #endregion

    #region Set Text

    public void SetUnfilteredText(string unfilteredText)
    {
        _filteringInputField.text = unfilteredText;
    }

    public void SetFilteredText(string filteredText)
    {
        _filteringInputField.SetTextWithoutNotify(filteredText);
    }

    #endregion

    #region Get

    private string FilterText(string input, out bool isHasForbiddenSymbol)
    {
        string filteredString = new(input.Where(symbol =>
            _blockedSymbols.Contains(symbol) == false && (_isAllOtherSymbolsAllowed || _allowedSymbols.Contains(symbol))).ToArray());

        isHasForbiddenSymbol = filteredString != input;

        return filteredString;
    }

    #endregion
}