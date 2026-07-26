#region

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

[RequireComponent(typeof(TMP_Dropdown))]
public class DropdownSelectedItemSingleUI : BaseSelectedItemSingleUI
{
    #region Variables & References

    private TMP_Dropdown _currentDropdown;

    [SerializeField] private Transform _interactingVisualsTransform;

    #endregion

    #region Initialization

    public override void Initialize()
    {
        base.Initialize();

        _currentDropdown = GetComponent<TMP_Dropdown>();
        _currentDropdown.interactable = false;

        IsHasInteractState = true;
        _interactingVisualsTransform.gameObject.SetActive(false);
    }

    #endregion

    #region Select

    public override void SelectItem()
    {
        base.SelectItem();

        EventSystem.current.SetSelectedGameObject(gameObject);
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

        _interactingVisualsTransform.gameObject.SetActive(true);

        _currentSelectedItemArrowTransform.gameObject.SetActive(false);
    }

    public override void StopInteractingWithItem()
    {
        base.StopInteractingWithItem();

        _interactingVisualsTransform.gameObject.SetActive(false);

        _currentSelectedItemArrowTransform.gameObject.SetActive(true);
    }

    public override void NextInteraction()
    {
        base.NextInteraction();

        int currentDropdownValue = _currentDropdown.value;
        currentDropdownValue++;
        currentDropdownValue = currentDropdownValue >= _currentDropdown.options.Count ? 0 : currentDropdownValue;
        _currentDropdown.value = currentDropdownValue;
    }

    public override void PreviousInteraction()
    {
        base.PreviousInteraction();

        int currentDropdownValue = _currentDropdown.value;
        currentDropdownValue--;
        currentDropdownValue = currentDropdownValue < 0 ? _currentDropdown.options.Count - 1 : currentDropdownValue;
        _currentDropdown.value = currentDropdownValue;
    }

    #endregion
}