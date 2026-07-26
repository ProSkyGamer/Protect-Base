#region

using UnityEngine;
using UnityEngine.UI;

#endregion

public class ContentSize : MonoBehaviour
{
    #region Variables

    [SerializeField] private bool _isUpdatingAutomatically;
    private VerticalLayoutGroup _verticalLayoutGroup;
    private HorizontalLayoutGroup _horizontalLayoutGroup;
    private RectTransform _rectTransform;

    #endregion

    #region Initialization

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
        TryGetComponent(out _verticalLayoutGroup);
        TryGetComponent(out _horizontalLayoutGroup);
    }

    #endregion

    #region Update Size

    private void Update()
    {
        if (_isUpdatingAutomatically == false)
            return;

        UpdateSize();
    }

    public void UpdateSize()
    {
        float horizontalSize = 0f;
        float verticalSize = 0f;

        if (_verticalLayoutGroup != null)
        {
            horizontalSize = _rectTransform.sizeDelta.x;

            foreach (RectTransform currentChildren in _rectTransform.GetComponentsInChildren<RectTransform>())
            {
                if (currentChildren == _rectTransform) continue;
                if (currentChildren.parent != transform) continue;

                verticalSize += currentChildren.sizeDelta.y;
            }
        }
        else if (_horizontalLayoutGroup != null)
        {
            verticalSize = _rectTransform.sizeDelta.y;

            foreach (RectTransform currentChildren in _rectTransform.GetComponentsInChildren<RectTransform>())
            {
                if (currentChildren == _rectTransform) continue;
                if (currentChildren.parent != transform) continue;

                horizontalSize += currentChildren.sizeDelta.x;
            }
        }

        _rectTransform.sizeDelta = new Vector2(horizontalSize, verticalSize);
    }

    #endregion
}