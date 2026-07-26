#region

using UnityEngine;
using Zenject;

#endregion

public class GameStatsInstaller : MonoInstaller
{
    [SerializeField] private FiringMachineStatsSO _firingMachineStatsSO;
    [SerializeField] private CameraStatsSO _cameraStatsSO;
    [SerializeField] private EnumTranslationValuesSO _enumTranslationValuesSO;
    [SerializeField] private StringFormatsSO _stringFormatsSO;

    public override void InstallBindings()
    {
        Container.Bind<FiringMachineStatsSO>().FromInstance(_firingMachineStatsSO);
        Container.Bind<CameraStatsSO>().FromInstance(_cameraStatsSO);
        Container.Bind<EnumTranslationValuesSO>().FromInstance(_enumTranslationValuesSO);
        Container.Bind<StringFormatsSO>().FromInstance(_stringFormatsSO);
    }
}