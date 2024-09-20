using System.Threading.Tasks;

namespace PredictStarNumberMod.Star
{
    public class PredictedStarNumberMonitor
    {
        private bool predictedStarNumberChangeCompleted = false;
        private int tryPredictingCount = 0;

        public async Task AwaitUntilPredictedStarNumberChangedCompletly()
        {
            while (!(this.predictedStarNumberChangeCompleted && tryPredictingCount == 0))
            {
                await Task.Delay(200);
            }
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
