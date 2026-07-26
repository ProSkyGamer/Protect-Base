#region

using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class AmmoPageUI : BasePageUI, IInitializable
{
    #region Events

    public event Action<int, ShootingBlockType, AmmoType> AmmoTypeChanged;

    public event Action AllAmmoTypesReset;

    #endregion

    #region Variables & References

    [SerializeField] private TMP_Dropdown _firingMachineNumberDropdown;
    private readonly Dictionary<int, int> _firingMachineDropdownOptionToNumber = new();
    [SerializeField] private TMP_Dropdown _blockOneAmmoTypeDropdown;
    [SerializeField] private TMP_Dropdown _blockTwoAmmoTypeDropdown;
    [SerializeField] private List<AmmoType> _blocksAmmoTypeExceptions;
    [SerializeField] private Button _applyButton;
    [SerializeField] private Button _resetAllButton;
    [SerializeField] private Button _quitButton;
    [SerializeField] private Transform _firingMachineAmmoTypesList1;
    [SerializeField] private Transform _firingMachineAmmoTypesList2;
    [SerializeField] private int _maxDisplayingComponentsPerList = 8;

    private readonly List<FiringMachineAmmoTypesUI> _allFiringMachineAmmoTypes = new();

    private int _currentPseudoSelectedFiringMachineNumber = -1;

    private EnumTranslationValuesSO _enumTranslationValuesSO;
    private IAllFiringMachineInfoProvider _allFiringMachineInfoProvider;
    private FiringMachineAmmoTypesUIFactory _firingMachineAmmoTypesUIFactory;

    public override bool IsCanHide => true;

    #endregion

    #region Initialize

    [Inject]
    public void Construct(EnumTranslationValuesSO enumTranslationValuesSO,
        IAllFiringMachineInfoProvider allFiringMachineInfoProvider,
        FiringMachineAmmoTypesUIFactory firingMachineAmmoTypesUIFactory)
    {
        _enumTranslationValuesSO = enumTranslationValuesSO;
        _allFiringMachineInfoProvider = allFiringMachineInfoProvider;
        _firingMachineAmmoTypesUIFactory = firingMachineAmmoTypesUIFactory;
    }

    public async void Initialize()
    {
        await InitializeDropdownValues();
        SubscribeToUIEvents();
    }

    private async UniTask InitializeDropdownValues()
    {
        ClearDropdowns();

        InitializeAmmoTypeDropdowns();

        await InitializeFiringMachineNumbersDropdown();
    }

    private async UniTask InitializeFiringMachineNumbersDropdown()
    {
        List<IFiringMachineDataProvider> allFiringMachineData = await _allFiringMachineInfoProvider.GetAllDataProviders();

        List<IFiringMachineDataProvider> allFiringMachinesNumbers = allFiringMachineData
            .OrderBy(firingMachine => firingMachine.FiringMachineNumber).ToList();

        for (int i = 0; i < allFiringMachinesNumbers.Count; i++)
        {
            IFiringMachineDataProvider firingMachineSingle = allFiringMachinesNumbers[i];

            int firingMachineNumber = firingMachineSingle.FiringMachineNumber;
            _firingMachineDropdownOptionToNumber.Add(i, firingMachineNumber);

            _firingMachineNumberDropdown.options.Add(
                new TMP_Dropdown.OptionData(firingMachineNumber.ToString()));
        }
    }

    private void InitializeAmmoTypeDropdowns()
    {
        List<TMP_Dropdown.OptionData> ammoTypesOptions = Enum.GetValues(typeof(AmmoType)).Cast<AmmoType>().Select(ammoType =>
            new TMP_Dropdown.OptionData(
                _enumTranslationValuesSO.GetFiringMachineAmmoTypeString(ammoType))).ToList();

        _blockOneAmmoTypeDropdown.AddOptions(ammoTypesOptions);
        _blockTwoAmmoTypeDropdown.AddOptions(ammoTypesOptions);
    }

    private void ClearDropdowns()
    {
        _firingMachineNumberDropdown.options.Clear();
        _blockOneAmmoTypeDropdown.options.Clear();
        _blockTwoAmmoTypeDropdown.options.Clear();
    }

    private void SubscribeToUIEvents()
    {
        _applyButton.onClick.AddListener(OnApplyButtonPressed);
        _resetAllButton.onClick.AddListener(OnResetAllButtonPressed);
        _quitButton.onClick.AddListener(OnQuitButtonPressed);
        _firingMachineNumberDropdown.onValueChanged.AddListener(OnFiringMachineDropdownValueChanged);
    }

    private void OnApplyButtonPressed()
    {
        int firingMachineNumber = _firingMachineDropdownOptionToNumber[_firingMachineNumberDropdown.value];

        AmmoType blockOneAmmoType = (AmmoType)_blockOneAmmoTypeDropdown.value;
        AmmoType blockTwoAmmoType = (AmmoType)_blockTwoAmmoTypeDropdown.value;

        AmmoTypeChanged?.Invoke(firingMachineNumber, ShootingBlockType.ExplosiveOne, blockOneAmmoType);
        AmmoTypeChanged?.Invoke(firingMachineNumber, ShootingBlockType.ExplosiveTwo, blockTwoAmmoType);
    }

    private void OnResetAllButtonPressed()
    {
        AllAmmoTypesReset?.Invoke();
    }

    private void OnQuitButtonPressed()
    {
        RequestHide();
    }

    private void OnFiringMachineDropdownValueChanged(int newChangedOption)
    {
        if (_allFiringMachineAmmoTypes.Count <= newChangedOption)
            return;

        _currentPseudoSelectedFiringMachineNumber = _firingMachineDropdownOptionToNumber[newChangedOption];

        PseudoSelectElement(_currentPseudoSelectedFiringMachineNumber).Forget();
    }

    private async UniTaskVoid PseudoSelectElement(int selectingFiringMachineNumber)
    {
        if (selectingFiringMachineNumber == -1)
            selectingFiringMachineNumber = _firingMachineDropdownOptionToNumber[_firingMachineNumberDropdown.value];

        FiringMachineAmmoTypesUI selectingFiringMachineAmmoTypes =
            _allFiringMachineAmmoTypes.Find(firingMachineAmmoType =>
                firingMachineAmmoType.LinkedFiringMachineNumber == selectingFiringMachineNumber);

        if (selectingFiringMachineAmmoTypes == null)
            return;

        SelectedUIItemController.ActivatePseudoSelection(selectingFiringMachineAmmoTypes);

        List<IFiringMachineDataProvider> allFiringMachineData =
            await _allFiringMachineInfoProvider.GetAllDataProviders();

        IFiringMachineDataProvider currentFiringMachine =
            allFiringMachineData.First(firingMachineData =>
                firingMachineData.FiringMachineNumber == selectingFiringMachineNumber);

        _blockOneAmmoTypeDropdown.value =
            (int)currentFiringMachine.GetShootingBlockAmmoType(ShootingBlockType.ExplosiveOne);

        _blockTwoAmmoTypeDropdown.value =
            (int)currentFiringMachine.GetShootingBlockAmmoType(ShootingBlockType.ExplosiveTwo);
    }

    #endregion

    #region Visual

    public override void Show()
    {
        base.Show();

        _firingMachineNumberDropdown.value = 0;
    }

    public override void Hide()
    {
        SelectedUIItemController.DeactivatePseudoSelection();

        base.Hide();
    }

    public async UniTaskVoid UpdateVisual()
    {
        foreach (Transform toDelete in _firingMachineAmmoTypesList1.GetComponentsInChildren<Transform>())
        {
            if (toDelete == _firingMachineAmmoTypesList1)
                continue;

            Destroy(toDelete.gameObject);
        }

        foreach (Transform toDelete in _firingMachineAmmoTypesList2.GetComponentsInChildren<Transform>())
        {
            if (toDelete == _firingMachineAmmoTypesList2)
                continue;

            Destroy(toDelete.gameObject);
        }

        _allFiringMachineAmmoTypes.Clear();

        List<IFiringMachineDataProvider> allDataProviders = await _allFiringMachineInfoProvider.GetAllDataProviders();

        foreach (IFiringMachineDataProvider firingMachineDataProvider in allDataProviders)
        {
            FiringMachineAmmoTypesUI displayingAmmoTypesUI =
                _firingMachineAmmoTypesUIFactory.Create(firingMachineDataProvider);

            Transform newAmmoTypesParent = _allFiringMachineAmmoTypes.Count >= _maxDisplayingComponentsPerList
                ? _firingMachineAmmoTypesList2
                : _firingMachineAmmoTypesList1;

            displayingAmmoTypesUI.transform.SetParent(newAmmoTypesParent);

            _allFiringMachineAmmoTypes.Add(displayingAmmoTypesUI);
        }

        PseudoSelectElement(_currentPseudoSelectedFiringMachineNumber).Forget();
    }

    #endregion
}