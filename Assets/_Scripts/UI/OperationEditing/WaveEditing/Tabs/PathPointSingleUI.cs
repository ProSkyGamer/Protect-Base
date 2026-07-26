#region

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class PathPointSingleUI : MonoBehaviour, IInitializable
{
    #region Events

    public event Action<PathPointSingleUI> PathPointDeleted;

    public event Action<PathPointSingleUI> PathPointSelected;

    #endregion

    #region Variables & References

    [SerializeField] private NumberInputFieldFilterUI _xMapCoordinateInputField;
    [SerializeField] private NumberInputFieldFilterUI _yMapCoordinateInputField;
    [SerializeField] private TextMeshProUGUI _currentPathPointText;
    [SerializeField] private string _firstPathPointString = "Точка появления";
    [SerializeField] private string _pathPointString = "Точка {0}";
    [SerializeField] private Button _deletePathPointButton;
    [SerializeField] private Color _selectedPointBackgroundColor;
    [SerializeField] private Color _deselectedPointBackgroundColor;

    private Image _pathPointButtonBackground;
    private Button _pathPointButton;

    private Vector3 _worldPointCoordinates;
    private Vector2 _screenCenteredMapPoint;
    private PathPointType _currentPathPointType;

    private bool _isHasTrustedPointPosition;
    private Vector2 _trustedPointPosition;

    private bool _isBlocked;
    private ObjectLimits _pathPointLimits;

    public int PointIndex { get; private set; }

    #endregion

    #region Initialization

    [Inject]
    public void Construct(bool isBlocked, ObjectLimits objectLimits, int pathPointIndex)
    {
        _isBlocked = isBlocked;
        _pathPointLimits = objectLimits;

        _pathPointButton = GetComponent<Button>();
        _pathPointButtonBackground = GetComponent<Image>();

        _pathPointButtonBackground.color = _deselectedPointBackgroundColor;

        SetIndex(pathPointIndex);
    }

    public void Initialize()
    {
        InitializeInputFieldLimits();

        if (_isBlocked == false)
            SetIndex(PointIndex);

        SubscribeToUIEvents();

        ChangeBlockedState(_isBlocked);
    }

    private void SubscribeToUIEvents()
    {
        _pathPointButton.onClick.AddListener(OnAnyObjectInteraction);

        _xMapCoordinateInputField.GetInputField().onSelect.AddListener(_ => { OnAnyObjectInteraction(); });

        _yMapCoordinateInputField.GetInputField().onSelect.AddListener(_ => { OnAnyObjectInteraction(); });

        _deletePathPointButton.onClick.AddListener(OnDeletePathPointButtonClicked);
    }

    private void InitializeInputFieldLimits()
    {
        _xMapCoordinateInputField.SetMinValue(_pathPointLimits.MinPoint.x);
        _xMapCoordinateInputField.SetMaxValue(_pathPointLimits.MaxPoint.x);
        _yMapCoordinateInputField.SetMinValue(_pathPointLimits.MinPoint.y);
        _yMapCoordinateInputField.SetMaxValue(_pathPointLimits.MaxPoint.y);
    }

    private void OnDeletePathPointButtonClicked()
    {
        if (_isBlocked)
            return;

        PathPointDeleted?.Invoke(this);

        Destroy(gameObject);
    }

    private void OnAnyObjectInteraction()
    {
        if (_isBlocked)
            return;

        Select();
    }

    #endregion

    #region Set

    public void SetIndex(int currentPointIndex)
    {
        PointIndex = currentPointIndex;

        string currentPathPointString = currentPointIndex == 0
            ? _firstPathPointString
            : string.Format(_pathPointString, currentPointIndex);

        _currentPathPointText.text = currentPathPointString;
    }

    public void SetPoint(Vector2 mapCenteredPoint, Vector2 screenCenteredPoint, Vector3 newWorldPointCoordinates)
    {
        _xMapCoordinateInputField.SetAndFilterText(mapCenteredPoint.x.ToString());
        _yMapCoordinateInputField.SetAndFilterText(mapCenteredPoint.y.ToString());

        _screenCenteredMapPoint = screenCenteredPoint;

        _worldPointCoordinates = newWorldPointCoordinates;
    }

    public void SetType(PathPointType newPathPointType)
    {
        _currentPathPointType = newPathPointType;
    }

    public void SetTrustedPosition(Vector2 newTrustedPointPosition)
    {
        _isHasTrustedPointPosition = true;
        _trustedPointPosition = newTrustedPointPosition;
    }

    public void ResetPointToTrusted()
    {
        if (_isHasTrustedPointPosition == false)
            return;

        _xMapCoordinateInputField.SetAndFilterText(_trustedPointPosition.x.ToString());
        _yMapCoordinateInputField.SetAndFilterText(_trustedPointPosition.y.ToString());
    }

    public void ChangeBlockedState(bool isBlocked)
    {
        _isBlocked = isBlocked;

        _yMapCoordinateInputField.GetInputField().interactable = !_isBlocked;
        _xMapCoordinateInputField.GetInputField().interactable = !_isBlocked;
        _deletePathPointButton.interactable = !_isBlocked;
    }

    #endregion

    #region Selection

    public void Select()
    {
        if (_isBlocked)
            return;

        _pathPointButtonBackground.color = _selectedPointBackgroundColor;
        _currentPathPointType = PathPointType.SelectedPathPoint;

        PathPointSelected?.Invoke(this);
    }

    public void Deselect()
    {
        if (_isBlocked)
            return;

        _pathPointButtonBackground.color = _deselectedPointBackgroundColor;

        if (_isHasTrustedPointPosition)
            return;

        PathPointDeleted?.Invoke(this);
        Destroy(gameObject);
    }

    #endregion

    #region Get

    public ReadonlyPathPoint GetPathPoint()
    {
        Vector2 screenCenteredMapPoint = _screenCenteredMapPoint;
        Vector2 mapCenteredMapPoint = new(_xMapCoordinateInputField.GetFloatValue(), _yMapCoordinateInputField.GetFloatValue());
        Vector3 worldPoint = _worldPointCoordinates;

        ReadonlyPathPoint enemyPathPoint = new(screenCenteredMapPoint, mapCenteredMapPoint, worldPoint, _currentPathPointType);

        return enemyPathPoint;
    }

    #endregion
}