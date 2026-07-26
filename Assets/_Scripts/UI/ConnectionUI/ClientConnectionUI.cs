#region

using UnityEngine;

#endregion

public class ClientConnectionUI : MonoBehaviour
{
    #region Variables & References

    [SerializeField] private Transform _connectingToServerTransform;
    [SerializeField] private Transform _tryingToReconnectToServerTransform;

    #endregion

    #region Initialization

    public void ChangeConnectingPageState(bool isConnecting)
    {
        _connectingToServerTransform.gameObject.SetActive(isConnecting);
    }

    public void ChangeReconnectPageState(bool isReconnecting)
    {
        _tryingToReconnectToServerTransform.gameObject.SetActive(isReconnecting);
    }

    #endregion
}