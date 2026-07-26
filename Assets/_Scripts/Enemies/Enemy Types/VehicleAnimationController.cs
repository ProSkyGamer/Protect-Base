#region

using UnityEngine;
using Zenject;

#endregion

public enum VehicleAnimations
{
    Move,
    Idle
}

[RequireComponent(typeof(Animator))]
public class VehicleAnimationController : EnemyAnimationController
{
    private Animator _vehicleAnimator;
    private static readonly int State = Animator.StringToHash("State");

    [Inject]
    public void Construct(Animator vehicleAnimator)
    {
        _vehicleAnimator = vehicleAnimator;
    }

    public void ChangeAnimation(VehicleAnimations newVehicleAnimations)
    {
        _vehicleAnimator.SetInteger(State, (int)newVehicleAnimations);
    }
}