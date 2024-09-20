using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PredictStarNumberMod.PP
{
    public class BestPredictedPPMonitor
    {
        private bool bestPredictedPPChangeCompleted = false;
        private int tryPredictingCount = 0;

        public async Task AwaitUntilBestPredictedPPChangedCompletly()
        {
            while (!(this.bestPredictedPPChangeCompleted && tryPredictingCount == 0))
            {
                await Task.Delay(200);
            }
        }

        internal void FinishChangingBestPredictedPP()
        {
            this.bestPredictedPPChangeCompleted = true;
        }

        internal void StartChangingBestPredictedPP()
        {
            this.bestPredictedPPChangeCompleted = false;
        }

        internal void PlusTryCalculatingCount()
        {
            tryPredictingCount++;
        }

        internal void MinusTryCalculatingCount()
        {
            tryPredictingCount--;
        }
    }
}
