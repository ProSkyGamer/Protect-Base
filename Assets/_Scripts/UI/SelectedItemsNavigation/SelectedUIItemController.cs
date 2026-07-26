public class SelectedUIItemController : ISelectedUIItem
{
    #region Variables & References

    private BaseSelectedItemSingleUI _pseudoSelectedItem;

    private bool _isCanInteract = true;

    #endregion

    #region Properties

    public bool IsCurrentlyInteracting { get; private set; }

    public BaseSelectedItemSingleUI CurrentSelectedItem { get; private set; }

    public bool IsPseudoSelectionEnabled => _pseudoSelectedItem != null;

    #endregion

    #region Interact

    public void InteractNumbers(char interactingCharacter)
    {
        if (CurrentSelectedItem is not InputFieldSelectedItemSingleUI inputFieldSelectedItem)
            return;

        inputFieldSelectedItem.AddCharacter(interactingCharacter);
    }

    public void InterfaceBackspace()
    {
        if (CurrentSelectedItem is not InputFieldSelectedItemSingleUI inputFieldSelectedItem)
            return;

        inputFieldSelectedItem.RemoveLastCharacter();
    }

    public void InterfaceClear()
    {
        if (CurrentSelectedItem is not InputFieldSelectedItemSingleUI inputFieldSelectedItem)
            return;

        inputFieldSelectedItem.ClearField();
    }

    public void InterfaceInteract()
    {
        if (_isCanInteract == false)
            return;

        if (IsCurrentlyInteracting)
        {
            CurrentSelectedItem.StopInteractingWithItem();
            IsCurrentlyInteracting = false;
        }
        else if (IsCurrentlyInteracting == false && CurrentSelectedItem.IsHasInteractState)
        {
            CurrentSelectedItem.InteractWithItem();
            IsCurrentlyInteracting = true;
        }
        else if (CurrentSelectedItem.IsHasInteractState == false)
        {
            CurrentSelectedItem.InteractWithItem();
        }
    }

    public void InterfaceUp()
    {
        if (_isCanInteract == false)
            return;

        if (CurrentSelectedItem.IsHasUpSelectedItem && !IsCurrentlyInteracting)
        {
            CurrentSelectedItem.InteractUp(out bool isStopping);

            if (isStopping)
                return;

            if (CurrentSelectedItem != null)
                SelectItem(CurrentSelectedItem.UpSelectedItem);
        }
        else if (IsCurrentlyInteracting)
        {
            CurrentSelectedItem.NextInteraction();
        }
    }

    public void InterfaceDown()
    {
        if (_isCanInteract == false)
            return;

        if (CurrentSelectedItem.IsHasDownSelectedItem && !IsCurrentlyInteracting)
        {
            CurrentSelectedItem.InteractDown(out bool isStopping);

            if (isStopping)
                return;

            if (CurrentSelectedItem != null)
                SelectItem(CurrentSelectedItem.DownSelectedItem);
        }
        else if (IsCurrentlyInteracting)
        {
            CurrentSelectedItem.PreviousInteraction();
        }
    }

    public void InterfaceLeft()
    {
        if (_isCanInteract == false)
            return;

        if (CurrentSelectedItem.IsHasLeftSelectedItem && !IsCurrentlyInteracting)
        {
            CurrentSelectedItem.InteractLeft(out bool isStopping);

            if (isStopping)
                return;

            if (CurrentSelectedItem != null)
                SelectItem(CurrentSelectedItem.LeftSelectedItem);
        }
        else if (IsCurrentlyInteracting)
        {
            CurrentSelectedItem.PreviousInteraction();
        }
    }

    public void InterfaceRight()
    {
        if (_isCanInteract == false)
            return;

        if (CurrentSelectedItem.IsHasRightSelectedItem && !IsCurrentlyInteracting)
        {
            CurrentSelectedItem.InteractRight(out bool isStopping);

            if (isStopping)
                return;

            if (CurrentSelectedItem != null)
                SelectItem(CurrentSelectedItem.RightSelectedItem);
        }
        else if (IsCurrentlyInteracting)
        {
            CurrentSelectedItem.NextInteraction();
        }
    }

    #endregion

    #region Select

    public void SelectItem(BaseSelectedItemSingleUI selectingItem)
    {
        if (CurrentSelectedItem != null)
            CurrentSelectedItem.DeselectItem();

        CurrentSelectedItem = selectingItem;

        CurrentSelectedItem.SelectItem();
    }

    public void BlockInteraction()
    {
        _isCanInteract = false;
    }

    public void UnlockInteraction()
    {
        _isCanInteract = true;
    }

    #endregion

    #region Pseudo Select

    public void ActivatePseudoSelection(BaseSelectedItemSingleUI baseSelectedItemSingleUI)
    {
        if (_pseudoSelectedItem != null)
            _pseudoSelectedItem.DeselectItem();

        _pseudoSelectedItem = baseSelectedItemSingleUI;

        _pseudoSelectedItem.PseudoSelectItem();
    }

    public void DeactivatePseudoSelection()
    {
        if (_pseudoSelectedItem != null)
            _pseudoSelectedItem.DeselectItem();

        _pseudoSelectedItem = null;
    }

    #endregion
}