using BetterSongList.HarmonyPatches.UI;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Newtonsoft.Json;
using PredictStarNumberMod.Configuration;
using PredictStarNumberMod.Map;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using TMPro;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace PredictStarNumberMod.HarmonyPatches
{
    public class StarNumberSetter : IAffinity
    {
        private readonly MapDataContainer _mapDataContainer;
        private readonly Star.Star _star;
        private readonly Model.Model _model;

        private float originalFontSize = float.MinValue;

        public StarNumberSetter(MapDataContainer mapDataContainer, Star.Star star, Model.Model model)
        {
            _mapDataContainer = mapDataContainer;
            _star = star;
            _model = model;
        }

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
        [AffinityPatch(typeof(ExtraLevelParams), nameof(ExtraLevelParams.Postfix))]
        [AffinityPostfix]
        // Postfixにパッチをあてているせいでパッチ当てられるPostfixの引数は取得できない模様
        protected void Postfix(ref TextMeshProUGUI[] ___fields)
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

            if (!PluginConfig.Instance.Enable)
            {
                if (_star.PredictedStarNumber == _star.SkipStarNumber) return;
                
                // In oreder to hide overlay
                _star.ChangePredictedStarNumber(_star.SkipStarNumber);
                return;
            }

            // データなし
            if (___fields[1].text == "?")
            {
                _star.ChangePredictedStarNumber(_star.SkipStarNumber);
                return;
            }

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
                    _star.ChangePredictedStarNumber(await PredictStarNumber());
                    
                    string predictedStarNumberString = _star.PredictedStarNumber.ToString("0.00");
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
                    _star.ChangePredictedStarNumber(_star.ErrorStarNumber);
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
                if (_model.ModelByte.Length == 1)
                {
                    _model.ModelByte = await GetModel();
                }

                _mapDataContainer.Data = await _mapDataContainer.GetMapData(_mapDataContainer.MapHash, _mapDataContainer.BeatmapDifficulty, _mapDataContainer.Characteristic);
                if (_model.Session == null)
                {
                    _model.Session = new InferenceSession(_model.ModelByte);
                }
                string inputNoneName = _model.Session?.InputMetadata.First().Key;
                double[] data = new double[15]
                {
                    _mapDataContainer.Data.Bpm,
                    _mapDataContainer.Data.Duration,
                    _mapDataContainer.Data.Difficulty,
                    _mapDataContainer.Data.SageScore,
                    _mapDataContainer.Data.Njs,
                    _mapDataContainer.Data.Offset,
                    _mapDataContainer.Data.Notes,
                    _mapDataContainer.Data.Bombs,
                    _mapDataContainer.Data.Obstacles,
                    _mapDataContainer.Data.Nps,
                    _mapDataContainer.Data.Events,
                    _mapDataContainer.Data.Chroma,
                    _mapDataContainer.Data.Errors,
                    _mapDataContainer.Data.Warns,
                    _mapDataContainer.Data.Resets
                };
#if DEBUG
                var innodedims = _model.Session?.InputMetadata.First().Value.Dimensions;
                Plugin.Log.Info(string.Join(", ",innodedims));
                Plugin.Log.Info(string.Join(". ", data));
#endif
                var inputTensor = new DenseTensor<double>(data, new int[] { 1, data.Length }, false);  // let's say data is fed into the Tensor objects
                List<NamedOnnxValue> inputs = new List<NamedOnnxValue>()
                    {
                        NamedOnnxValue.CreateFromTensor<double>(inputNoneName, inputTensor)
                    };
                using (var results = _model.Session?.Run(inputs))
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
                var request = new HttpRequestMessage(HttpMethod.Get, _model.ModelAssetEndpoint);
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