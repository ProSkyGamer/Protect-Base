#region

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class SavedOperationSingleUI : MonoBehaviour
{
    #region Events

    public event Action<SavedOperationData> OperationChosen;

    public event Action<SavedOperationData> DisplayedMoreOperationInfo;

    public event Action<SavedOperationData> OperationDeleted;

    #endregion

    #region Variables & References

    [SerializeField] private TextMeshProUGUI _operationNameText;
    [SerializeField] private TextMeshProUGUI _operationWavesCountText;
    [SerializeField] private Button _chooseOperationButton;
    [SerializeField] private Button _deleteOperationButton;

    private SavedOperationData _savedOperationData;
    private Button _operationButton;

    #endregion

    #region Inititalization

    [Inject]
    public void Construct(SavedOperationData savedOperationData)
    {
        SetOperationData(savedOperationData);
    }

    private void SetOperationData(SavedOperationData savedOperationData)
    {
        _operationButton = GetComponent<Button>();

        _operationNameText.text = savedOperationData.OperationName;
        _operationWavesCountText.text = savedOperationData.OperationData.AllOperationWaves.Count.ToString();

        _savedOperationData = savedOperationData;

        _operationButton.onClick.AddListener(OperationButtonClicked);

        _chooseOperationButton.onClick.AddListener(ChooseOperationButtonClicked);

        _deleteOperationButton.onClick.AddListener(DeleteOperationButtonClicked);
    }

    private void DeleteOperationButtonClicked()
    {
        OperationDeleted?.Invoke(_savedOperationData);
    }

    private void ChooseOperationButtonClicked()
    {
        OperationChosen?.Invoke(_savedOperationData);
    }

    private void OperationButtonClicked()
    {
        DisplayedMoreOperationInfo?.Invoke(_savedOperationData);
    }

    #endregion
}