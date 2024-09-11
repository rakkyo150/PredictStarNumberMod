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
        internal static double predictedStarNumber = double.MinValue;

        internal static string mapHash = string.Empty;
        internal static BeatmapDifficulty difficulty = BeatmapDifficulty.Easy;
        internal static int difficultyRank = int.MinValue;
        internal static string characteristic = string.Empty;
        internal static MapDataGetter.MapData mapData = new MapDataGetter.MapData(float.MinValue, float.MinValue, int.MinValue,
            int.MinValue, float.MinValue, float.MinValue, int.MinValue, int.MinValue, int.MinValue, float.MinValue, int.MinValue,
            int.MinValue, int.MinValue, int.MinValue, int.MinValue);

        internal static Action ChangedPredictedStarNumber;

        private static string modelAssetEndpoint = "https://api.github.com/repos/rakkyo150/PredictStarNumberHelper/releases/latest";
        private static byte[] modelByte = new byte[] { 0 };
        private static InferenceSession session = null;

        private static float originalFontSize = float.MinValue;

        private static double errorStarNumber = -1.0;

        public class LatestRelease
        {
            public List<DownloadUrl> assets;
        }

        public class DownloadUrl
        {
            public string browser_download_url;
        }

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

            if (originalFontSize == float.MinValue)
            {
                originalFontSize = ___fields[1].fontSize;
            }

            if (___fields[1].fontSize != originalFontSize)
            {
                ___fields[1].fontSize = originalFontSize;
            }

            if (!PluginConfig.Instance.Enable) return;

            // データなし
            if (___fields[1].text == "?") return;

            if (!PluginConfig.Instance.DisplayValueInRankMap && IsRankedMap(___fields)) return;

            bool isRankedMap = IsRankedMap(___fields);

            if (isRankedMap)
            {
                ___fields[1].text += "...";
            }
            else
            {
                ___fields[1].text = "...";
            }

            wrapper(___fields);

            // 非同期で書き換えをする必要がある
            async Task wrapper(TextMeshProUGUI[] fields)
            {
                try
                {
                    predictedStarNumber = await PredictStarNumber(); // ランク
                    ChangedPredictedStarNumber?.Invoke();
                    
                    string predictedStarNumberString = predictedStarNumber.ToString("0.00");
#if DEBUG
                    Plugin.Log.Info(predictedStarNumberString);
#endif
                    if (isRankedMap)
                    {
                        SetPredictedStarNumberForRankedMap(fields, predictedStarNumberString);
                        return;
                    }

                    fields[1].text = $"({predictedStarNumberString})";
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error(ex);
                    predictedStarNumber = errorStarNumber;
                    if (isRankedMap)
                    {
                        SetPredictedStarNumberForRankedMap(fields, "Error");
                        fields[1].fontSize = 3.3f;
                        return;
                    }
                    fields[1].text = "(Error)";
                }
            }

            async Task<double> PredictStarNumber()
            {
#if DEBUG
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
#endif
                if (modelByte.Length == 1)
                {
                    modelByte = await GetModel();
                }

                StarNumberSetter.mapData = await MapDataGetter.GetMapData(mapHash, difficulty, characteristic);
                if (session == null)
                {
                    session = new InferenceSession(modelByte);
                }
                string inputNoneName = session?.InputMetadata.First().Key;
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
#if DEBUG
                var innodedims = session?.InputMetadata.First().Value.Dimensions;
                Plugin.Log.Info(string.Join(", ",innodedims));
                Plugin.Log.Info(string.Join(". ", data));
#endif
                var inputTensor = new DenseTensor<double>(data, new int[] { 1, data.Length }, false);  // let's say data is fed into the Tensor objects
                List<NamedOnnxValue> inputs = new List<NamedOnnxValue>()
                    {
                        NamedOnnxValue.CreateFromTensor<double>(inputNoneName, inputTensor)
                    };
                using (var results = session?.Run(inputs))
                {
#if DEBUG
                    Plugin.Log.Info(string.Join(". ", results));
                    sw.Stop();
                    Plugin.Log.Info(sw.Elapsed.ToString());
#endif
                    return results.First().AsTensor<double>()[0];
                }
            }

            async Task<byte[]> GetModel()
            {
                HttpClient client = new HttpClient();
                var request = new HttpRequestMessage(HttpMethod.Get, modelAssetEndpoint);
                request.Headers.Add("User-Agent", "C# App");
                var response = await client.SendAsync(request);
                string assetString = await response.Content.ReadAsStringAsync();
                LatestRelease latestRelease = JsonConvert.DeserializeObject<LatestRelease>(assetString);
                string modelDownloadUrl = latestRelease.assets[2].browser_download_url;
#if DEBUG
                Plugin.Log.Info(modelDownloadUrl);
#endif
                request = new HttpRequestMessage(HttpMethod.Get, modelDownloadUrl);
                request.Headers.Add("User-Agent", "C# App");
                var modelResponse = await client.SendAsync(request);
                client.Dispose();
                request.Dispose();
                return await modelResponse.Content.ReadAsByteArrayAsync();
            }
        }

        private static void SetPredictedStarNumberForRankedMap(TextMeshProUGUI[] fields, string predictedStarNumber)
        {
            fields[1].text = fields[1].text.Replace("...", "");
            // 初回は２回呼び出されるみたいなので
            if (!fields[1].text.Contains("(") && !fields[1].text.Contains(")"))
            {
                fields[1].text += $"({predictedStarNumber})";
            }
            fields[1].fontSize = 3.2f;
        }

        private static bool IsRankedMap(TextMeshProUGUI[] fields)
        {
            return Double.TryParse(fields[1].text, out _);
        }
    }
}