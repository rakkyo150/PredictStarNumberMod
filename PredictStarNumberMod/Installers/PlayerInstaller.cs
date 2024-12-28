using IPA.Loader;
using PredictStarNumberMod.Overlay;
using Zenject;

namespace PredictStarNumberMod.Installers
{
    internal class PlayerInstaller : Installer
    {
        public override void InstallBindings()
        {
            // すべてのModのEnable後に実行されるっぽいので、よほどのことが無ければこれで依存関係問題ないはず
            if (PluginManager.GetPlugin("HttpSiraStatus") == null)
            {
                Plugin.Log?.Info("HttpSiraStatus Not Found");
                return;
            }
            Container.BindInterfacesAndSelfTo<NowPP>().AsSingle();
        }
    }
}
