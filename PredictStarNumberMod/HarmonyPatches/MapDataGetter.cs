using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static IPA.Logging.Logger;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace PredictStarNumberMod.Patches
{
    /// <summary>
    /// This patches ClassToPatch.MethodToPatch(Parameter1Type arg1, Parameter2Type arg2)
    /// </summary>
    [HarmonyPatch(typeof(StandardLevelDetailView), nameof(StandardLevelDetailView.RefreshContent))]
    public class MapDataGetter
    {
        /// <summary>
        /// This code is run before the original code in MethodToPatch is run.
        /// </summary>
        /// <param name="__instance">The instance of ClassToPatch</param>
        /// <param name="arg1">The Parameter1Type arg1 that was passed to MethodToPatch</param>
        /// <param name="____privateFieldInClassToPatch">Reference to the private field in ClassToPatch named '_privateFieldInClassToPatch', 
        ///     added three _ to the beginning to reference it in the patch. Adding ref means we can change it.</param>
        // PrefixではIDifficultyBeatmapはNullなのでここで取得しないと無理
        [HarmonyBefore(new string[] { "Kinsi55.BeatSaber.BetterSongList" })] // If another mod patches this method, apply this patch after the other mod's.
        static void Postfix(IDifficultyBeatmap ____selectedDifficultyBeatmap)
        {
            string hash = GetHashOfPreview(____selectedDifficultyBeatmap.level);
            string mapType = ____selectedDifficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName
                + "-" + ____selectedDifficultyBeatmap.difficulty.ToString();

            // Plugin.Log.Info(hash);
            // Plugin.Log.Info(mapType);

            MapDataDeliverer.Instance.SetHash(hash);
            MapDataDeliverer.Instance.SetMapType(mapType);

            // From BetterSongList.Util.BeatmapsUtil
            string GetHashOfPreview(IPreviewBeatmapLevel preview)
            {
                if (preview.levelID.Length < 53)
                    return null;

                if (preview.levelID[12] != '_') // custom_level_<hash, 40 chars>
                    return null;

                return preview.levelID.Substring(13, 40);
            }
        }
    }
}