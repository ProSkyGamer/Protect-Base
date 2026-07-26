#region

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

#endregion

public class MarkerPageButton : MonoBehaviour, IInitializable, IPointerEnterHandler, IPointerExitHandler
{
    #region Events

    public event Action<MarkerPage> Clicked;

    public event Action<MarkerPage> StartedHover;

    public event Action<MarkerPage> EndedHover;

    #endregion

    #region Variables & References

    [SerializeField] private MarkerPage _linkedMarkerPage;
    private Button _tabButton;
    [SerializeField] private Image _tabSwitchingIndicatorBackgroundImage;
    [SerializeField] private Image _tabSwitchingProgressBarImage;
    [SerializeField] private Image _tabButtonIconImage;
    [SerializeField] private Color _activeTabButtonImageColor;
    [SerializeField] private Color _inactiveTabButtonImageColor;

    public MarkerPage LinkedPage => _linkedMarkerPage;

    #endregion

    #region Initialization

    public void Initialize()
    {
        _tabButton = GetComponent<Button>();

        _tabButton.onClick.AddListener(TabButtonClicked);

        _tabSwitchingIndicatorBackgroundImage.color = _inactiveTabButtonImageColor;
        _tabSwitchingProgressBarImage.color = _activeTabButtonImageColor;
    }

    private void TabButtonClicked()
    {
        Clicked?.Invoke(_linkedMarkerPage);
    }

    #endregion

    #region Hover

    public void OnPointerEnter(PointerEventData eventData)
    {
        StartedHover?.Invoke(_linkedMarkerPage);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EndedHover?.Invoke(_linkedMarkerPage);
    }

    #endregion

    #region Visual

    public void ChangeActiveState(bool isActive)
    {
        _tabSwitchingProgressBarImage.gameObject.SetActive(isActive);
        _tabButtonIconImage.color = isActive ? _activeTabButtonImageColor : _inactiveTabButtonImageColor;
    }

    public void SetProgressBarValue(float progressBarValue)
    {
        _tabSwitchingProgressBarImage.fillAmount = progressBarValue;
    }

    #endregion
}