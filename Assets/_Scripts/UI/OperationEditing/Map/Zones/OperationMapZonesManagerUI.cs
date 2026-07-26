#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#endregion

public class OperationMapZonesManagerUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private Transform _allBaseZonesTransform;
    [SerializeField] private List<BaseDisplayingZones> _allBaseDisplayingZones;
    [SerializeField] private List<Transform> _allBaseZoneHintsTransforms;
    [SerializeField] private RawImage _displayingZoneRawImagePrefab;
    [SerializeField] private Transform _allDisplayingZonesContainer;
    [SerializeField] private List<CustomDisplayingZoneColors> _allCustomDisplayingZoneColors;
    [SerializeField] private List<CustomDisplayingZoneHints> _allCustomDisplayingZoneHints;

    private bool _isDisplayingZones;

    #endregion

    #region Zones

    public void ToggleZonesDisplayStatus()
    {
        _isDisplayingZones = !_isDisplayingZones;

        _allBaseZonesTransform.gameObject.SetActive(_isDisplayingZones);

        foreach (Transform zoneHintTransform in _allBaseZoneHintsTransforms)
        {
            zoneHintTransform.gameObject.SetActive(_isDisplayingZones);
        }
    }

    public void ChangeCurrentZonesDisplayStatus(bool newStatus)
    {
        _allBaseZonesTransform.gameObject.SetActive(newStatus);

        _isDisplayingZones = newStatus;

        foreach (Transform zoneHintTransform in _allBaseZoneHintsTransforms)
        {
            zoneHintTransform.gameObject.SetActive(newStatus);
        }
    }

    public void DisplayZones(List<CustomDisplayingZones> allDisplayingZones)
    {
        ClearCurrentZones();
        List<ZoneAvailabilityType> allDisplayingZoneAvailabilityTypes = new();

        foreach (CustomDisplayingZones customDisplayingZone in allDisplayingZones)
        {
            if (customDisplayingZone.DisplayingZoneTypes.Count > 0)
                allDisplayingZoneAvailabilityTypes.Add(customDisplayingZone.ZoneAvailabilityType);

            foreach (OperationTerritoryType displayingZoneType in customDisplayingZone.DisplayingZoneTypes)
            {
                RawImage newDisplayingZoneImage =
                    Instantiate(_displayingZoneRawImagePrefab, _allDisplayingZonesContainer);

                newDisplayingZoneImage.texture = GetZoneTexture(displayingZoneType);
                newDisplayingZoneImage.color = GetAvailabilityColor(customDisplayingZone.ZoneAvailabilityType);
            }
        }

        ChangeCurrentZonesDisplayStatus(false);
        DisplayCurrentZoneHints(allDisplayingZoneAvailabilityTypes);
    }

    private void DisplayCurrentZoneHints(List<ZoneAvailabilityType> displayingZoneAvailabilityType)
    {
        foreach (CustomDisplayingZoneHints customDisplayingZoneHint in _allCustomDisplayingZoneHints)
        {
            customDisplayingZoneHint.ZoneAvailabilityHint.gameObject.SetActive(
                displayingZoneAvailabilityType.Contains(customDisplayingZoneHint.ZoneAvailabilityType));
        }
    }

    public void ClearCurrentZones()
    {
        foreach (Transform toDelete in _allDisplayingZonesContainer.GetComponentsInChildren<Transform>())
        {
            if (toDelete == _displayingZoneRawImagePrefab.transform || toDelete == _allDisplayingZonesContainer) continue;

            Destroy(toDelete.gameObject);
        }

        DisplayCurrentZoneHints(new List<ZoneAvailabilityType>());
    }

    #endregion

    #region Get

    private Texture GetZoneTexture(OperationTerritoryType zoneType)
    {
        return _allBaseDisplayingZones.Find(baseDisplayZone => baseDisplayZone.ZoneType == zoneType)?.ZoneRenderTexture;
    }

    private Color GetAvailabilityColor(ZoneAvailabilityType zoneAvailabilityType)
    {
        return _allCustomDisplayingZoneColors.Find(zoneColor => zoneColor.ZoneAvailabilityType == zoneAvailabilityType)?.ZoneColor ?? Color.white;
    }

    #endregion
}