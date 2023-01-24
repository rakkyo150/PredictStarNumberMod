using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Newtonsoft.Json;
using PredictStarNumberMod.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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
        internal static BeatmapDifficulty difficulty = BeatmapDifficulty.Easy;
        internal static int difficultyRank = int.MinValue;
        internal static string characteristic = string.Empty;
        internal static MapDataGetter.MapData mapData = new MapDataGetter.MapData(float.MinValue, float.MinValue, int.MinValue,
            int.MinValue, float.MinValue, float.MinValue, int.MinValue, int.MinValue, int.MinValue, float.MinValue, int.MinValue,
            int.MinValue, int.MinValue, int.MinValue, int.MinValue);

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

            ___fields[1].fontSize = 4f;

            if (!PluginConfig.Instance.Enable) return;

            // データなし
            if (___fields[1].text == "?") return;

            if (!PluginConfig.Instance.Parallel && IsRankedMap(___fields)) return;

            string originalText = ___fields[1].text;
            bool isRankedMap = IsRankedMap(___fields);

            if (isRankedMap)
            {
                ___fields[1].text = originalText + "...";
            }
            else
            {
                ___fields[1].text = "...";
            }

            // 非同期で書き換えをする必要がある
            async Task wrapper(TextMeshProUGUI[] fields)
            {
                try
                {
                    string predictedStarNumber = await PredictStarNumber();// ランク
                    if (isRankedMap)
                    {
                        fields[1].text = originalText + $"({predictedStarNumber})";
                        Plugin.Log.Info(fields[1].fontSize.ToString());
                        fields[1].fontSize = 3.3f;
                        return;
                    }

                    fields[1].text = $"({predictedStarNumber})";
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error(ex);
                    fields[1].text = "Error";
                }
            }

            wrapper(___fields);


            async Task<string> PredictStarNumber()
            {

                StarNumberSetter.mapData = await MapDataGetter.GetMapData(mapHash, difficulty, characteristic);
                InferenceSession session = new InferenceSession("model.onnx");
                string inputNoneName = session.InputMetadata.First().Key;
                double[] data = new double[15]
                {
                    mapData.Bpm,
                    mapData.Duration,
                    mapData.Difficulty,
                    mapData.SageScore,
                    mapData.Njs,
                    mapData.Offset,
                    mapData.Notes,
                    mapData.Bombs,
                    mapData.Obstacles,
                    mapData.Nps,
                    mapData.Events,
                    mapData.Chroma,
                    mapData.Errors,
                    mapData.Warns,
                    mapData.Resets
                };
                var innodedims = session.InputMetadata.First().Value.Dimensions;
#if DEBUG
                Plugin.Log.Info(string.Join(", ",innodedims));
                Plugin.Log.Info(string.Join(". ", data));
#endif
                var inputTensor = new DenseTensor<double>(data, new int[] {1,15}, false);  // let's say data is fed into the Tensor objects
                List<NamedOnnxValue> inputs = new List<NamedOnnxValue>()
            {
                NamedOnnxValue.CreateFromTensor<double>(inputNoneName, inputTensor)
            };
                using (var results = session.Run(inputs))
                {
#if DEBUG
                    Plugin.Log.Info(string.Join(". ", results));
#endif
                    return results.First().AsTensor<double>()[0].ToString("0.00");
                }
            }
        }

        private static bool IsRankedMap(TextMeshProUGUI[] fields)
        {
            return Double.TryParse(fields[1].text, out _);
        }
    }
}