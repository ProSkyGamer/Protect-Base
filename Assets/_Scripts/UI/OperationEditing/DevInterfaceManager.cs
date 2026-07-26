#region

using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

#endregion

public class DevInterfaceManager : IInitializable, ILateInitializable, IDisposable
{
    #region Events

    public event Action<bool> VisibilityChanged;

    #endregion

    #region Variables & References

    private readonly List<IDevInterface> _allDevInterfaces = new();

    private bool IsDevInterfaceOpen => _allDevInterfaces.Any(devInterface => devInterface.IsShown);

    #endregion

    #region Initialization

    [Inject]
    public void Construct(List<IDevInterface> allDevInterfaces)
    {
        _allDevInterfaces.AddRange(allDevInterfaces);
    }

    public void Initialize()
    {
        foreach (IDevInterface devInterface in _allDevInterfaces)
        {
            devInterface.VisibilityChanged += DevInterface_OnVisibilityChanged;
        }
    }

    public void LateInitialize()
    {
        VisibilityChanged?.Invoke(IsDevInterfaceOpen);
    }

    private void DevInterface_OnVisibilityChanged()
    {
        VisibilityChanged?.Invoke(IsDevInterfaceOpen);
    }

    #endregion

    public void Dispose()
    {
        foreach (IDevInterface devInterface in _allDevInterfaces)
        {
            devInterface.VisibilityChanged -= DevInterface_OnVisibilityChanged;
        }
    }
}