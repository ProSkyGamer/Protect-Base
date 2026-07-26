#region

using UnityEngine;
using Zenject;

#endregion

public abstract class BaseUpdatableMarkerSingleUI : MonoBehaviour, IInitializable, IOperationUpdateListener, ISceneResettable
{
    #region Variables & References

    protected Transform FollowingObjectTransform;

    #endregion

    #region Initialization

    [Inject]
    public void Construct(Transform newFollowingObjectTransform)
    {
        FollowingObjectTransform = newFollowingObjectTransform;
    }

    public abstract void Initialize();

    public virtual void UpdateOperationsVisuals()
    {
        UpdateVisuals();
    }

    #endregion

    #region Visual

    protected abstract void UpdateVisuals();

    #endregion

    public abstract void OnSceneReset();
}