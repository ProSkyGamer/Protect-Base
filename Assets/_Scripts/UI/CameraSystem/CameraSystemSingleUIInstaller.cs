#region

using UnityEngine;
using Zenject;

#endregion

public class CameraSystemSingleUIInstaller : MonoInstaller
{
    [SerializeField] private CameraSystemSingleUI _cameraSystemSingleUI;

    [Inject] private CameraSystemSingle _cameraSystemSingle;

    public override void InstallBindings()
    {
        Container.BindInstance(_cameraSystemSingle);
        Container.BindInterfacesAndSelfTo<CameraSystemSingleUI>().FromInstance(_cameraSystemSingleUI).AsSingle();
    }
}