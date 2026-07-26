#region

using System;
using System.Collections.Generic;
using UnityEngine;

#endregion

[Serializable]
public class NavigationPanelSingle
{
    public NavigationPanelType NavigationPanelType;
    public Transform NavigationPanelTransform;
}

public class NavigationPanelUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private List<NavigationPanelSingle> _allNavigationPanels;

    #endregion

    #region Visual

    public void Show(NavigationPanelType navigationPanelType)
    {
        gameObject.SetActive(true);

        foreach (NavigationPanelSingle navigationPanelSingle in _allNavigationPanels)
        {
            navigationPanelSingle.NavigationPanelTransform.gameObject.SetActive(false);
        }

        Transform showingNavigationPanel =
            _allNavigationPanels.Find(navigationPanel => navigationPanel.NavigationPanelType == navigationPanelType)
                .NavigationPanelTransform;

        if (showingNavigationPanel == null) return;

        showingNavigationPanel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    #endregion
}