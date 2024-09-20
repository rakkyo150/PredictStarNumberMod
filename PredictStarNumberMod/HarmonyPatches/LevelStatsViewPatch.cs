using IPA.Loader;
using PredictStarNumberMod.PP;
using SiraUtil.Affinity;
using System;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace PredictStarNumberMod.HarmonyPatches
{
    public class LevelStatsViewPatch: IAffinity
    {
        private Vector2 originalAnchoredPosition = new Vector2((float)12.00, (float)-3.80);
        private Vector2 modifiedAnchordPostion = new Vector2((float)12.00, (float)-5.30);
        private double neverClearPercentage = 0;

        private readonly PPCalculator _pPCalculator;
        private readonly Star.Star _star;
        private readonly PredictedStarNumberMonitor _predictedStarNumberMonitor;
        private readonly BeatmapLevelLoader _beatmapLevelLoader;
        private readonly StandardLevelDetailViewController _standardLevelDetailViewController;
        private readonly BeatmapLevelsEntitlementModel _beatmapLevelsEntitlementModel;
        private readonly BeatmapDataLoader _beatmapDataLoader = new BeatmapDataLoader();

        public LevelStatsViewPatch(PPCalculator pPCalculator, Star.Star star, PredictedStarNumberMonitor predictedStarNumberMonitor, CurveDownloader curveDownloader, BeatmapLevelLoader beatmapLevelLoader, StandardLevelDetailViewController standardLevelDetailViewController, BeatmapLevelsEntitlementModel beatmapLevelsEntitlementModel)
        {
            _pPCalculator = pPCalculator;
            _star = star;
            _predictedStarNumberMonitor = predictedStarNumberMonitor;
            _beatmapLevelLoader = beatmapLevelLoader;
            _standardLevelDetailViewController = standardLevelDetailViewController;
            _beatmapLevelsEntitlementModel = beatmapLevelsEntitlementModel;
        }

        [AffinityPatch(typeof(LevelStatsView), nameof(LevelStatsView.ShowStats))]
        [AffinityPrefix]
        protected void Prefix(ref TextMeshProUGUI ____highScoreText)
        {
            // 前回実行時に譜面データはあるがプレイヤーのクリアデータがない場合、trueになったままなので
            _predictedStarNumberMonitor.ClearPredictedStarNumberChanged();
            RectTransform rectTransform = ____highScoreText.GetComponent<RectTransform>();
            if (rectTransform.anchoredPosition == modifiedAnchordPostion)
                rectTransform.anchoredPosition = originalAnchoredPosition;
        }

        /// <summary>
        /// This code is run after the original code in MethodToPatch is run.
        /// </summary>
        /// <param name="__instance">The instance of ClassToPatch</param>
        /// <param name="arg1">The Parameter1Type arg1 that was passed to MethodToPatch</param>
        /// <param name="____privateFieldInClassToPatch">Reference to the private field in ClassToPatch named '_privateFieldInClassToPatch', 
        ///     added three _ to the beginning to reference it in the patch. Adding ref means we can change it.</param>
        [AffinityAfter(new string[] { "com.Idlebob.BeatSaber.ScorePercentage" })]
        [AffinityPatch(typeof(LevelStatsView), nameof(LevelStatsView.ShowStats))]
        [AffinityPrefix]
        // Entrypoint
        protected void Postfix(ref TextMeshProUGUI ____highScoreText, BeatmapKey beatmapKey, PlayerData playerData)
        {
            wrapper(____highScoreText, beatmapKey, playerData);
        }

        private async Task wrapper(TextMeshProUGUI field,BeatmapKey beatmapKey, PlayerData playerData)
        {
            try
            {
                _star.ChangePredictedStarNumber(await _star.PredictStarNumber());
            }
            catch(Exception ex)
            {
                Plugin.Log.Error(ex);
                _star.ChangePredictedStarNumber(_star.ErrorStarNumber);
            }

            double percentage = await GetPercentage(beatmapKey, playerData);

            RectTransform rectTransform = field.GetComponent<RectTransform>();

            if (percentage == neverClearPercentage)
            {
                _pPCalculator.PredictedPP = _pPCalculator.NoPredictedPP;
                if (PluginManager.GetPlugin("BetterSongList") != null) return;

                // 短い間隔でマップを変更した場合、最終実行時の結果を残すため
                field.text = field.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)[0];
                if (_star.PredictedStarNumber == _star.SkipStarNumber
                    || _star.PredictedStarNumber == _star.ErrorStarNumber)
                    return;
                field.text += "\n(★" + _star.PredictedStarNumber.ToString("0.00") + ")";
                if (rectTransform.anchoredPosition == originalAnchoredPosition)
                    rectTransform.anchoredPosition = modifiedAnchordPostion;
                return;
            }

            try
            {
                await _predictedStarNumberMonitor.AwaitUntilPredictedStarNumberChanged();

                _pPCalculator.PredictedPP = await _pPCalculator.CalculatePP(percentage);
#if DEBUG
                Plugin.Log.Info(_pPCalculator.PredictedPP.ToString());
#endif

                // 短い間隔でマップを変更した場合、最終実行時の結果を残すため
                field.text = field.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)[0];
                
                if (_star.PredictedStarNumber == _star.SkipStarNumber
                    || _star.PredictedStarNumber == _star.ErrorStarNumber)
                    return;

                if(PluginManager.GetPlugin("BetterSongList") == null)
                {
                    field.text += "\n(★" + _star.PredictedStarNumber.ToString("0.00") + " | " + _pPCalculator.PredictedPP.ToString("0.00") + "PP)";
                }
                else
                {
                    field.text += "\n(" + _pPCalculator.PredictedPP.ToString("0.00") + "PP)";
                }
                if (rectTransform.anchoredPosition == originalAnchoredPosition)
                    rectTransform.anchoredPosition = modifiedAnchordPostion;
                Plugin.Log.Info(field.text);
            }
            catch (Exception e)
            {
                Plugin.Log?.Error(e);
            }
        }

        private async Task<double> GetPercentage(BeatmapKey beatmapKey, PlayerData playerData)
        {
            if (playerData == null) return neverClearPercentage;

            PlayerLevelStatsData playerLevelStatsData = playerData.TryGetPlayerLevelStatsData(in beatmapKey);

            if (playerLevelStatsData == null) return neverClearPercentage;

            if (!playerLevelStatsData.validScore) return neverClearPercentage;

            int highscore = playerLevelStatsData.highScore;

            BeatmapLevel beatmapLevel = _standardLevelDetailViewController.beatmapLevel;
            BeatmapLevelDataVersion beatmapLevelDataVersion = await _beatmapLevelsEntitlementModel.GetLevelDataVersionAsync(beatmapLevel.levelID, CancellationToken.None);
            LoadBeatmapLevelDataResult beatmapLevelData = await _beatmapLevelLoader.LoadBeatmapLevelDataAsync(beatmapLevel, beatmapLevelDataVersion, CancellationToken.None);
            IReadonlyBeatmapData currentReadonlyBeatmapData = await _beatmapDataLoader.LoadBeatmapDataAsync(beatmapLevelData.beatmapLevelData, beatmapKey, beatmapLevel.beatsPerMinute, true, null, beatmapLevelDataVersion, playerData.gameplayModifiers, playerData.playerSpecificSettings, false);

            int currentDifficultyMaxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(currentReadonlyBeatmapData);
            double resultPercentage = (double)((double)highscore / (double)currentDifficultyMaxScore);

            return resultPercentage;
        }
    }
}