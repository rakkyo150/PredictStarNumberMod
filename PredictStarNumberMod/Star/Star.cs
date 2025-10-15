using CsBindgen;
using PredictStarNumberMod.Map;
using PredictStarNumberMod.Utilities;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace PredictStarNumberMod.Star
{
    public class Star
    {
        public double SkipStarNumber { get; } = -1.0;
        public double ErrorStarNumber { get; } = -10.0;

        private double predictedStarNumber = double.MinValue;
        private string previoudMapHash = string.Empty;
        private BeatmapDifficulty preciousBeatmapDifficulty;
        private BeatmapCharacteristicSO previousCharacteristic;

        public Action<double> ChangedPredictedStarNumber;

        private readonly Object lockPredictedStarNumber = new Object();
        private readonly Object lockMapData = new Object();
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly OrderedAsyncTaskQueue<double> _orderedAsyncTaskQueue = new OrderedAsyncTaskQueue<double>();

        private readonly Model.Model _model;
        private readonly MapDataContainer _mapDataContainer;

        public Star(Model.Model model, MapDataContainer mapDataContainer)
        {
            _model = model;
            _mapDataContainer = mapDataContainer;
        }

        internal void SetPredictedStarNumber(double newPredictedStarNumber)
        {
            lock (lockPredictedStarNumber)
            {
                this.predictedStarNumber = newPredictedStarNumber;
                this.ChangedPredictedStarNumber?.Invoke(this.predictedStarNumber);
#if DEBUG
                Plugin.Log.Info($"predictedStarNumber Changed : newPredictedStarNumber=={newPredictedStarNumber}");
#endif
            }
        }

        public async Task<double> GetPredictedStarNumberAfterWaitingQueue()
        {
            await _orderedAsyncTaskQueue.WaitUntilQueueEmptyAsync();

            lock (lockPredictedStarNumber)
            {
                return this.predictedStarNumber;
            }
        }

        public async Task<double> AddQueuePredictingAndSettingStarNumber()
        {
            return await _orderedAsyncTaskQueue.StartTaskAsync(async () =>
            {
                double predictedStarNumber = await PredictStarNumber();
                SetPredictedStarNumber(predictedStarNumber);
                return predictedStarNumber;
            });
        }

        internal void RefreshPreviousMadDataForPredictingStarNumber()
        {
            lock (lockMapData)
            {
                previoudMapHash = _mapDataContainer.MapHash;
                preciousBeatmapDifficulty = _mapDataContainer.BeatmapDifficulty;
                previousCharacteristic = _mapDataContainer.Characteristic;
            }
        }

        internal async Task<double> AddQueueSettingSkipStarNumber()
        {
            return await _orderedAsyncTaskQueue.StartTaskAsync(async () =>
            {
                SetPredictedStarNumber(this.SkipStarNumber);
                return this.SkipStarNumber;
            });
        }

        private async Task<double> PredictStarNumber()
        {
#if DEBUG
            var sw = new System.Diagnostics.Stopwatch();
            sw.Start();
#endif
            try 
            {
#if DEBUG
                Plugin.Log.Info($"previoudMapHash : {previoudMapHash}, _mapDataContainer.MapHash : {_mapDataContainer.MapHash}, preciousBeatmapDifficulty : {preciousBeatmapDifficulty}, _mapDataContainer.BeatmapDifficulty : {_mapDataContainer.BeatmapDifficulty}, previousCharacteristic : {previousCharacteristic}, _mapDataContainer.Characteristic : {_mapDataContainer.Characteristic}");
#endif
                if (previoudMapHash == _mapDataContainer.MapHash && preciousBeatmapDifficulty == _mapDataContainer.BeatmapDifficulty
                    && previousCharacteristic == _mapDataContainer.Characteristic)
                {
#if DEBUG
                    Plugin.Log.Info("Same data");
#endif
                    return this.predictedStarNumber;
                }

                this.RefreshPreviousMadDataForPredictingStarNumber();

                if (_model.ModelByte.Length == 1)
                {
                    _model.ModelByte = await _model.GetModel();
                }

                _mapDataContainer.Data = await _mapDataContainer.GetMapData(_mapDataContainer.MapHash, _mapDataContainer.BeatmapDifficulty, _mapDataContainer.Characteristic);


                if (_mapDataContainer.Data == _mapDataContainer.NoMapData) return this.SkipStarNumber;

                double[] data = new double[15]
                {
                    _mapDataContainer.Data.Bpm,
                    _mapDataContainer.Data.Duration,
                    _mapDataContainer.Data.Difficulty,
                    _mapDataContainer.Data.SageScore,
                    _mapDataContainer.Data.Njs,
                    _mapDataContainer.Data.Offset,
                    _mapDataContainer.Data.Notes,
                    _mapDataContainer.Data.Bombs,
                    _mapDataContainer.Data.Obstacles,
                    _mapDataContainer.Data.Nps,
                    _mapDataContainer.Data.Events,
                    _mapDataContainer.Data.Chroma,
                    _mapDataContainer.Data.Errors,
                    _mapDataContainer.Data.Warns,
                    _mapDataContainer.Data.Resets
                };

                unsafe
                {
                    fixed (double* record = data) // 配列の固定ピン留め                                                                                  
                    {
                        fixed (byte* model = _model.ModelByte)
                        {
                            return NativeMethods.get_predicted_values(record, (UIntPtr)data.Length, model, (UIntPtr)_model.ModelByte.Length);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex);
                return this.ErrorStarNumber;
            }
            finally
            {
#if DEBUG
                // Plugin.Log.Info(string.Join(". ", results));
                sw.Stop();
                Plugin.Log.Info("Elapsed : " + sw.Elapsed.ToString());
#endif
            }
        }
    }
}
