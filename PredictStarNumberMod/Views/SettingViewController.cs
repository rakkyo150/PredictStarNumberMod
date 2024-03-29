﻿using BeatSaberMarkupLanguage.Attributes;
using BeatSaberMarkupLanguage.Settings;
using BeatSaberMarkupLanguage.ViewControllers;
using PredictStarNumberMod.Configuration;
using Zenject;

namespace PredictStarNumberMod
{
    [HotReload(RelativePathToLayout = @"SettingViewController.bsml")]
    [ViewDefinition("PredictStarNumberMod.SettingViewController.bsml")]
    internal class SettingViewController : BSMLAutomaticViewController, IInitializable
    {
        private string ResourceName => string.Join(".", this.GetType().Namespace, this.GetType().Name);

        // UIValueアトリビュートもいらない
        public bool Enable
        {
            get { return PluginConfig.Instance.Enable; }
            set
            {
                if (PluginConfig.Instance.Enable == value) return;
                PluginConfig.Instance.Enable = value;
                Plugin.Log.Info("Enable");
                NotifyPropertyChanged();
            }
        }

        public bool Parallel
        {
            get { return PluginConfig.Instance.Parallel; }
            set
            {
                if (PluginConfig.Instance.Parallel == value) return;
                PluginConfig.Instance.Parallel = value;
                Plugin.Log.Info("Parallel Enable");
                NotifyPropertyChanged();
            }
        }

        public void Dispose()
        {
            if (PluginConfig.Instance != null) BSMLSettings.instance.RemoveSettingsMenu(this);
        }

        public void Initialize()
        {
            BSMLSettings.instance.AddSettingsMenu("<size=85%>PredictStarNumberMod</size>", ResourceName, this);
        }

        [UIAction("#post-parse")]
        internal void PostParse()
        {
            // Code to run after BSML finishes
        }
    }
}
