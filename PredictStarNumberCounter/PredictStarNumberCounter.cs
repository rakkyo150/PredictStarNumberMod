using CountersPlus.Counters.Custom;
using CountersPlus.Counters.Interfaces;
using CountersPlus.Utils;
using PredictStarNumberCounter.Configuration;
using PredictStarNumberMod.PP;
using PredictStarNumberMod.Star;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace PredictStarNumberCounter
{
    internal class PredictStarNumberCounter : BasicCustomCounter, INoteEventHandler, IDisposable
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
            if (!PredictStarNumberMod.Configuration.PluginConfig.Instance.Enable) return;

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
            await SetPredictedStarNumber();
            await SetBestPredictedPP();
        }

        private async Task SetBestPredictedPP()
        {
            // なくても問題はないが、無駄な処理をなくすため
            if (!PredictStarNumberMod.Configuration.PluginConfig.Instance.DisplayBestPP) return;

            double bestPredictedPP = await _pP.GetBestPredictedPP();
            bestPredictedPPString = bestPredictedPP.ToString($"F{PluginConfig.Instance.DecimalPrecision}") + "PP";
            if (bestPredictedPP == _pP.NoPredictedPP)
            {
                bestPredictedPPString = "-";
            }
            _counter.text = MakeCounterText();
        }

        private async Task SetPredictedStarNumber()
        {
            double predictedStarNumber = await _star.GetPredictedStarNumber();
            predictedStarNumberString = predictedStarNumber.ToString($"F{PluginConfig.Instance.DecimalPrecision}");
            if (predictedStarNumber == _star.ErrorStarNumber || predictedStarNumber == _star.SkipStarNumber)
            {
                predictedStarNumberString = "-";
            }
        }

        private async void ChangeNowPP()
        {
            // なくても問題はないが、無駄な処理をなくすため
            if (!PredictStarNumberMod.Configuration.PluginConfig.Instance.Enable) return;

            if (!PredictStarNumberMod.Configuration.PluginConfig.Instance.DisplayNowPP) return;

            double nowPP = await _pP.CalculatePP(Convert.ToDouble(_relativeScoreAndImmediateRank.relativeScore));
            nowPPString = nowPP.ToString($"F{PluginConfig.Instance.DecimalPrecision}") + "PP";
            if (nowPP == _pP.NoPredictedPP)
            {
                nowPPString = "-";
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

        public void OnNoteCut(NoteData data, NoteCutInfo info) { }

        public void OnNoteMiss(NoteData data) { }

        public override void CounterDestroy() { }

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
