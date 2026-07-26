#region

using UnityEngine;

#endregion

public abstract class MarkerPage : MonoBehaviour
{
    public virtual void InitializePage(Transform followingObject)
    {
        UpdateVisuals();
    }

    public void Show()
    {
        gameObject.SetActive(true);

        UpdateVisuals();
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }

    public abstract void UpdateVisuals();
}