#region

using System;
using Zenject;

#endregion

public class OperatorPinsPageObserver : IInitializable, IDisposable
{
    #region Variables & References

    private readonly OperatorsPinsPageUI _operatorsPinsPageUI;
    private readonly OperatorsLoginManager _loginManager;

    #endregion

    #region Initialization

    public OperatorPinsPageObserver(OperatorsPinsPageUI operatorsPinsPageUI, OperatorsLoginManager loginManager)
    {
        _operatorsPinsPageUI = operatorsPinsPageUI;
        _loginManager = loginManager;
    }

    public void Initialize()
    {
        _operatorsPinsPageUI.PinChanged += OperatorsPinsPageUI_OnPinChanged;
    }

    private void OperatorsPinsPageUI_OnPinChanged(int operatorId, string pin)
    {
        _loginManager.ChangeOperatorPassword(operatorId, pin);
    }

    #endregion

    public void Dispose()
    {
        _operatorsPinsPageUI.PinChanged -= OperatorsPinsPageUI_OnPinChanged;
    }
}