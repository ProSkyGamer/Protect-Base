#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#endregion

[CreateAssetMenu]
public class CameraStatsSO : ScriptableObject
{
    public float MaxTargetDistance = 400f;
    public List<VolumeProfile> AllFocusProfiles;
}