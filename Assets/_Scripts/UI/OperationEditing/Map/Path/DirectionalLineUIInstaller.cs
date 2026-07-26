#region

using UnityEngine;
using Zenject;

#endregion

public class DirectionalLineUIInstaller : MonoInstaller
{
    [SerializeField] private MapPointDirectionLineUI _directionLineUI;

    [Inject] private string _lineText;

    public override void InstallBindings()
    {
        Container.Bind<string>().FromInstance(_lineText);

        Container.BindInterfacesAndSelfTo<MapPointDirectionLineUI>().FromInstance(_directionLineUI);
    }
}