#region

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

#endregion

public class OperationMapMarkersManagerUI : MonoBehaviour, IInitializable
{
    #region Events

    public event Action<MapMarkerSingleUI> MarkerSelected;

    public event Action<MapMarkerSingleUI> MarkerHover;

    public event Action MarkerStoppedHover;

    #endregion

    #region Variables & References

    private readonly List<MapMarkerSingleUI> _allMapMarkers = new();
    [SerializeField] private List<MarkersAdditionalInfoUI> _allMarkerAdditionalInfoTypes;

    private ObjectLimits _mapLimits;
    private MarkersAdditionalInfoUI _currentDisplayingMarkerAdditionalInfo;

    private readonly List<Vector2> _additionalOffsetMultipliers = new()
    {
        new Vector2(0f, 1f),
        new Vector2(0f, -1f),
        new Vector2(1f, 0f),
        new Vector2(-1f, 0f)
    };

    private ITerritoryInfoProvider _territoryInfoProvider;
    private MarkersFactory _markersFactory;

    public bool IsDisplayingMarkerAdditionalInfo => _currentDisplayingMarkerAdditionalInfo != null;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(ITerritoryInfoProvider territoryInfoProvider, MarkersFactory markersFactory)
    {
        _territoryInfoProvider = territoryInfoProvider;
        _markersFactory = markersFactory;
    }

    private void OnValidate()
    {
        List<MarkersAdditionalInfoUI> markerTypeDuplicates = _allMarkerAdditionalInfoTypes
            .GroupBy(markerInfo => markerInfo)
            .Where(groupedMarkersInfo => groupedMarkersInfo.Count() > 1)
            .Select(groupedMarkerInfo => groupedMarkerInfo.Key).ToList();

        foreach (MarkersAdditionalInfoUI duplicateType in markerTypeDuplicates)
        {
            Debug.LogError($"Тип {duplicateType} уже есть в массиве!");
        }
    }

    public void Initialize()
    {
        _mapLimits = _territoryInfoProvider.GetMapLimits();
    }

    #endregion

    #region Visuals

    public void HideCurrentDisplayingMarkerAdditionalInfo()
    {
        if (_currentDisplayingMarkerAdditionalInfo == null)
            return;

        _currentDisplayingMarkerAdditionalInfo.MarkerAdditionalInfo.Hide();
        _currentDisplayingMarkerAdditionalInfo = null;
    }

    public void HideAllDisplayingAdditionalInfo()
    {
        foreach (MarkersAdditionalInfoUI markerAdditionalInfo in _allMarkerAdditionalInfoTypes)
        {
            markerAdditionalInfo.MarkerAdditionalInfo.Hide();
        }
    }

    #endregion

    #region Add Marker

    public void AddMapMarker(Vector2 mapMarkerPosition, MarkerType markerType, Transform worldObject)
    {
        MarkerSingle newMapMarker = CreateAndPlaceMarker(mapMarkerPosition, markerType, worldObject);

        if (newMapMarker.MarkerAdditionalInfoButton != null)
            newMapMarker.MarkerAdditionalInfoButton.DisplayAdditionalInfo += AdditionalInfo_DisplayAdditionalInfo;

        newMapMarker.MarkerSingleUI.MapMarkerChosen += NewMapMarkerMapMarkerChosen;
        newMapMarker.MarkerSingleUI.MapMarkerPreChosen += NewMapMarkerMapMarkerPreChosen;
        newMapMarker.MarkerSingleUI.MapMarkerUnChosen += NewMapMarkerMapMarkerUnChosen;

        _allMapMarkers.Add(newMapMarker.MarkerSingleUI);
    }

    private MarkerSingle CreateAndPlaceMarker(Vector2 mapMarkerPosition, MarkerType markerType, Transform worldObject)
    {
        MarkerSingle newMapMarker = _markersFactory.Create(markerType, worldObject);
        newMapMarker.MarkerSingleUI.transform.position = mapMarkerPosition;

        return newMapMarker;
    }

    private void AdditionalInfo_DisplayAdditionalInfo(MarkerAdditionalInfoType markerAdditionalInfoType, Transform followingObjectTransform,
        MapMarkerSingleUI mapMarkerSingle)
    {
        if (markerAdditionalInfoType == MarkerAdditionalInfoType.None)
            return;

        foreach (MarkersAdditionalInfoUI markerAdditionalInfo in _allMarkerAdditionalInfoTypes)
        {
            markerAdditionalInfo.MarkerAdditionalInfo.Hide();
        }

        _currentDisplayingMarkerAdditionalInfo = _allMarkerAdditionalInfoTypes.Find(markerSingle =>
            markerSingle.MarkerAdditionalInfoType == markerAdditionalInfoType);

        if (_currentDisplayingMarkerAdditionalInfo == null)
            return;

        _currentDisplayingMarkerAdditionalInfo.MarkerAdditionalInfo.Show(followingObjectTransform,
            GetMarkerAdditionalInfoPosition(mapMarkerSingle.transform.position,
                _currentDisplayingMarkerAdditionalInfo));
    }

    private Vector3 GetMarkerAdditionalInfoPosition(Vector3 baseMarkerPosition,
        MarkersAdditionalInfoUI markerAdditionalInfoUI)
    {
        Vector3 infoSize = markerAdditionalInfoUI.MarkerAdditionalInfo.GetMarkerAdditionalInfoSize();
        Vector2 infoOffset = markerAdditionalInfoUI.MarkerAdditionalInfoOffset;
        Vector3 infoPosition = Vector3.zero;

        foreach (Vector2 offsetMultiplier in _additionalOffsetMultipliers)
        {
            infoPosition = baseMarkerPosition + new Vector3(
                offsetMultiplier.x * (infoOffset.x + infoSize.x / 2),
                offsetMultiplier.y * (infoOffset.y + infoSize.y / 2));

            Vector3 tabTopRightPosition = infoPosition + infoSize / 2;
            Vector3 tabBottomLeftPosition = infoPosition - infoSize / 2;

            if (_mapLimits.IsRectWithinBoundaries(tabBottomLeftPosition, tabTopRightPosition))
                break;
        }

        return infoPosition;
    }

    private void NewMapMarkerMapMarkerUnChosen()
    {
        MarkerStoppedHover?.Invoke();
    }

    private void NewMapMarkerMapMarkerPreChosen(MapMarkerSingleUI chosenMapMarker)
    {
        if (chosenMapMarker == null)
            return;

        MarkerHover?.Invoke(chosenMapMarker);
    }

    private void NewMapMarkerMapMarkerChosen(MapMarkerSingleUI chosenMapMarker)
    {
        if (chosenMapMarker == null)
            return;

        MarkerSelected?.Invoke(chosenMapMarker);
    }

    public void RemoveMarker(Transform worldObject)
    {
        MapMarkerSingleUI removingMarker = _allMapMarkers.Find(marker => marker.WorldObject == worldObject);

        if (removingMarker == null)
            return;

        removingMarker.MapMarkerChosen -= NewMapMarkerMapMarkerChosen;
        removingMarker.MapMarkerPreChosen -= NewMapMarkerMapMarkerPreChosen;
        removingMarker.MapMarkerUnChosen -= NewMapMarkerMapMarkerUnChosen;

        Destroy(removingMarker.gameObject);

        _allMapMarkers.Remove(removingMarker);
    }

    public void StartListeningForMarkerPoint(MarkerType markerType)
    {
        List<MapMarkerSingleUI> allAvailableMarkers = _allMapMarkers.Where(mapMarker => mapMarker.MarkerType == markerType).ToList();

        foreach (MapMarkerSingleUI availableMarker in allAvailableMarkers)
        {
            availableMarker.StartListeningForMapPoint();
        }
    }

    public void StopListeningForMarkerPoint()
    {
        foreach (MapMarkerSingleUI availableMarker in _allMapMarkers)
        {
            availableMarker.StopListeningForMapPoint();
        }
    }

    #endregion

    #region Get

    public MapMarkerSingleUI GetMarkerSingle(MarkerType markerType)
    {
        foreach (MapMarkerSingleUI mapMarkerSingleUI in _allMapMarkers)
        {
            if (mapMarkerSingleUI.MarkerType != markerType)
                continue;

            return mapMarkerSingleUI;
        }

        return null;
    }

    #endregion
}