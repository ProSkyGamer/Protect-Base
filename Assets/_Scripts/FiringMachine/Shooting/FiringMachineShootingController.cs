#region

using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using Zenject;

#endregion

public class FiringMachineShootingController : NetworkBehaviour, ISceneResettable
{
    #region Events

    public event Action AmmoCountsUpdated;

    public event Action<bool> FiringModeStateChanged;

    public event Action<ShootingBlockType, ShootingType> ShotInitiated;

    #endregion

    #region Variables & References

    private readonly Dictionary<ShootingBlockType, BaseShootingBlock> _allShootingBlocks = new();
    private BaseShootingBlock _currentSelectedBaseShootingBlock;

    private bool _isFiringModeEnabled;

    private FiringMachineStatsSO _firingMachineStatsSO;

    #endregion

    #region Properties

    public AmmoType SelectedAmmoType => _currentSelectedBaseShootingBlock.CurrentAmmoType;

    public int CurrentAmmoCount => _currentSelectedBaseShootingBlock.CurrentAmmoCount;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(FiringMachineStatsSO firingMachineStatsSO)
    {
        _firingMachineStatsSO = firingMachineStatsSO;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        BaseShootingBlock[] allShootingBlocks = GetComponents<BaseShootingBlock>();

        foreach (BaseShootingBlock shootingBlock in allShootingBlocks)
        {
            _allShootingBlocks.Add(shootingBlock.ShootingBlockType, shootingBlock);
        }

        foreach (KeyValuePair<ShootingBlockType, BaseShootingBlock> shootingBlock in _allShootingBlocks)
        {
            shootingBlock.Value.ResetAmmoCount();
            shootingBlock.Value.AmmoCountChanged += ShootingBlock_OnAmmoCountChanged;
        }

        if (IsServer)
            SelectShootingBlockByAmmoCounts();
    }

    private void ShootingBlock_OnAmmoCountChanged()
    {
        if (_currentSelectedBaseShootingBlock.CurrentAmmoCount <= 0)
            SelectShootingBlockByAmmoCounts();

        AmmoCountsUpdated?.Invoke();
    }

    #endregion

    #region Shooting

    public void FiringModeToggle()
    {
        FiringModeToggleServerRpc();
    }

    [ServerRpc]
    private void FiringModeToggleServerRpc()
    {
        _isFiringModeEnabled = !_isFiringModeEnabled;

        FiringModeStateChanged?.Invoke(_isFiringModeEnabled);
    }

    public void Shoot(ShootingType shootingType)
    {
        if (_currentSelectedBaseShootingBlock == null)
            return;

        ShotInitiated?.Invoke(_currentSelectedBaseShootingBlock.ShootingBlockType, shootingType);

        ShootServerRpc(shootingType);
    }

    [ServerRpc]
    private void ShootServerRpc(ShootingType shootingType)
    {
        if (_currentSelectedBaseShootingBlock.CurrentAmmoCount <= 0)
            return;

        if (!_isFiringModeEnabled)
            return;

        if (_currentSelectedBaseShootingBlock.CurrentAmmoType is AmmoType.No)
            return;

        _currentSelectedBaseShootingBlock.Shoot(shootingType);
    }

    #endregion

    #region Shooting Block

    public void SelectShootingBlock(ShootingBlockType shootingBlockType)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game || IsServer == false)
            return;

        SelectShootingBlockServerRpc(shootingBlockType);
    }

    private void SelectShootingBlockByAmmoCounts()
    {
        foreach (ShootingBlockType autoSelectedType in _firingMachineStatsSO.AutoSelectingBlockTypes)
        {
            BaseShootingBlock autoSelectedBlock = _allShootingBlocks[autoSelectedType];

            if (autoSelectedBlock == null)
                continue;

            int blockAmmoCount = autoSelectedBlock.CurrentAmmoCount;

            if (blockAmmoCount != 0)
            {
                SelectShootingBlock(autoSelectedType);

                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SelectShootingBlockServerRpc(ShootingBlockType shootingBlockType)
    {
        SelectShootingBlockClientRpc(shootingBlockType);
    }

    [ClientRpc]
    private void SelectShootingBlockClientRpc(ShootingBlockType shootingBlockType)
    {
        BaseShootingBlock newSelectedBaseShootingBlock = _allShootingBlocks[shootingBlockType];

        if (newSelectedBaseShootingBlock == null)
            return;

        _currentSelectedBaseShootingBlock = newSelectedBaseShootingBlock;

        AmmoCountsUpdated?.Invoke();
    }

    #endregion

    #region Ammo Type

    public void ChangeAmmoType(ShootingBlockType shootingBlockType, AmmoType ammoType)
    {
        if (ClientTypeManager.CurrentClientType is not ClientType.Game)
            return;

        BaseShootingBlock changingBaseShootingBlock = _allShootingBlocks[shootingBlockType];

        if (changingBaseShootingBlock == null)
            return;

        changingBaseShootingBlock.SetAmmoType(ammoType);
    }

    #endregion

    #region Reload

    public void Reload()
    {
        foreach (ShootingBlockType shootingBlockSingle in _allShootingBlocks.Keys)
        {
            _allShootingBlocks[shootingBlockSingle].ResetAmmoCount();
        }
    }

    #endregion

    #region Get

    public List<ShootingBlockType> GetAllShootingBlockTypes()
    {
        return _allShootingBlocks.Select(shootingBlock => shootingBlock.Key).ToList();
    }

    public AmmoType GetShootingBlockAmmoType(ShootingBlockType shootingBlockType)
    {
        BaseShootingBlock baseShootingBlock = _allShootingBlocks[shootingBlockType];

        return baseShootingBlock?.CurrentAmmoType ?? AmmoType.No;
    }

    public int GetShootingBlockAmmoCount(ShootingBlockType shootingBlockType)
    {
        BaseShootingBlock baseShootingBlock = _allShootingBlocks[shootingBlockType];

        if (baseShootingBlock == null) return 0;

        return baseShootingBlock.CurrentAmmoCount;
    }

    public int GetShootingBlockMaxAmmoCount(ShootingBlockType shootingBlockType)
    {
        BaseShootingBlock baseShootingBlock = _allShootingBlocks[shootingBlockType];

        if (baseShootingBlock == null) return 0;

        return baseShootingBlock.MaxAmmoCount;
    }

    public bool IsShootingBlockSelected(ShootingBlockType shootingBlockType)
    {
        return _currentSelectedBaseShootingBlock.ShootingBlockType == shootingBlockType;
    }

    #endregion

    public void OnSceneReset()
    {
        foreach (ShootingBlockType shootingBlockType in _allShootingBlocks.Keys)
        {
            _allShootingBlocks[shootingBlockType].ResetAmmoCount();
        }

        SelectShootingBlockByAmmoCounts();

        _isFiringModeEnabled = false;
    }
}