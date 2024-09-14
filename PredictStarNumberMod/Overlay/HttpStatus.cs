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

        private OverlayStatus overlayStatus;

        public HttpStatus(IStatusManager statusManager)
        {
            _statusManager = statusManager;
        }

        public void OnChangedPredictedStarNumber()
        {
#if Debug
            Plugin.Log.Info("PredictedStarNumber changed");
#endif      
            if (!PluginConfig.Instance.Overlay)
            {
                if (this.overlayStatus == OverlayStatus.Hide) return;

                _statusManager.OtherJSON["PredictedStar"] = StarNumberSetter.skipStarNumber;
                _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);

                this.overlayStatus = OverlayStatus.Hide;
                return;
            }
            
            _statusManager.OtherJSON["PredictedStar"] = StarNumberSetter.PredictedStarNumber;
            _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);

            if (StarNumberSetter.PredictedStarNumber == StarNumberSetter.skipStarNumber)
                this.overlayStatus = OverlayStatus.Hide;
            else this.overlayStatus = OverlayStatus.Visible;
        }

        public void Initialize()
        {
            StarNumberSetter.ChangedPredictedStarNumber += OnChangedPredictedStarNumber;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this._disposedValue)
            {
                if (disposing)
                {
                    StarNumberSetter.ChangedPredictedStarNumber -= OnChangedPredictedStarNumber;
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
