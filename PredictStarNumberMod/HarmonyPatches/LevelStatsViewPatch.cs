using PredictStarNumberMod.PP;
using SiraUtil.Affinity;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static PredictStarNumberMod.PP.PPCalculatorData;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace PredictStarNumberMod.HarmonyPatches
{
    public class LevelStatsViewPatch: IAffinity
    {
        private Vector2 originalAnchoredPosition = new Vector2((float)12.00, (float)-3.80);
        private Vector2 modifiedAnchordPostion = new Vector2((float)12.00, (float)-5.30);

        private List<Point> _curve;
        private double[] _slopes;
        private double _multiplier = 1;

        private readonly PredictedStarNumberMonitor _predictedStarNumberMonitor;
        private readonly CurveDownloader _curveDownloader;
        private readonly BeatmapLevelLoader _beatmapLevelLoader;
        private readonly StandardLevelDetailViewController _standardLevelDetailViewController;
        private readonly BeatmapLevelsEntitlementModel _beatmapLevelsEntitlementModel;
        private readonly BeatmapDataLoader _beatmapDataLoader = new BeatmapDataLoader();

        public LevelStatsViewPatch(PredictedStarNumberMonitor predictedStarNumberMonitor, CurveDownloader curveDownloader, BeatmapLevelLoader beatmapLevelLoader, StandardLevelDetailViewController standardLevelDetailViewController, BeatmapLevelsEntitlementModel beatmapLevelsEntitlementModel)
        {
            _predictedStarNumberMonitor = predictedStarNumberMonitor;
            _curveDownloader = curveDownloader;
            _beatmapLevelLoader = beatmapLevelLoader;
            _standardLevelDetailViewController = standardLevelDetailViewController;
            _beatmapLevelsEntitlementModel = beatmapLevelsEntitlementModel;
        }

        [AffinityPatch(typeof(LevelStatsView), nameof(LevelStatsView.ShowStats))]
        [AffinityPrefix]
        protected void Prefix(ref TextMeshProUGUI ____highScoreText)
        {
            // 前回実行時に譜面データはあるがプレイヤーのクリアデータがない場合、trueになったままなので
            _predictedStarNumberMonitor.PredictedStarNumberChanged = false;
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
        protected void Postfix(ref TextMeshProUGUI ____highScoreText, BeatmapKey beatmapKey, PlayerData playerData)
        {
            wrapper(____highScoreText, beatmapKey, playerData);
        }

        private async Task wrapper(TextMeshProUGUI field,BeatmapKey beatmapKey, PlayerData playerData)
        {
            double percentage = await GetPercentage(beatmapKey, playerData);
            if(percentage == 0) return;

            try
            {
                double predictedPP = await CalculatePP(percentage);
#if DEBUG
                Plugin.Log.Info(predictedPP.ToString());
#endif
                RectTransform rectTransform = field.GetComponent<RectTransform>();
                // 短い間隔でマップを変更した場合、最終実行時の結果を残すため
                field.text = field.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)[0];
                
                if (StarNumberSetter.PredictedStarNumber == StarNumberSetter.skipStarNumber
                    || StarNumberSetter.PredictedStarNumber == StarNumberSetter.errorStarNumber)
                    return;

                field.text += "\n(" + predictedPP.ToString("0.00") + "PP)";
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
            if (playerData == null) return 0;

            PlayerLevelStatsData playerLevelStatsData = playerData.TryGetPlayerLevelStatsData(in beatmapKey);

            if (playerLevelStatsData == null) return 0;

            if (!playerLevelStatsData.validScore) return 0;

            int highscore = playerLevelStatsData.highScore;

            BeatmapLevel beatmapLevel = _standardLevelDetailViewController.beatmapLevel;
            BeatmapLevelDataVersion beatmapLevelDataVersion = await _beatmapLevelsEntitlementModel.GetLevelDataVersionAsync(beatmapLevel.levelID, CancellationToken.None);
            LoadBeatmapLevelDataResult beatmapLevelData = await _beatmapLevelLoader.LoadBeatmapLevelDataAsync(beatmapLevel, beatmapLevelDataVersion, CancellationToken.None);
            IReadonlyBeatmapData currentReadonlyBeatmapData = await _beatmapDataLoader.LoadBeatmapDataAsync(beatmapLevelData.beatmapLevelData, beatmapKey, beatmapLevel.beatsPerMinute, true, null, beatmapLevelDataVersion, playerData.gameplayModifiers, playerData.playerSpecificSettings, false);

            int currentDifficultyMaxScore = ScoreModel.ComputeMaxMultipliedScoreForBeatmap(currentReadonlyBeatmapData);
            double resultPercentage = (double)((double)highscore / (double)currentDifficultyMaxScore);

            return resultPercentage;
        }

        private void SetCurve(Curves curves)
        {
            _curve = curves.ScoreSaber.standardCurve;
            _slopes = CurveUtils.GetSlopes(_curve);
        }

        public async Task<double> CalculatePP(double accuracy, bool failed = false)
        {
            if (_curve == null || _slopes == null)
            {
                while (!_curveDownloader.CurvesDownloadFinished)
                {
                    Plugin.Log?.Info("Waiting for CurveDownloader to initialize");
                    await Task.Delay(300);
                }
                SetCurve(_curveDownloader.Curves);
            }

            while(!_predictedStarNumberMonitor.PredictedStarNumberChanged)
            {
                await Task.Delay(200);
            }
            _predictedStarNumberMonitor.PredictedStarNumberChanged = false;

            double rawPP = StarNumberSetter.PredictedStarNumber * PPCalculatorData.DefaultStarMultipllier;

            double multiplier = _multiplier;
            if (failed)
            {
                multiplier -= 0.5f;
            }

            return rawPP * CurveUtils.GetCurveMultiplier(_curve, _slopes, accuracy * multiplier);
        }
    }
}