#region

using UnityEngine;

#endregion

public interface IShootingAnglesProvider
{
    public float TotalFlightDistance { get; }

    public bool IsCanFireExplosiveAmmo { get; }

    public Vector3 GetRotatedPoint(Vector3 rotatingPoint);
}