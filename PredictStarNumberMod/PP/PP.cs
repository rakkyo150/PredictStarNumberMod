using PredictStarNumberMod.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PredictStarNumberMod.PP
{
    public class PP
    {
        internal double DefaultStarMultipllier { get; } = 42.11;
        internal double Multiplier { get; } = 1;

        public double NoPredictedPP { get; } = -1;

        private double bestPredictedPP;
        private double accuracy;
        internal List<Point> Curve { get; set; }
        internal double[] Slopes { get; set; }

        public Action<double> ChangedBestPredictedPP;

        private readonly Object lockObject = new Object();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly SemaphoreSlim semaphore_for_calculate = new SemaphoreSlim(1);
        private readonly OrderedAsyncTaskQueue<double> _orderedAsyncTaskQueue = new OrderedAsyncTaskQueue<double>();

        private readonly Star.Star _star;
        private readonly CurveDownloader _curveDownloader;

        public PP(Star.Star star, CurveDownloader curveDownloader)
        {
            _star = star;
            _curveDownloader = curveDownloader;
        }

        internal void SetBestPredictedPP(double newPredictedPP)
        {
            lock (lockObject)
            {
                this.bestPredictedPP = newPredictedPP;
                this.ChangedBestPredictedPP?.Invoke(this.bestPredictedPP);
#if DEBUG
                Plugin.Log.Info($"predictedPP Changed : newPredictedPP=={newPredictedPP}");
#endif
            }
        }

        internal void SetAccuracy(double newAccuracy)
        {
            lock (lockObject)
            {
                this.accuracy = newAccuracy;
            }
        }

        public async Task<double> GetBestPredictedPPAfterWaitingQueue()
        {
            await _orderedAsyncTaskQueue.WaitUntilQueueEmptyAsync();

            lock (lockObject)
            {
                return this.bestPredictedPP;
            }
        }

        public async Task<double> AddQueueCalculatingAndSettingBestPP(bool failed = false)
        {
            return await _orderedAsyncTaskQueue.StartTaskAsync(async () =>
            {
                double bestPP = await CalculateBestPP(failed);
                SetBestPredictedPP(bestPP);
                return bestPP;
            });
        }

        internal async Task<double> CalculateBestPP(bool failed = false)
        {
            await semaphore.WaitAsync();
            try
            {
                double bestPP = await this.CalculatePP(this.accuracy, failed);
                return bestPP;
            }
            finally
            {
                semaphore.Release();
            }
        }

        public async Task<double> CalculatePP(double accuracy, bool failed = false)
        {
            try
            {
                await semaphore_for_calculate.WaitAsync();

                if (this.Curve == null || this.Slopes == null)
                {
                    while (!_curveDownloader.CurvesDownloadFinished)
                    {
#if DEBUG
                        Plugin.Log?.Info("Waiting for CurveDownloader to initialize");
#endif
                        await Task.Delay(300);
                    }
                    SetCurve(_curveDownloader.Curves);
                }

                double predictedStarNumber = await _star.GetPredictedStarNumberAfterWaitingQueue();
                Plugin.Log.Info($"predictedStarNumber at CalculatePP : {predictedStarNumber}");

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
            finally
            {
                semaphore_for_calculate.Release();
            }
        }

        private void SetCurve(Curves curves)
        {
            lock (lockObject)
            {
                this.Curve = curves.ScoreSaber.standardCurve;
                this.Slopes = this.GetSlopes();
            }
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
