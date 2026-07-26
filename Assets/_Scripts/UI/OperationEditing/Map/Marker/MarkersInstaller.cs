#region

using System.Collections.Generic;
using UnityEngine;
using Zenject;

#endregion

public class MarkersInstaller : MonoInstaller
{
    [SerializeField] private List<MarkerAdditionalInfo> _allMarkersInfo;
    [SerializeField] private List<MarkerPage> _allMarkerPages;
    [SerializeField] private List<MarkerPageButton> _allMarkerPagesButtons;

    public override void InstallBindings()
    {
        foreach (MarkerAdditionalInfo markerAdditionalInfo in _allMarkersInfo)
        {
            Container.BindInterfacesAndSelfTo<MarkerAdditionalInfo>().FromInstance(markerAdditionalInfo);
        }

        foreach (MarkerPage markerPage in _allMarkerPages)
        {
            Container.BindInterfacesAndSelfTo<MarkerPage>().FromInstance(markerPage);
        }

        foreach (MarkerPageButton markerPageButton in _allMarkerPagesButtons)
        {
            Container.BindInterfacesAndSelfTo<MarkerPageButton>().FromInstance(markerPageButton);
        }
    }
}