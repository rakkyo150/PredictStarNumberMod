using Newtonsoft.Json;
using PredictStarNumberMod.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace PredictStarNumberMod.Patches
{
    public class StarNumberSetter
    {
        internal static string mapHash = string.Empty;
        internal static string mapType = string.Empty;

        /// <summary>
        /// This code is run after the original code in MethodToPatch is run.
        /// </summary>
        /// <param name="__instance">The instance of ClassToPatch</param>
        /// <param name="arg1">The Parameter1Type arg1 that was passed to MethodToPatch</param>
        /// <param name="____privateFieldInClassToPatch">Reference to the private field in ClassToPatch named '_privateFieldInClassToPatch', 
        ///     added three _ to the beginning to reference it in the patch. Adding ref means we can change it.</param>
        // Postfixにパッチをあてているせいでパッチ当てられるPostfixの引数は取得できない模様
        static void Postfix(ref TextMeshProUGUI[] ___fields)
        {
            // IDifficultyBeatmap selectedDifficultyBeatmap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap;はNullになる
            // Resources.FindObjectsOfTypeAll<IDifficultyBeatmap>().FirstOrDefault();はUnityのObjectじゃないのでダメ

            if (!PluginConfig.Instance.Enable) return;

            // データなし
            if (___fields[1].text == "?") return;

            // ランク
            if (Double.TryParse(___fields[1].text, out _)) return;

            ___fields[1].text = "...";

            // 非同期で書き換えをする必要がある
            async void wrapper(TextMeshProUGUI[] fields)
            {
                string predictedStarNumber = await PredictStarNumber();
                // Plugin.Log.Info(predictedStarNumber);
                string showedStarNumber = $"({predictedStarNumber})";
                fields[1].text = showedStarNumber;
            }

            wrapper(___fields);


            async Task<string> PredictStarNumber()
            {
                string endpoint = $"https://predictstarnumber.herokuapp.com/api2/hash/{mapHash}";

                HttpClient client = new HttpClient();
                var response = await client.GetAsync(endpoint);
                string jsonString = await response.Content.ReadAsStringAsync();

                dynamic jsonDynamic = JsonConvert.DeserializeObject<dynamic>(jsonString);

                string rank = JsonConvert.SerializeObject(jsonDynamic[mapType]);

                return rank;
            }
        }
    }
}