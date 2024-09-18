using CountersPlus.Installers;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using IPALogger = IPA.Logging.Logger;

namespace PredictStarNumberCounter
{
    [Plugin(RuntimeOptions.SingleStartInit)]
    public class Plugin
    {
        internal static Plugin Instance { get; private set; }
        internal static IPALogger Log { get; private set; }

        [Init]
        /// <summary>
        /// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
        /// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
        /// Only use [Init] with one Constructor.
        /// </summary>
        public void Init(IPALogger logger, Config config)
        {
            Instance = this;
            Log = logger;
            Log.Info("PredictStarNumberCounter initialized.");
        }

        #region BSIPA Config
        //Uncomment to use BSIPA's config
        [Init]
        public void InitWithConfig(Config conf)
        {
            Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
            Plugin.Log?.Debug("Config loaded");
        }
        #endregion

        [OnEnable]
        public void OnEnable() { }

        [OnDisable]
        public void OnDisable() { }
    }
}
