#region

using UnityEngine;

#endregion

public class MainAmmoBlockUI : AmmoBlockUI
{
    [SerializeField] private Transform _fullAmmoBlock;
    [SerializeField] private Transform _notFullAmmoBlock;

    public override void ShowFullAmmoBlock()
    {
        _fullAmmoBlock.gameObject.SetActive(true);
        _notFullAmmoBlock.gameObject.SetActive(false);
    }

    public override void ShowNotFullAmmoBlock()
    {
        _notFullAmmoBlock.gameObject.SetActive(true);
        _fullAmmoBlock.gameObject.SetActive(false);
    }
}