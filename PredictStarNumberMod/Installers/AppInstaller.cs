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

            // すべてのModのEnable後に実行されるっぽいので、よほどのことが無ければこれで依存関係問題ないはず
            if (PluginManager.GetPlugin("HttpSiraStatus") == null) return;

            Plugin.Log?.Info("HttpSiraStatus found");
            Container.BindInterfacesAndSelfTo<HttpStatus>().AsSingle();
        }
    }
}