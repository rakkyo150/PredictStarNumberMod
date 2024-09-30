using IPA.Loader;
using PredictStarNumberMod.HarmonyPatches;
using Zenject;

namespace PredictStarNumberMod.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SettingViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesTo<MapDataGetter>().AsSingle();
            Container.BindInterfacesTo<LevelStatsViewPatch>().AsSingle();

            if (PluginManager.GetPlugin("BetterSongList") == null)
            {
                Plugin.Log?.Info("BetterSongList Not Found");
                return;
            }

            Container.BindInterfacesTo<StarNumberSetter>().AsSingle();
        }
    }
}
