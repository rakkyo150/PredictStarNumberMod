using CountersPlus.Counters.Custom;
using CountersPlus.Counters.Interfaces;
using CountersPlus.Utils;
using PredictStarNumberCounter.Configuration;
using PredictStarNumberMod.PP;
using PredictStarNumberMod.Star;
using System;
using System.Globalization;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace PredictStarNumberCounter
{
    internal class PredictStarNumberCounter: BasicCustomCounter, INoteEventHandler, IDisposable
    {
        private readonly Star _star;
        private readonly PP _pP;
        private readonly RelativeScoreAndImmediateRankCounter _relativeScoreAndImmediateRank;

        private TMP_Text _counter;

        private string nowPPString = "-";
        private string bestPredictedPPString = "-";
        private string predictedStarNumberString = "-";

        float x = PluginConfig.Instance.OffsetX;
        float y = PluginConfig.Instance.OffsetY;
        float z = PluginConfig.Instance.OffsetZ;
        private bool disposedValue;

        public PredictStarNumberCounter(Star star, PP pP, RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRank)
        {
            _star = star;
            _pP = pP;
            _relativeScoreAndImmediateRank = relativeScoreAndImmediateRank;

            _relativeScoreAndImmediateRank.relativeScoreOrImmediateRankDidChangeEvent += ChangeNowPP;
        }

        public override void CounterInit()
        {
            string defaultValue = Format(0, PluginConfig.Instance.DecimalPrecision);

            if (PluginConfig.Instance.EnableLabel)
            {
                var label = CanvasUtility.CreateTextFromSettings(Settings, new Vector3(x, y, z));
                label.text = PluginConfig.Instance.LabelText;
                label.fontSize = PluginConfig.Instance.LabelFontSize;
            }

            _counter = CanvasUtility.CreateTextFromSettings(Settings, new Vector3(x, y - 0.2f, z));
            _counter.lineSpacing = -26;
            _counter.fontSize = PluginConfig.Instance.FigureFontSize;
            AddPPAndStarInfo();
            _counter.alignment = TextAlignmentOptions.Top;
        }

        private async Task AddPPAndStarInfo()
        {
            double predictedStarNumber = await _star.GetPredictedStarNumber();
            predictedStarNumberString = predictedStarNumber.ToString("0.00");
            if (predictedStarNumber == _star.ErrorStarNumber || predictedStarNumber == _star.SkipStarNumber)
            {
                predictedStarNumberString = "-";
            }
            double bestPredictedPP = await _pP.GetBestPredictedPP();
            bestPredictedPPString = bestPredictedPP.ToString("0.00") + "PP";
            if (bestPredictedPP == _pP.NoPredictedPP)
            {
                bestPredictedPPString = "-";
            }   
            _counter.text = MakeCounterText();
        }

        private string MakeCounterText()
        {
            switch (PluginConfig.Instance.Display)
            {
                case PluginConfig.DisplayType.All:
                    return predictedStarNumberString + "★" + "\n" + nowPPString + " | " + bestPredictedPPString;
                case PluginConfig.DisplayType.StarOnly:
                    return predictedStarNumberString + "★";
                case PluginConfig.DisplayType.NowPPOnly:
                    return nowPPString;
                case PluginConfig.DisplayType.BestPPOnly:
                    return bestPredictedPPString;
                case PluginConfig.DisplayType.StarAndNowPP:
                    return predictedStarNumberString + "★" + "\n" + nowPPString;
                case PluginConfig.DisplayType.StarAndBestPP:
                    return predictedStarNumberString + "★" + "\n" + bestPredictedPPString;
                case PluginConfig.DisplayType.NowPPAndBestPP:
                    return nowPPString + " | " + bestPredictedPPString;
                default:
                    return predictedStarNumberString + "★" + "\n" + nowPPString + " | " + bestPredictedPPString;
            }
        }

        public void OnNoteCut(NoteData data, NoteCutInfo info)
        {

        }

        private async void ChangeNowPP()
        {
            double nowPP = await _pP.CalculatePP(Convert.ToDouble(_relativeScoreAndImmediateRank.relativeScore));
            nowPPString = nowPP.ToString("0.00") + "PP";
            if (nowPP == _pP.NoPredictedPP)
            {
                nowPPString = "-";
            }
            _counter.text = MakeCounterText();
        }

        public void OnNoteMiss(NoteData data)
        {

        }

        public override void CounterDestroy()
        {

        }

        private string Format(double StandardDeviation, int DecimalPrecision)
        {
            return StandardDeviation.ToString($"F{DecimalPrecision}", CultureInfo.InvariantCulture);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    _relativeScoreAndImmediateRank.relativeScoreOrImmediateRankDidChangeEvent -= ChangeNowPP;
                }
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
