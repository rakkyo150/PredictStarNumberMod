using PredictStarNumberMod.Overlay;
using Zenject;

namespace PredictStarNumberMod.Installers
{
    internal class PlayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<NowPP>().AsSingle();
        }
    }
}
