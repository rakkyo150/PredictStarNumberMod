using System;
using Zenject;

namespace PredictStarNumberMod.PP
{
    public class PredictedStarNumberMonitor : IInitializable, IDisposable
    {
        public bool PredictedStarNumberChanged { get; set; } = false;

        private bool _disposedValue;

        private readonly Star.Star _star;

        public PredictedStarNumberMonitor(Star.Star star)
        {
            _star = star;
        }

        public void Initialize()
        {
            _star.ChangedPredictedStarNumber += OnChangedPredictedStarNumber;
        }

        private void OnChangedPredictedStarNumber()
        {
            this.PredictedStarNumberChanged = true;
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
    }
}
