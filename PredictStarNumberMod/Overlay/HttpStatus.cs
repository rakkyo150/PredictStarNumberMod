using HttpSiraStatus.Enums;
using HttpSiraStatus.Interfaces;
using PredictStarNumberMod.HarmonyPatches;
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

        private OverlayStatus overlayStatus;

        public HttpStatus(IStatusManager statusManager, Star.Star star)
        {
            _statusManager = statusManager;
            _star = star;
        }

        public void OnChangedPredictedStarNumber()
        {
#if Debug
            Plugin.Log.Info("PredictedStarNumber changed");
#endif      
            if (!PluginConfig.Instance.Overlay)
            {
                if (this.overlayStatus == OverlayStatus.Hide) return;

                _statusManager.OtherJSON["PredictedStar"] = _star.SkipStarNumber;
                _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);

                this.overlayStatus = OverlayStatus.Hide;
                return;
            }
            
            _statusManager.OtherJSON["PredictedStar"] = _star.PredictedStarNumber;
            _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);

            if (_star.PredictedStarNumber == _star.SkipStarNumber)
                this.overlayStatus = OverlayStatus.Hide;
            else this.overlayStatus = OverlayStatus.Visible;
        }

        public void Initialize()
        {
            _star.ChangedPredictedStarNumber += OnChangedPredictedStarNumber;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    _star.ChangedPredictedStarNumber -= OnChangedPredictedStarNumber;
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
