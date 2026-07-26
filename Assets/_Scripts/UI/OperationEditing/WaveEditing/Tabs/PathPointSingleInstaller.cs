#region

using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class PathPointSingleInstaller : MonoInstaller
{
    [SerializeField] private List<NumberInputFieldFilterUI> _allNumberInputFields;
    [SerializeField] private PathPointSingleUI _pathPointSingleUI;

    [Inject] private bool _isBlocked;
    [Inject] private ObjectLimits _objectLimits;
    [Inject] private int _pathPointIndex;

    public override void InstallBindings()
    {
        Container.Bind<bool>().FromInstance(_isBlocked);
        Container.Bind<ObjectLimits>().FromInstance(_objectLimits);
        Container.Bind<int>().FromInstance(_pathPointIndex);

        foreach (NumberInputFieldFilterUI numberInputField in _allNumberInputFields)
        {
            Container.BindInterfacesAndSelfTo<NumberInputFieldFilterUI>().FromInstance(numberInputField);
        }

        Container.BindInterfacesAndSelfTo<PathPointSingleUI>().FromInstance(_pathPointSingleUI).AsSingle();
    }
}