#region

using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class FiringMachineInstaller : MonoInstaller
{
    #region Variables & References

    [SerializeField] private FiringMachineController _firingMachineController;
    [SerializeField] private FiringMachineViewController _viewController;
    [SerializeField] private FiringMachineShootingController _shootingController;
    [SerializeField] private List<BaseShootingBlock> _allShootingBlocks;
    [SerializeField] private FiringMachineHealthController _healthController;
    [SerializeField] private FiringMachineVisuals _visualsController;

    [Inject] private FiringMachineStatsSO _firingMachineStatsSO;
    [Inject] private Transform _firingMachineSpawnTransform;
    [Inject] private int _firingMachineNumber;

    #endregion

    public override void InstallBindings()
    {
        Container.Bind<FiringMachineStatsSO>().FromInstance(_firingMachineStatsSO);
        Container.BindInterfacesAndSelfTo<FiringMachineObserver>().AsSingle().NonLazy();
        Container.BindInstance(_firingMachineNumber).AsSingle();
        Container.BindInstance(_firingMachineSpawnTransform).AsSingle();

        Container.BindInterfacesAndSelfTo<FiringMachineViewController>().FromInstance(_viewController).AsSingle();
        Container.BindInterfacesAndSelfTo<FiringMachineVisuals>().FromInstance(_visualsController).AsSingle();

        foreach (BaseShootingBlock shootingBlock in _allShootingBlocks)
        {
            Container.BindInterfacesAndSelfTo<BaseShootingBlock>().FromInstance(shootingBlock);
        }

        Container.BindInterfacesAndSelfTo<FiringMachineShootingController>().FromInstance(_shootingController)
            .AsSingle();

        Container.BindInterfacesAndSelfTo<FiringMachineHealthController>().FromInstance(_healthController).AsSingle();

        Container.BindInterfacesAndSelfTo<FiringMachineController>().FromInstance(_firingMachineController);
    }
}