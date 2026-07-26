#region

using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

[Serializable]
public class AmmoBlockTypeUI
{
    public ShootingBlockType BlockType;
    public List<AmmoBlockUI> AllBlockTypeAmmoBlocks;
    public bool IsNumerationTowardCenter;
}

public class FiringMachineSingleUI : MonoBehaviour
{
    #region Variables & Refernces

    [SerializeField] private TextMeshProUGUI _firingMachineNumberText;
    [SerializeField] private Image _healthBarValueImage;
    [SerializeField] private Transform _deadFiringMachineMarker;
    [SerializeField] private Transform _currentActiveFiringMachineTransform;
    [SerializeField] private Color _activeTextColor;
    private Color _normalTextColor;

    [SerializeField] private Transform _ammoBlocksTransform;
    [SerializeField] private List<AmmoBlockTypeUI> _allAmmoBlockTypesUI;

    private IFiringMachineDataProvider _firingMachineDataProvider;
    private bool _isInitialized;

    #endregion

    #region Initialize

    [Inject]
    public void Construct(IFiringMachineDataProvider firingMachineDataProvider)
    {
        Initialize(firingMachineDataProvider);
    }

    private void Initialize(IFiringMachineDataProvider firingMachineDataProvider)
    {
        if (_isInitialized)
            return;

        _isInitialized = true;
        _firingMachineDataProvider = firingMachineDataProvider;

        _normalTextColor = _firingMachineNumberText.color;
        _firingMachineDataProvider.PovStatusChanged += CurrentFiringMachine_OnFiringMachineModeChanged;
        _firingMachineDataProvider.AmmoCountChanged += CurrentAmmoCountChanged;
        _firingMachineDataProvider.HealthChanged += CurrentFiringMachine_OnFiringMachineHealthChanged;
        _firingMachineDataProvider.ActiveChanged += FiringMachineDataProvider_OnActiveChanged;

        UpdateVisual();
    }

    private void FiringMachineDataProvider_OnActiveChanged()
    {
        UpdateActive();
    }

    private void CurrentFiringMachine_OnFiringMachineHealthChanged(int firingMachineNumber)
    {
        UpdateHealthBar();
    }

    private void CurrentAmmoCountChanged()
    {
        UpdateAmmo();
    }

    private void CurrentFiringMachine_OnFiringMachineModeChanged()
    {
        UpdateActive();
    }

    #endregion

    #region Visual

    private void UpdateVisual()
    {
        UpdateActive();
        UpdateAmmo();
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        float healthPercentage = _firingMachineDataProvider.ReadonlyHealthComponent.CurrentHealth /
                                 _firingMachineDataProvider.ReadonlyHealthComponent.MaxHealth;

        _healthBarValueImage.fillAmount = healthPercentage < 0 ? 0f : healthPercentage;
    }

    private void UpdateActive()
    {
        _firingMachineNumberText.text = _firingMachineDataProvider.FiringMachineNumber.ToString();
        _ammoBlocksTransform.gameObject.SetActive(_firingMachineDataProvider.CurrentPoVStatus);
        _deadFiringMachineMarker.gameObject.SetActive(_firingMachineDataProvider.ReadonlyHealthComponent.IsDestroyed);

        if (_firingMachineDataProvider.IsActive == false)
        {
            _currentActiveFiringMachineTransform.gameObject.SetActive(false);
            _firingMachineNumberText.color = _normalTextColor;
        }
        else
        {
            _currentActiveFiringMachineTransform.gameObject.SetActive(true);
            _firingMachineNumberText.color = _activeTextColor;
        }
    }

    private void UpdateAmmo()
    {
        foreach (AmmoBlockTypeUI ammoBlockTypeUI in _allAmmoBlockTypesUI)
        {
            int blockMaxAmmoCount = _firingMachineDataProvider.GetShootingBlockMaxAmmoCount(ammoBlockTypeUI.BlockType);
            int blockAmmoCount = _firingMachineDataProvider.GetShootingBlockAmmoCount(ammoBlockTypeUI.BlockType);

            int totalAmmoBlocks = ammoBlockTypeUI.AllBlockTypeAmmoBlocks.Count;
            int fullAmmoBlocks = blockAmmoCount / (blockMaxAmmoCount / totalAmmoBlocks);
            int notFullAmmoBlocks = totalAmmoBlocks - fullAmmoBlocks;

            if (ammoBlockTypeUI.IsNumerationTowardCenter)
            {
                notFullAmmoBlocks /= 2;

                for (int i = 0; i < ammoBlockTypeUI.AllBlockTypeAmmoBlocks.Count / 2; i++)
                {
                    AmmoBlockUI startingAmmoBlock = ammoBlockTypeUI.AllBlockTypeAmmoBlocks[i];

                    AmmoBlockUI endingAmmoBlock =
                        ammoBlockTypeUI.AllBlockTypeAmmoBlocks[ammoBlockTypeUI.AllBlockTypeAmmoBlocks.Count - 1 - i];

                    if (notFullAmmoBlocks - 1 >= i)
                    {
                        startingAmmoBlock.ShowNotFullAmmoBlock();
                        endingAmmoBlock.ShowNotFullAmmoBlock();
                    }
                    else
                    {
                        startingAmmoBlock.ShowFullAmmoBlock();
                        endingAmmoBlock.ShowFullAmmoBlock();
                    }
                }
            }
            else
            {
                for (int i = 0; i < ammoBlockTypeUI.AllBlockTypeAmmoBlocks.Count; i++)
                {
                    AmmoBlockUI ammoBlockUI = ammoBlockTypeUI.AllBlockTypeAmmoBlocks[i];

                    if (notFullAmmoBlocks - 1 >= i)
                        ammoBlockUI.ShowNotFullAmmoBlock();
                    else
                        ammoBlockUI.ShowFullAmmoBlock();
                }
            }
        }

        #endregion
    }
}