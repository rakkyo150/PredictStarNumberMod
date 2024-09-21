using System.Threading.Tasks;

namespace PredictStarNumberMod.Star
{
    public class PredictedStarNumberMonitor
    {
        private bool predictedStarNumberChangeCompleted = false;
        private int tryPredictingCount = 0;

        private int leftAwaitingProcessCount = 0;

        public async Task AwaitUntilPredictedStarNumberChangedCompletly()
        {
            int myProcess = leftAwaitingProcessCount;
            leftAwaitingProcessCount++;

            while (!(this.predictedStarNumberChangeCompleted && tryPredictingCount == 0))
            {
                await Task.Delay(100);
            }

            // 複数の処理が同時に終了するときに、呼び出された順に終了するようにする
            while (myProcess != 0)
            {
                await Task.Delay(100);
                myProcess--;
            }

            leftAwaitingProcessCount--;
        }

        internal void FinishChangingPredictedStarNumber()
        {
            this.predictedStarNumberChangeCompleted = true;
        }

        internal void StartChangingPredictedStarNumber()
        {
            this.predictedStarNumberChangeCompleted = false;
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
