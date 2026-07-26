#region

using System;
using UnityEngine;

#endregion

public class WaitingConnectionUI : MonoBehaviour, IDevInterface
{
    public event Action VisibilityChanged;

    public bool IsShown { get; private set; }

    public void Show()
    {
        gameObject.SetActive(true);

        IsShown = true;
        VisibilityChanged?.Invoke();
    }

    public void Hide()
    {
        gameObject.SetActive(false);

        IsShown = false;
        VisibilityChanged?.Invoke();
    }
}