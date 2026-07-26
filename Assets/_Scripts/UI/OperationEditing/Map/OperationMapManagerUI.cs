#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class OperationMapManagerUI : MonoBehaviour, IInitializable
{
    #region Events

    public event Action InterfaceClicked;
    public event Action BaseZonesDisplayed;

    #endregion

    #region Variables & References

    [SerializeField] private Button _displayBaseZonesButtonToggle;
    [SerializeField] private List<MapListeningTypeHints> _allMapListeningTypeHints;
    [SerializeField] private Transform _phantomMapPoint;

    #endregion

    #region Initialization

    public void Initialize()
    {
        _displayBaseZonesButtonToggle.onClick.AddListener(DisplayBaseZoneButtonClicked);
    }

    private void DisplayBaseZoneButtonClicked()
    {
        BaseZonesDisplayed?.Invoke();
    }

    #endregion

    #region Map Points

    public void HidePhantomPoint()
    {
        _phantomMapPoint.gameObject.SetActive(false);
    }

    public void UpdateCurrentPhantomPointPosition(Vector2 newPhantomPointPosition)
    {
        if (_phantomMapPoint.gameObject.activeSelf == false)
            _phantomMapPoint.gameObject.SetActive(true);

        _phantomMapPoint.transform.position = newPhantomPointPosition;
    }

    public void DisplayHint(MapListeningPointType listeningPointType)
    {
        _allMapListeningTypeHints.Find(hint => hint.MapListeningPointType == listeningPointType).MapListeningTypeHintTransform.gameObject
            .SetActive(true);
    }

    public void HideAllHints()
    {
        foreach (MapListeningTypeHints hint in _allMapListeningTypeHints)
        {
            hint.MapListeningTypeHintTransform.gameObject.SetActive(false);
        }
    }

    public void InterfaceClick()
    {
        InterfaceClicked?.Invoke();
    }

    #endregion
}