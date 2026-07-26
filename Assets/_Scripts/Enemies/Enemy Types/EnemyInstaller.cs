#region

using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

#endregion

public class EnemyInstaller : MonoInstaller
{
    [SerializeField] private EnemyController _enemyController;
    [SerializeField] private EnemyAnimationController _enemyAnimationController;
    [SerializeField] private Animator _enemyAnimator;

    [Inject] private ReadonlyEnemyInitializationStats _enemyInitializationStats;

    public override void InstallBindings()
    {
        List<Type> enemyBindingTypes = GetBindTypes(_enemyController, typeof(EnemyController));
        Container.Bind(enemyBindingTypes).FromInstance(_enemyController);

        if (_enemyAnimationController != null)
        {
            List<Type> enemyAnimationBindingTypes = GetBindTypes(_enemyAnimationController, typeof(EnemyAnimationController));
            Container.Bind(enemyAnimationBindingTypes).FromInstance(_enemyAnimationController);
        }

        Container.Bind<Animator>().FromInstance(_enemyAnimator);
        Container.Bind<ReadonlyEnemyInitializationStats>().FromInstance(_enemyInitializationStats);
    }

    private List<Type> GetBindTypes(Object bindingObject, Type baseObjectType)
    {
        Type objectType = bindingObject.GetType();
        List<Type> objectBindTypes = new List<Type>() { baseObjectType, objectType };
        objectBindTypes.AddRange(objectType.GetInterfaces());

        return objectBindTypes;
    }
}