using HttpSiraStatus.Enums;
using HttpSiraStatus.Interfaces;
using PredictStarNumberMod.HarmonyPatches;
using System;
using Zenject;

namespace PredictStarNumberMod.Overlay
{
    internal class HttpStatus: IInitializable, IDisposable
    {
        private bool _disposedValue;
        private readonly IStatusManager _statusManager;

        public HttpStatus(IStatusManager statusManager)
        {
            _statusManager = statusManager;
        }

        public void OnChangedPredictedStarNumber()
        {
#if Debug
            Plugin.Log.Info("PredictedStarNumber changed");
#endif
            _statusManager.OtherJSON["PredictedStar"] = StarNumberSetter.PredictedStarNumber;
            _statusManager.EmitStatusUpdate(ChangedProperty.Other, BeatSaberEvent.Other);
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
    }
}
