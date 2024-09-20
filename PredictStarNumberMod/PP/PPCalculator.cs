using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PredictStarNumberMod.PP
{
    public class PPCalculator
    {
        public double PredictedPP { get; set; }

        internal List<Point> Curve { get; set; }
        internal double[] Slopes { get; set; }

        internal double NoPredictedPP { get; } = -1;
        internal double DefaultStarMultipllier { get; } = 42.11;
        internal double Multiplier { get; } = 1;

        private readonly Star.Star _star;
        private readonly CurveDownloader _curveDownloader;

        public PPCalculator(Star.Star star, CurveDownloader curveDownloader)
        {
            _star = star;
            _curveDownloader = curveDownloader;
        }

        public async Task<double> CalculatePP(double accuracy, bool failed = false)
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

            double predictedStarNumber = await _star.GetPredictedStarNumber();
            double rawPP = predictedStarNumber * this.DefaultStarMultipllier;

            double multiplier = this.Multiplier;
            if (failed)
            {
                multiplier -= 0.5f;
            }

            return rawPP * this.GetCurveMultiplier(accuracy * multiplier);
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
