using HarmonyLib;
using PredictStarNumberMod.PP;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using static PredictStarNumberMod.PP.PPCalculatorData;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace PredictStarNumberMod.HarmonyPatches
{
    /// <summary>
    /// This patches ClassToPatch.MethodToPatch(Parameter1Type arg1, Parameter2Type arg2)
    /// </summary>
    [HarmonyPatch(typeof(LevelStatsView))]
    [HarmonyPatch("ShowStats", MethodType.Normal)]
    public class LevelStatsViewPatch
    {
        public static bool CurvesDownloadFinished { get; set; } = false;
        public static Curves Curves { get; set; } = null;

        private static Vector2 originalAnchoredPosition = new Vector2((float)12.00, (float)-3.80);
        private static Vector2 modifiedAnchordPostion = new Vector2((float)12.00, (float)-5.30);

        private static List<Point> _curve;
        private static double[] _slopes;
        private static double _multiplier = 1;

        /// <summary>
        /// This code is run after the original code in MethodToPatch is run.
        /// </summary>
        /// <param name="__instance">The instance of ClassToPatch</param>
        /// <param name="arg1">The Parameter1Type arg1 that was passed to MethodToPatch</param>
        /// <param name="____privateFieldInClassToPatch">Reference to the private field in ClassToPatch named '_privateFieldInClassToPatch', 
        ///     added three _ to the beginning to reference it in the patch. Adding ref means we can change it.</param>
        [HarmonyAfter(new string[] { "com.Idlebob.BeatSaber.ScorePercentage" })]
        static void Postfix(ref TextMeshProUGUI ____highScoreText, BeatmapKey beatmapKey, PlayerData playerData)
        {
            var rectTransform = ____highScoreText.GetComponent<RectTransform>();
            if(rectTransform.anchoredPosition == originalAnchoredPosition)
                rectTransform.anchoredPosition = modifiedAnchordPostion;
            wrapper(____highScoreText);
        }

        static async Task wrapper(TextMeshProUGUI field)
        {
            try
            {
                double predictedPP = await CalculatePP(5.08, 0.9649);
                field.text += "\n(" + predictedPP.ToString("0.00") + "PP)";
            }
            catch (Exception e)
            {
                Plugin.Log?.Error(e);
            }
        }

        private static void SetCurve(Curves curves)
        {
            _curve = curves.ScoreSaber.standardCurve;
            _slopes = CurveUtils.GetSlopes(_curve);
        }

        public static async Task<double> CalculatePP(double star, double accuracy, bool failed = false)
        {
            if (_curve == null || _slopes == null)
            {
                while (!CurvesDownloadFinished)
                {
                    Plugin.Log?.Info("Waiting for CurveDownloader to initialize");
                    await Task.Delay(300);
                }
                SetCurve(Curves);
            }

            double rawPP = star * PPCalculatorData.DefaultStarMultipllier;

            double multiplier = _multiplier;
            if (failed)
            {
                multiplier -= 0.5f;
            }

            return rawPP * CurveUtils.GetCurveMultiplier(_curve, _slopes, accuracy * multiplier);
        }
    }
}