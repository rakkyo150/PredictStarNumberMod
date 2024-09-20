using PredictStarNumberMod.Configuration;
using PredictStarNumberMod.Map;
using SiraUtil.Affinity;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace PredictStarNumberMod.HarmonyPatches
{
    /// <summary>
    /// This patches ClassToPatch.MethodToPatch(Parameter1Type arg1, Parameter2Type arg2)
    /// </summary>
    public class MapDataGetter: IAffinity
    {
        private readonly MapDataContainer _mapDataContainer;
        
        public MapDataGetter(MapDataContainer mapDataContainer)
        {
            _mapDataContainer = mapDataContainer;
        }
        
        /// <summary>
        /// This code is run before the original code in MethodToPatch is run.
        /// </summary>
        /// <param name="__instance">The instance of ClassToPatch</param>
        /// <param name="arg1">The Parameter1Type arg1 that was passed to MethodToPatch</param>
        /// <param name="____privateFieldInClassToPatch">Reference to the private field in ClassToPatch named '_privateFieldInClassToPatch', 
        ///     added three _ to the beginning to reference it in the patch. Adding ref means we can change it.</param>
        // PrefixではIDifficultyBeatmapはNullなのでここで取得しないと無理
        [AffinityBefore(new string[] { "Kinsi55.BeatSaber.BetterSongList" })] // If another mod patches this method, apply this patch after the other mod's.
        [AffinityPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.RefreshContent))]
        [AffinityPostfix]
        protected void Postfix(StandardLevelDetailView __instance, BeatmapLevel ____beatmapLevel)
        {
            if (!PluginConfig.Instance.Enable) return;

            string mapHash = GetHashOfLevel(____beatmapLevel);
            _mapDataContainer.MapHash = mapHash;
            _mapDataContainer.BeatmapDifficulty = __instance.beatmapKey.difficulty;
            _mapDataContainer.Characteristic = __instance.beatmapKey.beatmapCharacteristic;

            // From BetterSongList.Util.BeatmapsUtil
            string GetHashOfLevel(BeatmapLevel level)
            {
                return level == null ? null : GetHashOfLevelId(level.levelID);
            }

            string GetHashOfLevelId(string id)
            {
                if (id.Length < 53)
                    return null;

                if (id[12] != '_') // custom_level_<hash, 40 chars>
                    return null;

                return id.Substring(13, 40);
            }
        }
    }
}