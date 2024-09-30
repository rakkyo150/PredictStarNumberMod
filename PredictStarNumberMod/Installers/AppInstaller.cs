using IPA.Loader;
using PredictStarNumberMod.Map;
using PredictStarNumberMod.Overlay;
using PredictStarNumberMod.PP;
using Zenject;

namespace PredictStarNumberMod.Installers
{
    internal class AppInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<CurveDownloader>().AsSingle();
            Container.Bind<MapDataContainer>().AsSingle();
            Container.Bind<Star.Star>().AsSingle();
            Container.Bind<Model.Model>().AsSingle();
            Container.Bind<PP.PP>().AsSingle();

            // すべてのModのEnable後に実行されるっぽいので、よほどのことが無ければこれで依存関係問題ないはず
            if (PluginManager.GetPlugin("HttpSiraStatus") == null)
            {
                Plugin.Log?.Info("HttpSiraStatus Not Found");
                return;
            }

            Container.BindInterfacesAndSelfTo<HttpStatus>().AsSingle();
        }
    }
}