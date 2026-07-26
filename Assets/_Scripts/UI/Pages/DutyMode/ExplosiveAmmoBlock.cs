#region

using UnityEngine;

#endregion

public class ExplosiveAmmoBlock : AmmoBlockUI
{
    [SerializeField] private Transform _ammoBlock;

    public override void ShowFullAmmoBlock()
    {
        _ammoBlock.gameObject.SetActive(true);
    }

    public override void ShowNotFullAmmoBlock()
    {
        _ammoBlock.gameObject.SetActive(false);
    }
}