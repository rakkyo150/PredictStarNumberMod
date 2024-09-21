using HttpSiraStatus.Enums;
using HttpSiraStatus.Interfaces;
using System;
using Zenject;
using PluginConfig = PredictStarNumberMod.Configuration.PluginConfig;

namespace PredictStarNumberMod.Overlay
{
    internal class HttpStatus: IInitializable, IDisposable
    {
        private bool _disposedValue;
        private readonly IStatusManager _statusManager;
        private readonly Star.Star _star;
        private readonly PP.PP _pP;

        private OverlayStatus predictedStarOverlayStatus;
        private OverlayStatus bestPredictedPPOverlayStatus;

        public HttpStatus(IStatusManager statusManager, Star.Star star, PP.PP pP)
        {
            _statusManager = statusManager;
            _star = star;
            _pP = pP;
        }

        public void OnChangedPredictedStarNumber(double predictedStarNumber)
        {
#if Debug
            Plugin.Log.Info("PredictedStarNumber changed");
#endif      
            if (!PluginConfig.Instance.Overlay)
            {
                if (this.predictedStarOverlayStatus == OverlayStatus.Hide) return;

                _statusManager.OtherJSON["PredictedStar"] = _star.SkipStarNumber;
                _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);

                this.predictedStarOverlayStatus = OverlayStatus.Hide;
                return;
            }
            
            _statusManager.OtherJSON["PredictedStar"] = predictedStarNumber;
            _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);

            if (predictedStarNumber == _star.SkipStarNumber)
                this.predictedStarOverlayStatus = OverlayStatus.Hide;
            else this.predictedStarOverlayStatus = OverlayStatus.Visible;
        }

        public void OnChangedBestPredictedPP(double bestPP)
        {
#if Debug
            Plugin.Log.Info("PredictedStarNumber changed");
#endif      
            if (!PluginConfig.Instance.Overlay)
            {
                if (this.bestPredictedPPOverlayStatus == OverlayStatus.Hide) return;

                _statusManager.OtherJSON["BestPredictedPP"] = _pP.NoPredictedPP;
                _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);

                this.bestPredictedPPOverlayStatus = OverlayStatus.Hide;
                return;
            }

            _statusManager.OtherJSON["BestPredictedPP"] = bestPP;
            _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);

            if (bestPP == _pP.NoPredictedPP)
                this.bestPredictedPPOverlayStatus = OverlayStatus.Hide;
            else this.bestPredictedPPOverlayStatus = OverlayStatus.Visible;
        }

        public void Initialize()
        {
            _star.ChangedPredictedStarNumber += OnChangedPredictedStarNumber;
            _pP.ChangedBestPredictedPP += OnChangedBestPredictedPP;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    _star.ChangedPredictedStarNumber -= OnChangedPredictedStarNumber;
                    _pP.ChangedBestPredictedPP -= OnChangedBestPredictedPP;
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

        public enum OverlayStatus
        {
            Visible,
            Hide
        }
    }
}
