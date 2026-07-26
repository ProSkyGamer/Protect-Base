#region

using System;
using System.Collections.Generic;
using System.Linq;

#endregion

public class DynamicInjector
{
    public event Action<Type, object> InterfaceInjected;

    private readonly List<Type> _allCheckingInterfaces = new()
    {
        typeof(IOperationStatsDataProvider),
        typeof(ISceneResettable),
        typeof(IDevInterfaceListener),
        typeof(IDutyInterfaceListener),
        typeof(IOperationsStatusListener),
        typeof(IOperationUpdateListener),
        typeof(IPathPointCoordinatesListener)
    };

    public void InjectAllInterfacesFrom(object injectingObject)
    {
        List<Type> injectingTypes = _allCheckingInterfaces.Where(t => t.IsInstanceOfType(injectingObject)).ToList();

        foreach (Type matchingType in injectingTypes)
        {
            InterfaceInjected?.Invoke(matchingType, injectingObject);
        }
    }
}