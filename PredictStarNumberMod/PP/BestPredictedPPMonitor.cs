using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PredictStarNumberMod.PP
{
    public class BestPredictedPPMonitor
    {
        private bool bestPredictedPPChangeCompleted = false;
        private int tryPredictingCount = 0;

        private int leftAwaitingProcessCount = 0;

        public async Task AwaitUntilBestPredictedPPChangedCompletly()
        {
            int myProcess = leftAwaitingProcessCount;
            leftAwaitingProcessCount++;

            while (!(this.bestPredictedPPChangeCompleted && tryPredictingCount == 0))
            {
                await Task.Delay(50);
            }

            // 複数の処理が同時に終了するときに、呼び出された順に終了するようにする
            while (myProcess != 0)
            {
                await Task.Delay(50);
                myProcess--;
            }

            leftAwaitingProcessCount--;
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
