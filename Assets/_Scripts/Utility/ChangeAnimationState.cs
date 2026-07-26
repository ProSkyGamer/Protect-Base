#region

using UnityEngine;

#endregion

public class ChangeAnimationState : MonoBehaviour
{
    #region Variables & References

    private Animator animator;

    [SerializeField] private int newAnimationStateIndex;
    private static readonly int state = Animator.StringToHash("State");

    #endregion

    #region Initialization

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetInteger(state, newAnimationStateIndex);
        Debug.Log(animator.GetInteger(state));
    }

    #endregion
}