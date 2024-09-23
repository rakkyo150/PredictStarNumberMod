using HttpSiraStatus.Enums;
using HttpSiraStatus.Interfaces;
using PredictStarNumberMod.Configuration;
using System;
using Zenject;
using static PredictStarNumberMod.Overlay.HttpStatus;

namespace PredictStarNumberMod.Overlay
{
    internal class NowPP : IInitializable, IDisposable
    {
        private bool _disposedValue;

        private readonly RelativeScoreAndImmediateRankCounter _relativeScoreAndImmediateRank;
        private readonly IStatusManager _statusManager;
        private readonly PP.PP _pP;

        private OverlayStatus nowPredictedPPOverlayStatus;

        public NowPP(RelativeScoreAndImmediateRankCounter relativeScoreAndImmediateRankCounter, IStatusManager statusManager, PP.PP pP)
        {
            _relativeScoreAndImmediateRank = relativeScoreAndImmediateRankCounter;
            _statusManager = statusManager;
            _pP = pP;
        }

        public void Initialize()
        {
            _relativeScoreAndImmediateRank.relativeScoreOrImmediateRankDidChangeEvent += OnChangeNowPP;
        }

        private async void OnChangeNowPP()
        {
#if Debug
            Plugin.Log.Info("PredictedStarNumber changed");
#endif
            if (!PluginConfig.Instance.Overlay || !PluginConfig.Instance.DisplayNowPP)
            {
                if (this.nowPredictedPPOverlayStatus == OverlayStatus.Hide) return;

                _statusManager.OtherJSON["NowPredictedPP"] = _pP.NoPredictedPP;
                _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);

                this.nowPredictedPPOverlayStatus = OverlayStatus.Hide;
                return;
            }

            double nowPP = await _pP.CalculatePP(Convert.ToDouble(_relativeScoreAndImmediateRank.relativeScore));

            _statusManager.OtherJSON["NowPredictedPP"] = nowPP;
            _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);

            if (nowPP == _pP.NoPredictedPP)
                this.nowPredictedPPOverlayStatus = OverlayStatus.Hide;
            else this.nowPredictedPPOverlayStatus = OverlayStatus.Visible;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    _relativeScoreAndImmediateRank.relativeScoreOrImmediateRankDidChangeEvent -= OnChangeNowPP;
                }
                this._disposedValue = true;
            }
        }

        public void Dispose()
        {
            // このコードを変更しないでください。クリーンアップ コードを 'Dispose(bool disposing)' メソッドに記述します
            this.Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
