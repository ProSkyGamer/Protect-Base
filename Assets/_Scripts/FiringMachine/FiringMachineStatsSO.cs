#region

using System.Collections.Generic;
using UnityEngine;

#endregion

[CreateAssetMenu()]
public class FiringMachineStatsSO : ScriptableObject
{
    public float StateSwitchTime;
    public float MaxHealth = 300;

    public List<ShootingBlockType> AutoSelectingBlockTypes = new()
    {
        ShootingBlockType.Main,
        ShootingBlockType.ExplosiveOne,
        ShootingBlockType.ExplosiveTwo
    };

    public float MinExplosiveBlockAnglesAdditionalDistance = 5f;
    public float MaxExplosiveBlockVerticalAscensionAngle = 30f;
    public float ExplBlockMinDistance = 0f;
    public float ExplBlockMaxDistance = 250f;
    public float ExplBlockStepDistance = 5f;
    public float ExplBlockBigStepDistance = 50f;

    public float HorizontalAdditionalAngle = 120f;
    public float VerticalNegativeAdditionalAngle = -45f;
    public float VerticalPositiveAdditionalAngle = 30f;
    public float MaxHorizontalRotationSpeedPerSecond = 15f;
    public float MaxVerticalRotationSpeedPerSecond = 7.5f;

    public List<float> AllCameraZoomLevelFieldOfViews;
    public int BaseCameraZoomLevel = 2;
}