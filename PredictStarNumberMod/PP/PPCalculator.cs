using System;
using System.Collections.Generic;
using System.Linq;

namespace PredictStarNumberMod.PP
{
    public class PPCalculator
    {
        internal List<Point> Curve { get; set; }
        internal double[] Slopes { get; set; }

        internal double DefaultStarMultipllier { get; } = 42.11;
        internal double Multiplier { get; } = 1;

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
