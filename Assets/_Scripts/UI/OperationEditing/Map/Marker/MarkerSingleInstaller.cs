#region

using UnityEngine;
using Zenject;

#endregion

public class MarkerSingleInstaller : MonoInstaller
{
    [SerializeField] private MapMarkerSingleUI _markerSingleUI;
    [SerializeField] private MarkerAdditionalInfoButtonUI _markerAdditionalInfoButtonUI;
    [SerializeField] private BaseUpdatableMarkerSingleUI _baseUpdatableMarkerSingleUI;

    [Inject] private MarkerType _markerType;
    [Inject] private Transform _worldObject;

    public MarkerSingle MarkerSingle { get; private set; }

    public override void InstallBindings()
    {
        Container.Bind<MarkerType>().FromInstance(_markerType);
        Container.Bind<Transform>().FromInstance(_worldObject);

        Container.BindInterfacesAndSelfTo<MapMarkerSingleUI>().FromInstance(_markerSingleUI);

        if (_markerAdditionalInfoButtonUI != null)
            Container.BindInterfacesAndSelfTo<MarkerAdditionalInfoButtonUI>().FromInstance(_markerAdditionalInfoButtonUI);

        if (_baseUpdatableMarkerSingleUI != null)
            Container.BindInterfacesAndSelfTo<BaseUpdatableMarkerSingleUI>().FromInstance(_baseUpdatableMarkerSingleUI);

        MarkerSingle = new MarkerSingle(_markerSingleUI, _markerAdditionalInfoButtonUI);

        Container.Bind<MarkerSingle>().FromInstance(MarkerSingle);
    }
}