#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public enum SoldierAnimations
{
    GuardIdle,
    CombatIdle,
    Run,
    Shoot,
    Reload,
    Hit,
    HitWhileShooting,
    BreakingFence,
    Death
}

[Serializable]
public class AnimationParams
{
    public SoldierAnimations SoldierAnimation;
    public AnimationClip SoldierAnimationClip;
    public string SoldierAnimationParamString;
}

[RequireComponent(typeof(Animator))]
public class SoldierAnimatorController : EnemyAnimationController
{
    #region Variables & References

    [SerializeField] private List<AnimationParams> _allSoldierAnimationParams;
    private Animator _soldierAnimator;
    private readonly int _state = Animator.StringToHash("State");

    #endregion

    #region Initialization

    [Inject]
    public void Construct(Animator soldierAnimator)
    {
        _soldierAnimator = soldierAnimator;
    }

    #endregion

    #region Animations

    public void ChangeAnimation(SoldierAnimations newSoldierAnimation)
    {
        _soldierAnimator.SetInteger(_state, (int)newSoldierAnimation);
    }

    public void ChangeAnimationSpeed(SoldierAnimations soldierAnimation, float newAnimationSpeed)
    {
        foreach (AnimationParams soldierAnimationParam in _allSoldierAnimationParams)
        {
            if (soldierAnimationParam.SoldierAnimation != soldierAnimation)
                continue;

            if (soldierAnimationParam.SoldierAnimationParamString == "")
                break;

            _soldierAnimator.SetFloat(soldierAnimationParam.SoldierAnimationParamString, newAnimationSpeed);

            break;
        }
    }

    #endregion

    #region Get

    public float GetAnimationLength(SoldierAnimations soldierAnimation)
    {
        float animationLength = _allSoldierAnimationParams.Find(animationParams => animationParams.SoldierAnimation == soldierAnimation)
            ?.SoldierAnimationClip.length ?? -1f;

        return animationLength;
    }

    #endregion
}