using BeatSaberMarkupLanguage.Attributes;
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
                NotifyPropertyChanged();

#if DEBUG
                Plugin.Log.Info("Enable has been changed");
#endif
            }
        }

        public bool DisplayValueInRankMap
        {
            get { return PluginConfig.Instance.DisplayValueInRankMap; }
            set
            {
                if (PluginConfig.Instance.DisplayValueInRankMap == value) return;
                PluginConfig.Instance.DisplayValueInRankMap = value;
                NotifyPropertyChanged();

#if DEBUG
                Plugin.Log.Info("DisplayValueInRankMap has been Changed");
#endif
            }
        }

        public bool Overlay
        {
            get { return PluginConfig.Instance.Overlay; }
            set
            {
                if (PluginConfig.Instance.Overlay == value) return;
                PluginConfig.Instance.Overlay = value;
                NotifyPropertyChanged();

#if DEBUG
                Plugin.Log.Info("Overlay has been Enable");
#endif
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
