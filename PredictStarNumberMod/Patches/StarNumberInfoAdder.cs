using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BetterSongList.HarmonyPatches;
using UnityEngine;
using IPA.Utilities;
using System.Reflection;
using TMPro;

/// <summary>
/// See https://github.com/pardeike/Harmony/wiki for a full reference on Harmony.
/// </summary>
namespace PredictStarNumberMod.Patches
{
    public class StarNumberInfoAdder
    {
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
            Plugin.Log.Info("Hello");
            
            // IDifficultyBeatmap selectedDifficultyBeatmap = BS_Utils.Plugin.LevelData.GameplayCoreSceneSetupData.difficultyBeatmap;

            // Plugin.Log.Info(______selectedDifficultyBeatmap.difficulty.ToString());
            // Plugin.Log.Info(______selectedDifficultyBeatmap.difficultyRank.ToString());
            // Plugin.Log.Info(______selectedDifficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName);

            // Resources.FindObjectsOfTypeAll<IDifficultyBeatmap>().FirstOrDefault();

            // Plugin.Log.Info(____selectedDifficultyBeatmap.difficulty.ToString());
            // Plugin.Log.Info(____selectedDifficultyBeatmap.difficultyRank.ToString());
            // Plugin.Log.Info(____selectedDifficultyBeatmap.parentDifficultyBeatmapSet.beatmapCharacteristic.serializedName);

            // データなし
            if (___fields[1].text == "?") return;

            // ランク
            if (Double.TryParse(___fields[1].text, out _)) return;

            ___fields[1].text = "...";
            
            // 非同期で書き換えをする必要がある
            async void wrapper(TextMeshProUGUI[] fields)
            {
                string predictedStarNumber= await StarNumberPredictor.PredictStarNumber("2B54E35336C03893CA51202DD426D32DA91CB472", "Lawless-Normal");
                string showedStarNumber= $"({predictedStarNumber})";
                fields[1].text = showedStarNumber;
            }    

            wrapper(___fields);
        }
    }
}