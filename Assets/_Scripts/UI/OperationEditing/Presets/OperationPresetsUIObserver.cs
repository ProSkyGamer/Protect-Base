#region

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Zenject;

#endregion

public class OperationPresetsUIObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly string _enterNameNotificationText = "Введите имя \nпод которым будет \nсохранена операция";

    private readonly AllOperationPresetsListUI _allOperationPresetsListUI;
    private readonly CurrentEditingOperationManager _currentEditingOperationManager;
    private readonly InputFieldNotificationUI _inputFieldNotificationUI;
    private readonly TemporaryNotificationsManagerUI _temporaryNotificationsManagerUI;
    private readonly OperationPresetsManager _operationPresetsManager;
    private readonly SelectedOperationPresetUI _selectedOperationPresetUI;

    #endregion

    #region Initialization

    public OperationPresetsUIObserver(AllOperationPresetsListUI allOperationPresetsListUI,
        CurrentEditingOperationManager currentEditingOperationManager, InputFieldNotificationUI inputFieldNotificationUI,
        TemporaryNotificationsManagerUI temporaryNotificationsManagerUI, OperationPresetsManager operationPresetsManager,
        SelectedOperationPresetUI selectedOperationPresetUI)
    {
        _allOperationPresetsListUI = allOperationPresetsListUI;
        _currentEditingOperationManager = currentEditingOperationManager;
        _inputFieldNotificationUI = inputFieldNotificationUI;
        _temporaryNotificationsManagerUI = temporaryNotificationsManagerUI;
        _operationPresetsManager = operationPresetsManager;
        _selectedOperationPresetUI = selectedOperationPresetUI;
    }

    public void Initialize()
    {
        _allOperationPresetsListUI.CurrentOperationAdded += AllOperationPresetsListUI_OnCurrentOperationAdded;
        _allOperationPresetsListUI.OperationDeleted += AllOperationPresetsListUI_OnOperationDeleted;
        _allOperationPresetsListUI.SelectedOperationChanged += AllOperationPresetsListUI_OnSelectedOperationChanged;
        _allOperationPresetsListUI.Hidden += AllOperationPresetsListUI_OnHidden;
        _allOperationPresetsListUI.Displayed += AllOperationPresetsListUI_OnDisplayed;
        _allOperationPresetsListUI.OperationSelected += AllOperationPresetsListUI_OnOperationSelected;

        _selectedOperationPresetUI.OperationSelected += SelectedOperationPresetUI_OnOperationSelected;
        _selectedOperationPresetUI.OperationRewritten += SelectedOperationPresetUI_OnOperationRewritten;

        _allOperationPresetsListUI.ClearAllSavedOperations();
        _allOperationPresetsListUI.Hide();

        _inputFieldNotificationUI.HideNotification();
    }

    private void AllOperationPresetsListUI_OnOperationSelected(SavedOperationData operationData)
    {
        _currentEditingOperationManager.SetCurrentEditingOperationSingle(operationData.OperationData);

        _allOperationPresetsListUI.Hide();
        _selectedOperationPresetUI.Hide();
    }

    private void AllOperationPresetsListUI_OnDisplayed()
    {
        UpdateAllOperationPresetsList().Forget();
    }

    private void AllOperationPresetsListUI_OnHidden()
    {
        _selectedOperationPresetUI.Hide();
    }

    private void AllOperationPresetsListUI_OnSelectedOperationChanged(SavedOperationData savedOperationData)
    {
        _selectedOperationPresetUI.Show(savedOperationData);
    }

    private void AllOperationPresetsListUI_OnOperationDeleted(int deletingOperationIndex)
    {
        _operationPresetsManager.RemoveOperationSingle(deletingOperationIndex);

        UpdateAllOperationPresetsList().Forget();
    }

    private void SelectedOperationPresetUI_OnOperationSelected(SavedOperationData operationData)
    {
        _currentEditingOperationManager.SetCurrentEditingOperationSingle(operationData.OperationData);

        _allOperationPresetsListUI.Hide();
        _selectedOperationPresetUI.Hide();
    }

    private void SelectedOperationPresetUI_OnOperationRewritten(int operationIndex, string operationName)
    {
        _operationPresetsManager.EditOperationSingle(
            _currentEditingOperationManager.GetCurrentEditingOperationSingle(), operationIndex, operationName);

        UpdateAllOperationPresetsList().Forget();
    }

    private void AllOperationPresetsListUI_OnCurrentOperationAdded()
    {
        if (_currentEditingOperationManager.GetTotalCurrentOperationWavesCount() <= 0)
        {
            _temporaryNotificationsManagerUI.AddNewNotification("Текущая операция пустая!");

            return;
        }

        DisplayNameNotification();
    }

    private async UniTaskVoid UpdateAllOperationPresetsList()
    {
        _allOperationPresetsListUI.ClearAllSavedOperations();
        _allOperationPresetsListUI.TurnOnListLoadingVisuals();

        List<SavedOperationData> savedOperationsSingle = await _operationPresetsManager.GetCurrentSavedOperationsAsync();

        _allOperationPresetsListUI.TurnOffListLoadingVisuals();

        _allOperationPresetsListUI.UpdateCurrentSavedOperationsList(savedOperationsSingle);
    }

    private void DisplayNameNotification()
    {
        _inputFieldNotificationUI.ShowNotification(_enterNameNotificationText);

        _inputFieldNotificationUI.NotificationConfirmed += InputFieldNotificationUIOnNotificationConfirmed;
        _inputFieldNotificationUI.NotificationCanceled += InputFieldNotificationUIOnNotificationCanceled;
    }

    private void InputFieldNotificationUIOnNotificationConfirmed(string inputFieldText)
    {
        _operationPresetsManager.AddOperationSingle(
            _currentEditingOperationManager.GetCurrentEditingOperationSingle(), inputFieldText);

        UpdateAllOperationPresetsList().Forget();

        HideNameNotification();
    }

    private void InputFieldNotificationUIOnNotificationCanceled()
    {
        HideNameNotification();
    }

    private void HideNameNotification()
    {
        _inputFieldNotificationUI.HideNotification();

        _inputFieldNotificationUI.NotificationConfirmed -= InputFieldNotificationUIOnNotificationConfirmed;
        _inputFieldNotificationUI.NotificationCanceled -= InputFieldNotificationUIOnNotificationCanceled;
    }

    public void Dispose()
    {
        _allOperationPresetsListUI.CurrentOperationAdded -= AllOperationPresetsListUI_OnCurrentOperationAdded;
        _allOperationPresetsListUI.OperationDeleted -= AllOperationPresetsListUI_OnOperationDeleted;
        _allOperationPresetsListUI.SelectedOperationChanged -= AllOperationPresetsListUI_OnSelectedOperationChanged;
        _allOperationPresetsListUI.Hidden -= AllOperationPresetsListUI_OnHidden;
        _allOperationPresetsListUI.Displayed -= AllOperationPresetsListUI_OnDisplayed;

        _selectedOperationPresetUI.OperationSelected -= SelectedOperationPresetUI_OnOperationSelected;
        _selectedOperationPresetUI.OperationRewritten -= SelectedOperationPresetUI_OnOperationRewritten;
    }

    #endregion
}