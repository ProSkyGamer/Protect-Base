#region

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

#endregion

public class FiringMachineMarkerAdditionalInfo : MarkerAdditionalInfo, IOperationUpdateListener,
    IPointerEnterHandler, IPointerExitHandler, IDisposable
{
    #region Variables & References

    [SerializeField] private float _tabAutoSwitchInterval = 5f;
    [SerializeField] private List<MarkerPageButton> _allMarkerTabsButtons;

    private MarkerPage _currentDisplayingPage;
    private MarkerPageButton _currentActiveInfoPageButton;

    private MarkerPage _previousDisplayingPage;
    private bool _isCurrentlyDisplaying;
    private bool _isDisplayingTabTemporary;

    private IFiringMachineDataProvider _displayingFiringMachine;

    private bool _isTabHovered;

    #endregion

    #region Initialization

    public override void Initialize()
    {
        foreach (MarkerPageButton markerTabButton in _allMarkerTabsButtons)
        {
            markerTabButton.Clicked += MarkerTabButton_OnTabButtonClicked;
            markerTabButton.StartedHover += MarkerTabButton_OnTabButtonHover;
            markerTabButton.EndedHover += MarkerTabButton_OnTabButtonStoppedHover;

            markerTabButton.SetProgressBarValue(0f);
        }

        foreach (MarkerPage markerPage in _otherPages)
        {
            markerPage.Hide();
        }

        base.Initialize();
    }

    public void UpdateOperationsVisuals()
    {
        _mainPage.UpdateVisuals();
        _currentDisplayingPage.UpdateVisuals();
    }

    private void MarkerTabButton_OnTabButtonClicked(MarkerPage markerPage)
    {
        _isDisplayingTabTemporary = false;
        ChangeCurrentTab(markerPage);
    }

    private void MarkerTabButton_OnTabButtonHover(MarkerPage markerPage)
    {
        _isDisplayingTabTemporary = true;

        _previousDisplayingPage = _currentDisplayingPage;

        ChangeTabVisual(markerPage);
        _currentActiveInfoPageButton.SetProgressBarValue(1f);
    }

    private void MarkerTabButton_OnTabButtonStoppedHover(MarkerPage markerPage)
    {
        ChangeTabVisual(_previousDisplayingPage);
        _isDisplayingTabTemporary = false;
    }

    #endregion

    #region Visual

    public override void Show(Transform followingTransform, Vector3 additionalTabPosition)
    {
        _displayingFiringMachine = followingTransform.GetComponent<IFiringMachineDataProvider>();

        if (_displayingFiringMachine == null)
            return;

        gameObject.SetActive(true);

        _isCurrentlyDisplaying = true;

        DisplayFiringMachineInfo(followingTransform, additionalTabPosition);

        UpdateVisuals();
        ChangeCurrentTab(_otherPages[0]);
    }

    private void DisplayFiringMachineInfo(Transform followingTransform, Vector3 additionalInfoTabPosition)
    {
        transform.position = additionalInfoTabPosition;

        _mainPage.InitializePage(followingTransform);

        foreach (MarkerPage markerPage in _otherPages)
        {
            markerPage.InitializePage(followingTransform);
        }
    }

    public override void Hide()
    {
        if (_isTabHovered)
            return;

        gameObject.SetActive(false);
        _isCurrentlyDisplaying = false;
    }

    public override void UpdateVisuals()
    {
        _mainPage.UpdateVisuals();

        foreach (MarkerPage markerPage in _otherPages)
        {
            markerPage.UpdateVisuals();
        }
    }

    private void ChangeCurrentTab(MarkerPage newDisplayingPage)
    {
        ChangeTabVisual(newDisplayingPage);

        MarkerPage nextSwitchingTab = GetNextSwitchingTab();

        AutomaticallySwapCurrentTabAsync(nextSwitchingTab).Forget();
    }

    private MarkerPage GetNextSwitchingTab()
    {
        int currentPageIndex = _otherPages.IndexOf(_currentDisplayingPage);
        int nextPageIndex = currentPageIndex + 1;
        nextPageIndex = nextPageIndex >= _otherPages.Count ? 0 : nextPageIndex;

        return _otherPages[nextPageIndex];
    }

    private MarkerPageButton GetMarkerPageButton(MarkerPage markerPage)
    {
        MarkerPageButton markerPageButton = _allMarkerTabsButtons.Find(tabButton => tabButton.LinkedPage == markerPage);

        return markerPageButton;
    }

    private async UniTaskVoid AutomaticallySwapCurrentTabAsync(MarkerPage nextSwitchingPage)
    {
        float tabAutoSwitchTimer = _tabAutoSwitchInterval;

        while (tabAutoSwitchTimer > 0)
        {
            if (_isCurrentlyDisplaying == false)
                return;

            if (_isDisplayingTabTemporary)
            {
                await UniTask.NextFrame();

                continue;
            }

            tabAutoSwitchTimer -= Time.deltaTime;

            _currentActiveInfoPageButton.SetProgressBarValue(1f - tabAutoSwitchTimer / _tabAutoSwitchInterval);

            await UniTask.NextFrame();
        }

        ChangeCurrentTab(nextSwitchingPage);
    }

    private void ChangeTabVisual(MarkerPage markerPage)
    {
        if (_currentActiveInfoPageButton != null)
            _currentActiveInfoPageButton.ChangeActiveState(false);

        if (_currentDisplayingPage != null)
            _currentDisplayingPage.Hide();

        _currentDisplayingPage = markerPage;
        _currentActiveInfoPageButton = GetMarkerPageButton(_currentDisplayingPage);

        MarkerPageButton newActiveTabButton = _allMarkerTabsButtons.Find(tabButton => tabButton.LinkedPage == markerPage);

        if (newActiveTabButton != null)
            newActiveTabButton.ChangeActiveState(true);

        markerPage.Show();
    }

    #endregion

    #region Hover

    public void OnPointerEnter(PointerEventData eventData)
    {
        _isTabHovered = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isTabHovered = false;
    }

    #endregion

    public void Dispose()
    {
        foreach (MarkerPageButton markerTabButton in _allMarkerTabsButtons)
        {
            markerTabButton.Clicked -= MarkerTabButton_OnTabButtonClicked;
            markerTabButton.StartedHover -= MarkerTabButton_OnTabButtonHover;
            markerTabButton.EndedHover -= MarkerTabButton_OnTabButtonStoppedHover;
        }
    }
}