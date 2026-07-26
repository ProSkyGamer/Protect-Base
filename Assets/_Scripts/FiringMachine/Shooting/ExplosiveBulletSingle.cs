#region

using System.Collections.Generic;
using UnityEngine;

#endregion

public class ExplosiveBulletSingle : BulletSingle
{
    #region Variables & References

    [SerializeField] protected float _explosionRange = 2.5f;
    [SerializeField] private bool _isHasExplosionSmoke;
    [SerializeField] protected VFXManager.VFXType _explosiveBulletSmokeVFXType;

    #endregion

    #region Collide

    protected override List<IHaveHealth> GetFinalCollidingHealthObjects()
    {
        if (IsServer == false)
            return null;

        List<IHaveHealth> allHittingHealthObjects = new List<IHaveHealth>();

        Vector3 castPosition = transform.position;
        float castRadius = _explosionRange;

        Collider[] collidingRaycastHits = Physics.OverlapSphere(castPosition, castRadius);

        foreach (Collider raycastHit in collidingRaycastHits)
        {
            if (!raycastHit.transform.TryGetComponent(out BulletSingle _) &&
                raycastHit.transform.TryGetComponent(out IHaveHealth healthComponent))
                allHittingHealthObjects.Add(healthComponent);
        }

        return allHittingHealthObjects;
    }

    #endregion

    #region VFX

    public void ChangeExplosiveBulletSmokeVFXType(VFXManager.VFXType newExplosiveBulletSmokeVFXType)
    {
        if (!IsServer) return;

        _explosiveBulletSmokeVFXType = newExplosiveBulletSmokeVFXType;
    }

    #endregion

    #region Destroy

    protected override void OnBulletDestroy()
    {
        if (!IsServer) return;

        if (_isHasExplosionSmoke)
            VFXManager.Instance.CreateVFX(_explosiveBulletSmokeVFXType, 15f, transform.position, Vector3.zero);

        base.OnBulletDestroy();
    }

    #endregion
}