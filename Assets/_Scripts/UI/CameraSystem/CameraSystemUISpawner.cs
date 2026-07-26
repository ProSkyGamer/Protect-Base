public class CameraSystemUISpawner
{
    private readonly CameraSystemSingleUIFactory _cameraSystemSingleUIFactory;
    private readonly CameraSystemSingleUIObserverFactory _cameraSystemSingleUIObserverFactory;

    public CameraSystemUISpawner(CameraSystemSingleUIFactory cameraSystemSingleUIFactory,
        CameraSystemSingleUIObserverFactory cameraSystemSingleUIObserverFactory)
    {
        _cameraSystemSingleUIFactory = cameraSystemSingleUIFactory;
        _cameraSystemSingleUIObserverFactory = cameraSystemSingleUIObserverFactory;
    }

    public CameraSystemSingleUI Create(CameraSystemSingle cameraSystemSingle)
    {
        CameraSystemSingleUI cameraSystemSingleUI = _cameraSystemSingleUIFactory.Create(cameraSystemSingle);

        CameraSystemSingleObserver cameraSystemSingleUIObserver =
            _cameraSystemSingleUIObserverFactory.Create(cameraSystemSingleUI, cameraSystemSingle);

        return cameraSystemSingleUI;
    }
}