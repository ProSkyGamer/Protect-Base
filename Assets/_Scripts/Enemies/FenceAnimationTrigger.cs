#region

using UnityEngine;

#endregion

public class FenceAnimationTrigger : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private EnemiesShortcutSingle _connectedShortcutSingle;
    private Animator _animator;

    #endregion

    #region Variables & References

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.enabled = true;
    }

    #endregion

    #region Animation

    public void OnTriggerNextFenceFall()
    {
        if (_connectedShortcutSingle == null)
        {
            Debug.Log("NO SHORTCUT CONNECTED TO FENCE");

            return;
        }

        _connectedShortcutSingle.OnTriggerNextFenceFall();
    }

    #endregion
}