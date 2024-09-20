using System;
using System.Threading.Tasks;
using Zenject;

namespace PredictStarNumberMod.PP
{
    public class PredictedStarNumberMonitor
    {
        private bool PredictedStarNumberChanged = false;
        private int tryPredictingCount = 0;

        public async Task AwaitUntilPredictedStarNumberChanged()
        {
            while (!this.PredictedStarNumberChanged || tryPredictingCount != 0)
            {
                await Task.Delay(200);
            }
        }
        
        internal void ChangePredictedStarNumberMonitorTrue()
        {
            this.PredictedStarNumberChanged = true;
        }

        internal void ClearPredictedStarNumberChanged()
        {
            this.PredictedStarNumberChanged = false;
        }

        internal void PlusTryPredictingCount()
        {
            tryPredictingCount++;
        }

        internal void MinusTryPredictingCount()
        {
            tryPredictingCount--;
        }
    }
}
