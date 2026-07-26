#region

using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public abstract class MarkerAdditionalInfo : MonoBehaviour, IInitializable
{
    #region Variables & References

    [SerializeField] protected MarkerPage _mainPage;
    [SerializeField] protected List<MarkerPage> _otherPages;

    private RectTransform _additionalInfoRectTransform;

    #endregion

    public virtual void Initialize()
    {
        _additionalInfoRectTransform = GetComponent<RectTransform>();

        Hide();
    }

    public abstract void Show(Transform followingTransform, Vector3 additionalTabPosition);

    public abstract void Hide();

    public abstract void UpdateVisuals();

    public Vector3 GetMarkerAdditionalInfoSize()
    {
        return _additionalInfoRectTransform.sizeDelta;
    }
}