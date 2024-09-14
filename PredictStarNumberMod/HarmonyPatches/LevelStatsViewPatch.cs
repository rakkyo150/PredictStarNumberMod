using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;

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
        private static Vector2 originalAnchoredPosition = new Vector2((float)12.00, (float)-3.80);
        private static Vector2 modifiedAnchordPostion = new Vector2((float)12.00, (float)-5.30);

        /// <summary>
        /// This code is run after the original code in MethodToPatch is run.
        /// </summary>
        /// <param name="__instance">The instance of ClassToPatch</param>
        /// <param name="arg1">The Parameter1Type arg1 that was passed to MethodToPatch</param>
        /// <param name="____privateFieldInClassToPatch">Reference to the private field in ClassToPatch named '_privateFieldInClassToPatch', 
        ///     added three _ to the beginning to reference it in the patch. Adding ref means we can change it.</param>
        [HarmonyAfter(new string[] { "com.Idlebob.BeatSaber.ScorePercentage" })]
        static void Postfix(ref TextMeshProUGUI ____highScoreText)
        {
            var rectTransform = ____highScoreText.GetComponent<RectTransform>();
            if(rectTransform.anchoredPosition == originalAnchoredPosition)
                rectTransform.anchoredPosition = modifiedAnchordPostion;
            ____highScoreText.text += "\n(?PP)";
        }
    }
}