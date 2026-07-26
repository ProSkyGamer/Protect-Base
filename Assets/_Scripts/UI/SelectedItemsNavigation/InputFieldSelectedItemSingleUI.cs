#region

using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

#endregion

[RequireComponent(typeof(TMP_InputField))]
public class InputFieldSelectedItemSingleUI : BaseSelectedItemSingleUI
{
    #region Events

    public event Action OnStoppedInteracting;

    #endregion

    #region Variables & References

    private TMP_InputField _currentInputField;
    private NumberInputFieldFilterUI _numberInputFieldFilterUI;

    private bool _isHasFiltration;
    private bool _isCurrentlyInteracting;

    [SerializeField] private bool _isInputHidden;

    private SelectedUIItemController _selectedUIItemController;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(SelectedUIItemController selectedUIItemController)
    {
        _selectedUIItemController = selectedUIItemController;

        TryGetComponent(out _numberInputFieldFilterUI);
        _currentInputField = GetComponent<TMP_InputField>();

        IsHasInteractState = true;
        _currentInputField.interactable = false;
        _currentInputField.onFocusSelectAll = false;

        _currentInputField.contentType =
            _isInputHidden ? TMP_InputField.ContentType.Password : TMP_InputField.ContentType.Standard;
    }

    public void AddCharacter(char addingCharacter)
    {
        if (IsDevInterfaceShowing)
            return;

        if (_selectedUIItemController.CurrentSelectedItem != this)
            return;

        if (_isCurrentlyInteracting == false)
            return;

        string allowedCharacters = "0123456789-";

        if (allowedCharacters.Contains(addingCharacter) == false)
            return;

        if (addingCharacter == '-')
        {
            if (_numberInputFieldFilterUI == null)
                return;

            if (_numberInputFieldFilterUI.IsMinusAllowed() == false)
                return;

            if (_currentInputField.text.Length != 0)
                return;
        }

        string currentInputFieldText = _currentInputField.text;
        currentInputFieldText += addingCharacter;
        _currentInputField.SetTextWithoutNotify(currentInputFieldText);
    }

    public void RemoveLastCharacter()
    {
        if (_currentInputField.text.Length > 0)
            _currentInputField.SetTextWithoutNotify(
                _currentInputField.text.Remove(_currentInputField.text.Length - 1));
    }

    public void ClearField()
    {
        _currentInputField.SetTextWithoutNotify("");
    }

    #endregion

    #region Interact

    public override void InteractWithItem()
    {
        base.InteractWithItem();

        if (_isInteractionUnlocked == false)
            return;

        if (IsDevInterfaceShowing)
            return;

        _currentSelectedItemArrowTransform.gameObject.SetActive(false);

        EventSystem.current.SetSelectedGameObject(gameObject);

        _isCurrentlyInteracting = true;
        _currentInputField.caretPosition = _currentInputField.text.Length;
        _currentInputField.caretWidth = 1;
    }

    public override void StopInteractingWithItem()
    {
        base.StopInteractingWithItem();

        _currentSelectedItemArrowTransform.gameObject.SetActive(true);

        _isCurrentlyInteracting = false;

        if (_numberInputFieldFilterUI != null)
            _numberInputFieldFilterUI.SetAndFilterText(_currentInputField.text);

        OnStoppedInteracting?.Invoke();
    }

    #endregion
}