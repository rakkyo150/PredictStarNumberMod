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

        private TMP_Text _counterLeft;
        private TMP_Text _counterRight;

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
                label.text = "Predict Star Number Counter";
                label.fontSize = PluginConfig.Instance.LabelFontSize;
            }

            Vector3 leftOffset = new Vector3(x, y - 0.2f, z);
            TextAlignmentOptions leftAlign = TextAlignmentOptions.Top;

            _counterRight = CanvasUtility.CreateTextFromSettings(Settings, new Vector3(x + 0.2f, y - 0.2f, z));
            _counterRight.lineSpacing = -26;
            _counterRight.fontSize = PluginConfig.Instance.FigureFontSize;
            AddPPAndStarInfo(_counterRight);
            _counterRight.alignment = TextAlignmentOptions.TopLeft;

            leftOffset = new Vector3(x - 0.2f, y - 0.2f, z);
            leftAlign = TextAlignmentOptions.TopRight;

            _counterLeft = CanvasUtility.CreateTextFromSettings(Settings, leftOffset);
            _counterLeft.lineSpacing = -26;
            _counterLeft.fontSize = PluginConfig.Instance.FigureFontSize;
            _counterLeft.text = "0.00PP";
            _counterLeft.alignment = leftAlign;
        }

        private async Task AddPPAndStarInfo(TMP_Text counter)
        {
            double predictedStarNumber = await _star.GetLatestPredictedStarNumber();
            if (predictedStarNumber == _star.ErrorStarNumber || predictedStarNumber == _star.SkipStarNumber)
            {
                counter.text = "-";
                return;
            }
            double bestPredictedPP = await _pP.GetLatestBestPredictedPP();
            if(bestPredictedPP == _pP.NoPredictedPP)
            {
                counter.text = "-";
                return;
            }
            counter.text = bestPredictedPP.ToString("0.00") + "PP(★" + predictedStarNumber.ToString("0.00p") + ")";
        }

        public void OnNoteCut(NoteData data, NoteCutInfo info)
        {

        }

        private async void ChangeNowPP()
        {
            double nowPP = await _pP.CalculatePP(Convert.ToDouble(_relativeScoreAndImmediateRank.relativeScore));
            if(nowPP == _pP.NoPredictedPP)
            {
                _counterLeft.text = "-";
                return;
            }
            _counterLeft.text = nowPP.ToString("0.00") + "PP";
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
