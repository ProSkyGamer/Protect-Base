#region

using UnityEngine;
using UnityEngine.UI;
using Zenject;

#endregion

public class MainPageUI : BasePageUI, IInitializable
{
    #region Variables & References

    [SerializeField] private Button _startButton;

    #endregion

    #region Initialization

    public void Initialize()
    {
        SubscribeToUIEvents();
    }

    private void SubscribeToUIEvents()
    {
        _startButton.onClick.AddListener(OnStartButtonPressed);
    }

    private void OnStartButtonPressed()
    {
        RequestHide();
    }

    #endregion
}