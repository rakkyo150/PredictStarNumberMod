﻿using PredictStarNumberMod.HarmonyPatches;
using PredictStarNumberMod.PP;
using Zenject;

namespace PredictStarNumberMod.Installers
{
    internal class MenuInstaller : Installer
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<SettingViewController>().FromNewComponentAsViewController().AsSingle();
            Container.BindInterfacesAndSelfTo<PredictedStarNumberMonitor>().AsSingle();
            Container.BindInterfacesTo<LevelStatsViewPatch>().AsSingle();
        }
    }
}
