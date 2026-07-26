#region

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class AllOperationPresetsListUI : MonoBehaviour, IInitializable
{
    #region Events

    public event Action CurrentOperationAdded;
    public event Action<int> OperationDeleted;

    public event Action<SavedOperationData> SelectedOperationChanged;
    public event Action<SavedOperationData> OperationSelected;

    public event Action Hidden;
    public event Action Displayed;

    #endregion

    #region Variables & References

    [SerializeField] private Transform _operationsLoadingTransform;
    [SerializeField] private Button _addCurrentOperationButton;
    [SerializeField] private Button _closeTabButton;

    private SavedOperationUIFactory _savedOperationUIFactory;
    private readonly List<SavedOperationSingleUI> _currentDisplayingSavedOperations = new();

    public bool IsDisplaying { get; private set; }

    #endregion

    #region Initialization

    [Inject]
    public void Construct(SavedOperationUIFactory savedOperationUIFactory)
    {
        _savedOperationUIFactory = savedOperationUIFactory;
    }

    public void Initialize()
    {
        _operationsLoadingTransform.gameObject.SetActive(false);

        _addCurrentOperationButton.onClick.AddListener(() => { CurrentOperationAdded?.Invoke(); });

        _closeTabButton.onClick.AddListener(Hide);
    }

    private void NewSavedOperationOperationDeleted(SavedOperationData deletingOperation)
    {
        OperationDeleted?.Invoke(deletingOperation.OperationIndex);
    }

    private void NewSavedOperationOn_DisplayMoreOperationInfo(SavedOperationData savedOperationData)
    {
        SelectedOperationChanged?.Invoke(savedOperationData);
    }

    private void NewSavedOperationOperationChosen(SavedOperationData savedOperationData)
    {
        OperationSelected?.Invoke(savedOperationData);
    }

    #endregion

    #region Add

    public void UpdateCurrentSavedOperationsList(List<SavedOperationData> savedOperationsSingle)
    {
        foreach (SavedOperationData savedOperationSingle in savedOperationsSingle)
        {
            SavedOperationSingleUI newSavedOperation = _savedOperationUIFactory.Create(savedOperationSingle);

            newSavedOperation.OperationChosen += NewSavedOperationOperationChosen;
            newSavedOperation.DisplayedMoreOperationInfo += NewSavedOperationOn_DisplayMoreOperationInfo;
            newSavedOperation.OperationDeleted += NewSavedOperationOperationDeleted;

            _currentDisplayingSavedOperations.Add(newSavedOperation);
        }
    }

    public void ClearAllSavedOperations()
    {
        foreach (SavedOperationSingleUI displayingOperationSingle in _currentDisplayingSavedOperations)
        {
            displayingOperationSingle.OperationChosen -= NewSavedOperationOperationChosen;
            displayingOperationSingle.DisplayedMoreOperationInfo -= NewSavedOperationOn_DisplayMoreOperationInfo;
            displayingOperationSingle.OperationDeleted -= NewSavedOperationOperationDeleted;

            Destroy(displayingOperationSingle.gameObject);
        }

        _currentDisplayingSavedOperations.Clear();
    }

    #endregion

    #region Visual

    public void Show()
    {
        IsDisplaying = true;

        gameObject.SetActive(true);

        Displayed?.Invoke();
    }

    public void Hide()
    {
        Hidden?.Invoke();

        IsDisplaying = false;

        gameObject.SetActive(false);
    }

    public void TurnOnListLoadingVisuals()
    {
        _operationsLoadingTransform.gameObject.SetActive(true);
        _addCurrentOperationButton.gameObject.SetActive(false);
    }

    public void TurnOffListLoadingVisuals()
    {
        _operationsLoadingTransform.gameObject.SetActive(false);
        _addCurrentOperationButton.gameObject.SetActive(true);
    }

    #endregion
}