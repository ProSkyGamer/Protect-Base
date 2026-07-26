#region

using System;
using UnityEngine;
using Zenject;

#endregion

public abstract class BasePageUI : MonoBehaviour, ISceneResettable, IUIPage
{
    #region Events

    public event Action PageShown;
    public event Action HideRequested;

    public event Action PageHidden;

    #endregion

    #region Variables & References

    [SerializeField] protected BaseSelectedItemSingleUI _firstSelectedItemUI;
    [SerializeField] private bool _isRememberingLastChosen;

    private BaseSelectedItemSingleUI _lastSelectedItemUI;
    protected bool IsShown;
    protected SelectedUIItemController SelectedUIItemController;

    public virtual bool IsCanHide => false;

    #endregion

    #region Visual

    [Inject]
    public void Construct(SelectedUIItemController selectedUIItemController)
    {
        SelectedUIItemController = selectedUIItemController;
    }

    public virtual void Show()
    {
        IsShown = true;

        gameObject.SetActive(true);

        BaseSelectedItemSingleUI selectingItem = _isRememberingLastChosen && _lastSelectedItemUI != null
            ? _lastSelectedItemUI
            : _firstSelectedItemUI;

        if (selectingItem != null)
            SelectedUIItemController.SelectItem(selectingItem);

        PageShown?.Invoke();
    }

    public virtual void Hide()
    {
        if (IsShown && _isRememberingLastChosen)
            _lastSelectedItemUI = SelectedUIItemController.CurrentSelectedItem;

        IsShown = false;

        gameObject.SetActive(false);

        PageHidden?.Invoke();
    }

    public void RequestHide()
    {
        HideRequested?.Invoke();
    }

    public virtual void UpdateVisuals()
    {
    }

    #endregion

    public void OnSceneReset()
    {
        _lastSelectedItemUI = null;
    }
}