#region

using System;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class MarkerAdditionalInfoButtonUI : MonoBehaviour, IInitializable
{
    #region Events

    public event Action<MarkerAdditionalInfoType, Transform, MapMarkerSingleUI> DisplayAdditionalInfo;

    #endregion

    #region Variables & References

    [SerializeField] private MarkerAdditionalInfoType _markerAdditionalInfoType;
    private Button _markerButton;
    private MapMarkerSingleUI _mapMarkerSingleUI;

    private Transform _followingObjectTransform;
    private bool _isOperationActive;

    #endregion

    #region Initialize

    [Inject]
    public void Construct(Transform newFollowingObjectTransform, MapMarkerSingleUI newMapMarkerSingle)
    {
        _followingObjectTransform = newFollowingObjectTransform;
        _mapMarkerSingleUI = newMapMarkerSingle;
    }

    public void Initialize()
    {
        _markerButton = GetComponent<Button>();

        _markerButton.onClick.AddListener(MarkerButtonClicked);
    }

    private void MarkerButtonClicked()
    {
        DisplayAdditionalInfo?.Invoke(_markerAdditionalInfoType, _followingObjectTransform, _mapMarkerSingleUI);
    }

    #endregion
}