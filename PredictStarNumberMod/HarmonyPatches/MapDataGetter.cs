using HarmonyLib;
using HarmonyLib.Tools;
using Newtonsoft.Json;
using PredictStarNumberMod.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;
using static PredictStarNumberMod.Patches.MapDataGetter;

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
        internal class MapData
        {
            float bpm;
            float duration;
            int difficulty;
            int sageScore;
            float njs;
            float offset;
            int notes;
            int bombs;
            int obstacles;
            float nps;
            int events;
            int chroma;
            int errors;
            int warns;
            int resets;

            internal float Bpm => bpm;
            internal float Duration => duration;
            internal int Difficulty => difficulty;
            internal int SageScore => sageScore;
            internal float Njs => njs;
            internal float Offset => offset;
            internal int Notes => notes;
            internal int Bombs => bombs;
            internal int Obstacles => obstacles;
            internal float Nps => nps;
            internal int Events => events;
            internal int Chroma => chroma;
            internal int Errors => errors;
            internal int Warns => warns;
            internal int Resets => resets;

            internal MapData(float bpm, float duration, int difficulty, int sageScore, float njs, float offset,
                int notes, int bombs, int obstacles, float nps, int events, int chroma, int errors,
                int warns, int resets)
            {
                this.bpm = bpm;
                this.duration = duration;
                this.difficulty = difficulty;
                this.sageScore = sageScore;
                this.njs = njs;
                this.offset = offset;
                this.notes = notes;
                this.bombs = bombs;
                this.obstacles = obstacles;
                this.nps = nps;
                this.events = events;
                this.chroma = chroma;
                this.errors = errors;
                this.warns = warns;
                this.resets = resets;
            }
        }

        internal static async Task<MapData> GetMapData(string hash, BeatmapDifficulty beatmapDifficulty ,string characteristic)
        {
            string endpoint = $"https://api.beatsaver.com/maps/hash/{hash}";
            
            float bpm = float.MinValue;
            float duration = float.MinValue;
            int difficulty = int.MinValue;
            int sageScore = int.MinValue;
            float njs = float.MinValue;
            float offset = float.MinValue;
            int notes = int.MinValue;
            int bombs = int.MinValue;
            int obstacles = int.MinValue;
            float nps = float.MinValue;
            int events = int.MinValue;
            int chroma = int.MinValue;
            int errors = int.MinValue;
            int warns = int.MinValue;
            int resets = int.MinValue;

            HttpClient client = new HttpClient();
            var response = await client.GetAsync(endpoint);
            string jsonString = await response.Content.ReadAsStringAsync();
#if DEBUG
            Plugin.Log.Info(jsonString);
#endif

            dynamic mapDetail = JsonConvert.DeserializeObject<dynamic>(jsonString);

            dynamic versions = mapDetail["versions"];
            dynamic mapDifficulty = versions[versions.Count-1]["diffs"];

            foreach (var eachDifficulty in mapDifficulty)
            {
                if (eachDifficulty["difficulty"] != beatmapDifficulty.ToString()
                    || eachDifficulty["characteristic"] != characteristic)
                {
                    continue;
                }

                bpm = mapDetail["metadata"]["bpm"];
                duration = mapDetail["metadata"]["duration"];
                difficulty = (int)beatmapDifficulty;
#if DEBUG
                Plugin.Log.Info("Difficulty : " + difficulty.ToString());
#endif
                if (versions[versions.Count - 1]["sageScore"] != null)
                {
                    sageScore = versions[versions.Count - 1]["sageScore"];
                }
                else
                {
                    sageScore = 0;
                }
                njs = eachDifficulty["njs"];
                offset = eachDifficulty["offset"];
                notes = eachDifficulty["notes"];
                bombs = eachDifficulty["bombs"];
                obstacles = eachDifficulty["obstacles"];
                nps = eachDifficulty["nps"];
                characteristic = eachDifficulty["characteristic"];
                events = eachDifficulty["events"];
                chroma = eachDifficulty["chroma"]==true ? 1 : 0;
                errors = eachDifficulty["paritySummary"]["errors"];
                warns = eachDifficulty["paritySummary"]["warns"];
                resets = eachDifficulty["paritySummary"]["resets"];
                Plugin.Log.Info(resets.ToString());
                break;
            }

            return new MapData(bpm, duration, difficulty, sageScore, njs, offset, notes, bombs, obstacles,
                nps, events, chroma, errors, warns, resets);
        }

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
            if (!PluginConfig.Instance.Enable) return;

            string mapHash = GetHashOfPreview(____selectedDifficultyBeatmap.level);
            StarNumberSetter.mapHash = mapHash;
            StarNumberSetter.difficulty = ____selectedDifficultyBeatmap.difficulty;
            StarNumberSetter.difficultyRank = ____selectedDifficultyBeatmap.difficultyRank;
            StarNumberSetter.characteristic = ____selectedDifficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName;

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