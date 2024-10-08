﻿using BetterSongList.HarmonyPatches.UI;
using PredictStarNumberMod.Configuration;
using PredictStarNumberMod.Map;
using SiraUtil.Affinity;
using System;
using System.Threading.Tasks;
using TMPro;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace PredictStarNumberMod.HarmonyPatches
{
    // LevelStatsViewPatchよりこっちが先に実行されるが、BetterSongListの依存を脱する必要がある
    // そのため、LevelStatsViewPatchにエントリーポイントを移行
    public class StarNumberSetter : IAffinity
    {
        private readonly MapDataContainer _mapDataContainer;
        private readonly Star.Star _star;

        private float originalFontSize = float.MinValue;

        private string sSRankStarNumber = string.Empty;

        private readonly object lockField = new object();

        public StarNumberSetter(MapDataContainer mapDataContainer, Star.Star star)
        {
            _mapDataContainer = mapDataContainer;
            _star = star;
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

            lock (lockField)
            {
                if (originalFontSize == float.MinValue)
                {
                    originalFontSize = ___fields[1].fontSize;
                }

                if (___fields[1].fontSize != originalFontSize)
                {
                    ___fields[1].fontSize = originalFontSize;
                }
            }

            if (!PluginConfig.Instance.Enable)
            {
                // In oreder to hide overlay
                _star.SetPredictedStarNumber(_star.SkipStarNumber);
                return;
            }

            // データなし
            if (CheckFieldsTextValue(___fields, "?"))
            {
                SetSkipStarNumberAndQuestionMark(___fields);
                return;
            }

            if (!PluginConfig.Instance.DisplayValuesInRankMap && IsRankedMap(___fields))
            {
                _star.SetPredictedStarNumber(_star.SkipStarNumber);
                return;
            }

            bool isRankedMap = IsRankedMap(___fields);

            lock (lockField)
            {
                if (isRankedMap)
                {
                    sSRankStarNumber = ___fields[1].text;
                    ___fields[1].text += "...";
                }
                else
                {
                    ___fields[1].text = "...";
                }
            }

            wrapper(___fields);

            // 非同期で書き換えをする必要がある
            async Task wrapper(TextMeshProUGUI[] fields)
            {
#if DEBUG
                Plugin.Log.Info("Start AddQueuePredictingStarNumber by BetterSongList");
#endif
                double predictedStarNumber = await _star.AddQueuePredictingAndSettingStarNumber();

                if (predictedStarNumber == _star.ErrorStarNumber)
                {
                    if (isRankedMap)
                    {
                        SetPredictedStarNumberForRankedMap(fields, "Error");
                        return;
                    }
                    SetPredictedStarNumberForUnrankedMap(fields, "Error");
                    return;
                }

                string predictedStarNumberString = predictedStarNumber.ToString("0.00");
#if DEBUG
                Plugin.Log.Info("predictedStarNumberString : " + predictedStarNumberString);
#endif
                if (isRankedMap)
                {
                    SetPredictedStarNumberForRankedMap(fields, predictedStarNumberString);
                    return;
                }

                SetPredictedStarNumberForUnrankedMap(fields, predictedStarNumberString);
            }
        }

        private bool CheckFieldsTextValue(TextMeshProUGUI[] ___fields, string value)
        {
            lock (lockField)
            {
                return ___fields[1].text == value;
            }
        }

        private async Task SetSkipStarNumberAndQuestionMark(TextMeshProUGUI[] fields)
        {
#if DEBUG
            Plugin.Log.Info("Start AddQueueSettingSkipStarNumber");
#endif
            double _ = await _star.AddQueueSettingSkipStarNumber();
#if DEBUG
            Plugin.Log.Info("Finish AddQueueSettingSkipStarNumber");
#endif
            lock (lockField)
            {
                fields[1].text = "?";
            }

            //　次回の星予測のデータのために更新必要
            _star.RefreshPreviousMadDataForPredictingStarNumber();
        }

        private void SetPredictedStarNumberForRankedMap(TextMeshProUGUI[] fields, string predictedStarNumber)
        {
            lock (lockField)
            {
                fields[1].text = fields[1].text.Replace("...", "");
                // 高速譜面変更でScoreSaberのランク情報書き消えるので
                fields[1].text = $"{sSRankStarNumber}({predictedStarNumber})";
                fields[1].fontSize = 3.2f;
            }
        }

        private void SetPredictedStarNumberForUnrankedMap(TextMeshProUGUI[] fields, string predictedStarNumber)
        {
            lock (lockField)
            {
                fields[1].text = $"({predictedStarNumber})";
                // 高速譜面変更でFontSizeが変わるので
                fields[1].fontSize = originalFontSize;
            }
        }

        private bool IsRankedMap(TextMeshProUGUI[] fields)
        {
            lock (lockField)
            {
                return Double.TryParse(fields[1].text, out _);
            }
        }
    }
}