#region

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Zenject;

#endregion

[RequireComponent(typeof(Image))]
public class BaseSelectedItemSingleUI : MonoBehaviour, IInitializable, IDevInterfaceListener
{
    #region Events

    public event Action UpEventTriggered;

    public event Action DownEventTriggered;

    public event Action RightEventTriggered;

    public event Action LeftEventTriggered;

    #endregion

    #region Created Class

    [Serializable]
    public class ChangingTexts
    {
        [FormerlySerializedAs("changingTextItem")]
        public TextMeshProUGUI ChangingTextItem;

        [FormerlySerializedAs("selectedTextColor")]
        public Color SelectedTextColor;

        [FormerlySerializedAs("originalTextColor")] [HideInInspector]
        public Color OriginalTextColor;
    }

    #endregion

    #region Variables & References

    private Image _selectItemImage;

    [FormerlySerializedAs("upSelectedItem")] [SerializeField]
    protected BaseSelectedItemSingleUI _upSelectedItem;

    [FormerlySerializedAs("downSelectedItem")] [SerializeField]
    protected BaseSelectedItemSingleUI _downSelectedItem;

    [FormerlySerializedAs("leftSelectedItem")] [SerializeField]
    protected BaseSelectedItemSingleUI _leftSelectedItem;

    [FormerlySerializedAs("rightSelectedItem")] [SerializeField]
    protected BaseSelectedItemSingleUI _rightSelectedItem;

    [FormerlySerializedAs("currentSelectedItemArrowTransform")] [SerializeField]
    protected Transform _currentSelectedItemArrowTransform;

    [FormerlySerializedAs("allTextsChangingColors")] [SerializeField]
    private List<ChangingTexts> _allTextsChangingColors;

    [FormerlySerializedAs("isInteractionUnlocked")] [SerializeField]
    protected bool _isInteractionUnlocked = true;

    protected bool IsDevInterfaceShowing;

    #endregion

    #region Properties

    public bool IsHasInteractState { get; protected set; }

    public BaseSelectedItemSingleUI UpSelectedItem => _upSelectedItem;

    public BaseSelectedItemSingleUI DownSelectedItem => _downSelectedItem;

    public BaseSelectedItemSingleUI LeftSelectedItem => _leftSelectedItem;

    public BaseSelectedItemSingleUI RightSelectedItem => _rightSelectedItem;

    public bool IsHasUpSelectedItem => _upSelectedItem != null;

    public bool IsHasDownSelectedItem => _downSelectedItem != null;

    public bool IsHasLeftSelectedItem => _leftSelectedItem != null;

    public bool IsHasRightSelectedItem => _rightSelectedItem != null;

    #endregion

    #region Initialization

    public virtual void Initialize()
    {
        _selectItemImage = GetComponent<Image>();

        _selectItemImage.enabled = false;

        if (_currentSelectedItemArrowTransform != null)
            _currentSelectedItemArrowTransform.gameObject.SetActive(false);

        foreach (ChangingTexts textChangingColor in _allTextsChangingColors)
        {
            textChangingColor.OriginalTextColor = textChangingColor.ChangingTextItem.color;
        }
    }

    #endregion

    public void DevInterfaceActivated()
    {
        IsDevInterfaceShowing = true;
    }

    public void DevInterfaceDeactivated()
    {
        IsDevInterfaceShowing = false;
    }

    #region Select

    public virtual void SelectItem()
    {
        _selectItemImage.enabled = true;
        _currentSelectedItemArrowTransform.gameObject.SetActive(true);

        foreach (ChangingTexts textChangingColor in _allTextsChangingColors)
        {
            textChangingColor.ChangingTextItem.color = textChangingColor.SelectedTextColor;
        }
    }

    public virtual void DeselectItem()
    {
        _selectItemImage.enabled = false;

        if (_currentSelectedItemArrowTransform != null)
            _currentSelectedItemArrowTransform.gameObject.SetActive(false);

        foreach (ChangingTexts textChangingColor in _allTextsChangingColors)
        {
            textChangingColor.ChangingTextItem.color = textChangingColor.OriginalTextColor;
        }
    }

    public virtual void PseudoSelectItem()
    {
        _selectItemImage.enabled = true;

        foreach (ChangingTexts textChangingColor in _allTextsChangingColors)
        {
            textChangingColor.ChangingTextItem.color = textChangingColor.SelectedTextColor;
        }
    }

    #endregion

    #region Interact

    public virtual void InteractWithItem()
    {
        if (_isInteractionUnlocked == false)
            return;

        if (IsDevInterfaceShowing)
            return;
    }

    public virtual void StopInteractingWithItem()
    {
    }

    public virtual void NextInteraction()
    {
        if (IsHasInteractState == false)
            return;

        if (_isInteractionUnlocked == false)
            return;
    }

    public virtual void PreviousInteraction()
    {
        if (IsHasInteractState == false)
            return;

        if (_isInteractionUnlocked == false)
            return;
    }

    public virtual void LockInteraction()
    {
        _isInteractionUnlocked = false;
    }

    public virtual void UnlockInteraction()
    {
        _isInteractionUnlocked = true;
    }

    public virtual void InteractUp(out bool isStopping)
    {
        isStopping = false;
        UpEventTriggered?.Invoke();
    }

    public virtual void InteractDown(out bool isStopping)
    {
        isStopping = false;
        DownEventTriggered?.Invoke();
    }

    public virtual void InteractRight(out bool isStopping)
    {
        isStopping = false;
        RightEventTriggered?.Invoke();
    }

    public virtual void InteractLeft(out bool isStopping)
    {
        isStopping = false;
        LeftEventTriggered?.Invoke();
    }

    #endregion

    #region Set Selected Item

    public void SetUpSelectedItem(BaseSelectedItemSingleUI baseSelectedItemSingleUI)
    {
        _upSelectedItem = baseSelectedItemSingleUI;
    }

    public void SetDownSelectedItem(BaseSelectedItemSingleUI baseSelectedItemSingleUI)
    {
        _downSelectedItem = baseSelectedItemSingleUI;
    }

    public void SetRightSelectedItem(BaseSelectedItemSingleUI baseSelectedItemSingleUI)
    {
        _rightSelectedItem = baseSelectedItemSingleUI;
    }

    public void SetLeftSelectedItem(BaseSelectedItemSingleUI baseSelectedItemSingleUI)
    {
        _leftSelectedItem = baseSelectedItemSingleUI;
    }

    #endregion
}