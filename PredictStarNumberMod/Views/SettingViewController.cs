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
            }
        }

        public bool DisplayBestPP
        {
            get { return PluginConfig.Instance.DisplayBestPP; }
            set
            {
                if (PluginConfig.Instance.DisplayBestPP == value) return;
                PluginConfig.Instance.DisplayBestPP = value;
                NotifyPropertyChanged();
            }
        }

        public bool DisplayNowPP
        {
            get { return PluginConfig.Instance.DisplayNowPP; }
            set
            {
                if (PluginConfig.Instance.DisplayNowPP == value) return;
                PluginConfig.Instance.DisplayNowPP = value;
                NotifyPropertyChanged();
            }
        }

        public bool DisplayValuesInRankMap
        {
            get { return PluginConfig.Instance.DisplayValuesInRankMap; }
            set
            {
                if (PluginConfig.Instance.DisplayValuesInRankMap == value) return;
                PluginConfig.Instance.DisplayValuesInRankMap = value;
                NotifyPropertyChanged();
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
