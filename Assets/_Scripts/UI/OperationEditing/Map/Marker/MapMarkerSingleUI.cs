#region

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Zenject;

#endregion

public class MapMarkerSingleUI : MonoBehaviour, IInitializable, IOperationsStatusListener, IPointerEnterHandler, IPointerExitHandler
{
    #region Events

    public event Action<MapMarkerSingleUI> MapMarkerChosen;

    public event Action<MapMarkerSingleUI> MapMarkerPreChosen;

    public event Action MapMarkerUnChosen;

    #endregion

    #region Variables & References

    [SerializeField] private Transform _pointOutlineTransform;
    [SerializeField] private MarkerType _markerType;
    [SerializeField] private Transform _hoverArrowTransform;

    private Button _mapMarkerButton;
    private bool _isListeningForClicks;

    private bool _isOperationActive;

    public MarkerType MarkerType => _markerType;

    public Transform WorldObject { get; private set; }

    public Vector3 MarkerWorldPointPosition { get; private set; }

    #endregion

    #region Initialize

    [Inject]
    public void Construct(Transform worldObject)
    {
        WorldObject = worldObject;
        MarkerWorldPointPosition = worldObject.position;
    }

    public void OperationStarted()
    {
        _isOperationActive = true;
    }

    public void OperationEnded()
    {
        _isOperationActive = false;
    }

    public void Initialize()
    {
        _mapMarkerButton = GetComponent<Button>();

        _mapMarkerButton.onClick.AddListener(MapMarkerButtonClicked);

        _hoverArrowTransform.gameObject.SetActive(false);
    }

    private void MapMarkerButtonClicked()
    {
        if (_isListeningForClicks == false)
            return;

        MapMarkerChosen?.Invoke(this);

        _hoverArrowTransform.gameObject.SetActive(_isOperationActive);
    }

    public void StartListeningForMapPoint()
    {
        _isListeningForClicks = true;
        _pointOutlineTransform.gameObject.SetActive(_isListeningForClicks);
    }

    public void StopListeningForMapPoint()
    {
        _isListeningForClicks = false;
        _pointOutlineTransform.gameObject.SetActive(_isListeningForClicks);
    }

    #endregion

    #region Hover

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_isListeningForClicks == false)
            return;

        MapMarkerPreChosen?.Invoke(this);

        _hoverArrowTransform.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (_isListeningForClicks == false)
            return;

        MapMarkerUnChosen?.Invoke();

        _hoverArrowTransform.gameObject.SetActive(false);
    }

    #endregion
}