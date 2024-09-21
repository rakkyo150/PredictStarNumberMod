using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PredictStarNumberMod.PP
{
    public class PP
    {
        private double bestPredictedPP;
        public double NoPredictedPP { get; } = -1;

        internal List<Point> Curve { get; set; }
        internal double[] Slopes { get; set; }

        internal double DefaultStarMultipllier { get; } = 42.11;
        internal double Multiplier { get; } = 1;

        public Action<double> ChangedBestPredictedPP;

        private readonly Star.Star _star;
        private readonly CurveDownloader _curveDownloader;
        private readonly BestPredictedPPMonitor _bestPredictedPPMonitor;

        public PP(Star.Star star, CurveDownloader curveDownloader, BestPredictedPPMonitor predictedPPMonitor)
        {
            _star = star;
            _curveDownloader = curveDownloader;
            _bestPredictedPPMonitor = predictedPPMonitor;
        }

        public async Task<double> GetLatestBestPredictedPP()
        {
            await _bestPredictedPPMonitor.AwaitUntilBestPredictedPPChangedCompletly();
            return this.bestPredictedPP;
        }

        internal void SetBestPredictedPP(double newPredictedPP)
        {
            this.bestPredictedPP = newPredictedPP;
            _bestPredictedPPMonitor.FinishChangingBestPredictedPP();
            this.ChangedBestPredictedPP?.Invoke(this.bestPredictedPP);
#if DEBUG
            Plugin.Log.Info($"predictedPP Changed : newPredictedPP=={newPredictedPP}");
#endif
        }

        public async Task<double> CalculatePP(double accuracy, bool failed = false)
        {
            try
            {
                if (this.Curve == null || this.Slopes == null)
                {
                    while (!_curveDownloader.CurvesDownloadFinished)
                    {
                        Plugin.Log?.Info("Waiting for CurveDownloader to initialize");
                        await Task.Delay(300);
                    }
                    SetCurve(_curveDownloader.Curves);
                }

                double predictedStarNumber = await _star.GetLatestPredictedStarNumber();

                if (predictedStarNumber == _star.SkipStarNumber
                                   || predictedStarNumber == _star.ErrorStarNumber)
                {
                    return this.NoPredictedPP;
                }

                double rawPP = predictedStarNumber * this.DefaultStarMultipllier;

                double multiplier = this.Multiplier;
                if (failed)
                {
                    multiplier -= 0.5f;
                }

                return rawPP * this.GetCurveMultiplier(accuracy * multiplier);
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex);
                return this.NoPredictedPP;
            }
        }
        
        internal async Task<double> CalculateBestPP(double accuracy, bool failed = false)
        {
            _bestPredictedPPMonitor.PlusTryCalculatingCount();
            double bestPP = await this.CalculatePP(accuracy, failed);
            _bestPredictedPPMonitor.MinusTryCalculatingCount();
            return bestPP;
        }

        private void SetCurve(Curves curves)
        {
            this.Curve = curves.ScoreSaber.standardCurve;
            this.Slopes = this.GetSlopes();
        }

        internal double[] GetSlopes()
        {
            var slopes = new double[this.Curve.Count - 1];
            for (var i = 0; i < this.Curve.Count - 1; i++)
            {
                var x1 = this.Curve[i].x;
                var y1 = this.Curve[i].y;
                var x2 = this.Curve[i + 1].x;
                var y2 = this.Curve[i + 1].y;

                var m = (y2 - y1) / (x2 - x1);
                slopes[i] = m;
            }

            return slopes;
        }

        internal double GetCurveMultiplier(double accuracy)
        {
            if (accuracy >= this.Curve.Last().x)
            {
                return this.Curve.Last().y;
            }

            if (accuracy <= 0)
            {
                return 0f;
            }

            var i = -1;

            foreach (var point in this.Curve)
            {
                if (point.x > accuracy)
                {
                    break;
                }

                i++;
            }

            var lowerScore = this.Curve[i].x;
            var lowerGiven = this.Curve[i].y;

            return Lerp(lowerScore, lowerGiven, accuracy, i);
        }

        internal double Lerp(double x1, double y1, double x3, int i)
        {
            var m = this.Slopes[i];

            return (m * (x3 - x1)) + y1;
        }

        public class Curves
        {
            public ScoreSaber ScoreSaber { get; set; }
        }

        public class ScoreSaber
        {
            public List<Point> modifierCurve { get; set; }
            public List<Point> standardCurve { get; set; }
            public List<string> songsAllowingPositiveModifiers { get; set; }
            public ScoreSaberModifiers modifiers { get; set; }
        }

        public class Point
        {
            public float x { get; set; }
            public float y { get; set; }
        }

        public class ScoreSaberModifiers
        {
            public float da;
            public float gn;
            public float fs;
        }
    }
}
