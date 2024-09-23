using PredictStarNumberMod.Configuration;
using IPA.Loader;
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
        private double neverClearPercentage = -1;

        private RectTransform rectTransform;

        private SongDetailsCache.SongDetails songDetails;

        private readonly PP.PP _pP;
        private readonly Star.Star _star;
        private readonly BeatmapLevelLoader _beatmapLevelLoader;
        private readonly StandardLevelDetailViewController _standardLevelDetailViewController;
        private readonly BeatmapLevelsEntitlementModel _beatmapLevelsEntitlementModel;
        private readonly BeatmapDataLoader _beatmapDataLoader = new BeatmapDataLoader();

        public LevelStatsViewPatch(PP.PP pP, Star.Star star, BeatmapLevelLoader beatmapLevelLoader, StandardLevelDetailViewController standardLevelDetailViewController, BeatmapLevelsEntitlementModel beatmapLevelsEntitlementModel)
        {
            _pP = pP;
            _star = star;
            _beatmapLevelLoader = beatmapLevelLoader;
            _standardLevelDetailViewController = standardLevelDetailViewController;
            _beatmapLevelsEntitlementModel = beatmapLevelsEntitlementModel;
        }

        [AffinityPatch(typeof(LevelStatsView), nameof(LevelStatsView.ShowStats))]
        [AffinityPrefix]
        protected void Prefix(ref TextMeshProUGUI ____highScoreText)
        {
            rectTransform = ____highScoreText.GetComponent<RectTransform>();
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
            if (!PluginConfig.Instance.Enable) return;
            wrapper(____highScoreText, beatmapKey, playerData);
        }

        private async Task wrapper(TextMeshProUGUI field, BeatmapKey beatmapKey, PlayerData playerData)
        {
            if (PluginManager.GetPlugin("BetterSongList") == null)
            {
                await SetStarNumberWithoutBetterSongList(beatmapKey);
            }

            double percentage = await GetPercentage(beatmapKey, playerData);
            _pP.SetAccuracy(percentage);
#if DEBUG
            Plugin.Log.Info("GetPercentage : " + percentage.ToString());
#endif
            // クリアなしの処理は早期にここで切れる
            // そのため、クリアありの処理でクリアなしの処理の前に実行された処理に関して、後にfield.textでも確認が必要
            if (percentage == neverClearPercentage || !PluginConfig.Instance.DisplayBestPP || field.text == "-")
            {
                _pP.SetBestPredictedPP(_pP.NoPredictedPP);
                if (PluginManager.GetPlugin("BetterSongList") != null) return;
                DeleteSecondAndSubsequentLines(field);
                double predictedStarNumber = await _star.GetPredictedStarNumber();
                if (predictedStarNumber == _star.SkipStarNumber
                    || predictedStarNumber == _star.ErrorStarNumber)
                    return;
                field.text += "\n(★" + predictedStarNumber.ToString("0.00") + ")";
                ChangeFieldHeightForSecondAndSubsequentLines(rectTransform);
                return;
            }

            if (!PluginConfig.Instance.DisplayBestPP) return;

            try
            {
                double predictedStarNumber = await _star.GetPredictedStarNumber();

                DeleteSecondAndSubsequentLines(field);
                if (predictedStarNumber == _star.SkipStarNumber
                    || predictedStarNumber == _star.ErrorStarNumber)
                {
                    _pP.SetBestPredictedPP(_pP.NoPredictedPP);
                    return;
                }

                try
                {
#if DEBUG
                    Plugin.Log.Info($"Start AddQueueCalculatingAndSettingBestPP by LevelStatsViewPatch : {percentage}");
#endif
                    // 高速で譜面切り替えると、percentageをここで引数としては使うと、なぜかその時点で前の譜面の値に切り替わる
                    // そこで、_pPに排他制御を効かせてもらうことで、この問題を解決
                    double test = await _pP.AddQueueCalculatingAndSettingBestPP();
#if DEBUG
                    Plugin.Log.Info($"test : {test}");
#endif
                }
                catch (Exception ex)
                {
                    Plugin.Log.Error(ex);
                    _pP.SetBestPredictedPP(_pP.NoPredictedPP);
                }
#if DEBUG
                Plugin.Log.Info($"GetPercentage after awaiting _pP.AddQueueCalculatingAndSettingBestPP(percentage) : {percentage}");
#endif
                
                double bestPredictedPP = await _pP.GetBestPredictedPP();
#if DEBUG
                Plugin.Log.Info($"bestPredictedPP : {bestPredictedPP}");
#endif

                // 上述したとおり、非同期処理のawait待ち先の処理が後にずれる場合があるので、ここでも確認
                if (field.text == "-")
                {
                    _pP.SetBestPredictedPP(_pP.NoPredictedPP);
                    if (PluginManager.GetPlugin("BetterSongList") != null) return;
                    DeleteSecondAndSubsequentLines(field);
                    if (predictedStarNumber == _star.SkipStarNumber
                        || predictedStarNumber == _star.ErrorStarNumber)
                        return;
                    field.text += "\n(★" + predictedStarNumber.ToString("0.00") + ")";
                    ChangeFieldHeightForSecondAndSubsequentLines(rectTransform);
                    return;
                }
                
                DeleteSecondAndSubsequentLines(field);
                if (PluginManager.GetPlugin("BetterSongList") == null)
                {
                    field.text += "\n(★" + predictedStarNumber.ToString("0.00") + " | " + bestPredictedPP.ToString("0.00") + "PP)";
                }
                else
                {
                    field.text += "\n(" + bestPredictedPP.ToString("0.00") + "PP)";
                }
                ChangeFieldHeightForSecondAndSubsequentLines(rectTransform);
            }
            catch (Exception e)
            {
                Plugin.Log?.Error(e);
            }
        }

        private void DeleteSecondAndSubsequentLines(TextMeshProUGUI field)
        {
            // 短い間隔でマップを変更した場合、最終実行時の結果を残すため
            field.text = field.text.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)[0];
        }

        private void ChangeFieldHeightForSecondAndSubsequentLines(RectTransform rectTransform)
        {
            if (rectTransform.anchoredPosition == originalAnchoredPosition)
                rectTransform.anchoredPosition = modifiedAnchordPostion;
        }

        // 高速で譜面切り替えると、譜面が完全に切り替わっていない瞬間は変な値が返ってくることがある？
        // スレッドセーフにしてる途中からなくなったかも
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

        private async Task SetStarNumberWithoutBetterSongList(BeatmapKey beatmapKey)
        {
            try
            {
                if (PluginConfig.Instance.DisplayValuesInRankMap || PluginManager.GetPluginFromId("SongDetailsCache") == null)
                {
                    await _star.AddQueuePredictingAndSettingStarNumber();
                    return;
                }

                if(songDetails == null) songDetails = await SongDetailsCache.SongDetails.Init();
                bool songExists = songDetails.songs.FindByHash(beatmapKey.GetHashCode().ToString(), out var song);
                bool difficyltyExits = song.GetDifficulty(out var difficulty, (SongDetailsCache.Structs.MapDifficulty)beatmapKey.difficulty,
                    (SongDetailsCache.Structs.MapCharacteristic)this.GetCharacteristicFromDifficulty(beatmapKey));
                if (!songExists || !difficyltyExits)
                {
                    await _star.AddQueuePredictingAndSettingStarNumber();
                    return;
                }

                if (difficulty.stars > 0)
                {
                    _star.SetPredictedStarNumber(_star.SkipStarNumber);
                    return;
                }

                await _star.AddQueuePredictingAndSettingStarNumber();
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex);
                _star.SetPredictedStarNumber(_star.ErrorStarNumber);
            }
        }

        private int GetCharacteristicFromDifficulty(BeatmapKey diff)
        {
            var d = diff.beatmapCharacteristic?.sortingOrder;

            if (d == null || d > 4)
                return 0;

            // 360 and 90 are "flipped" as far as the enum goes
            if (d == 3)
                d = 4;
            else if (d == 4)
                d = 3;

            return (int)d + 1;
        }
    }
}