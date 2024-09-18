using System;

namespace PredictStarNumberMod.Star
{
    public class Star
    {
        public double PredictedStarNumber { get; set; } = double.MinValue;
        internal double SkipStarNumber { get; } = -1.0;
        internal double ErrorStarNumber { get; } = -10.0;

        public Action ChangedPredictedStarNumber;

        internal void ChangePredictedStarNumber(double newPredictedStarNumber)
        {
            this.PredictedStarNumber = newPredictedStarNumber;
            this.ChangedPredictedStarNumber?.Invoke();
        }
    }
}
