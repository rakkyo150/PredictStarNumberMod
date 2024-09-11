using PredictStarNumberMod.Overlay;
using Zenject;

namespace PredictStarNumberMod.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<HttpStatus>().AsSingle();
        }
    }
}