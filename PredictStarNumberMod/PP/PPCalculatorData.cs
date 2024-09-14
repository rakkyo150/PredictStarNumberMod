using System;
using System.Collections.Generic;

namespace PredictStarNumberMod.PP
{
    public class PPCalculatorData
    {
        public static double DefaultStarMultipllier { get; } = 42.11;

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
