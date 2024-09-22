using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.OnnxRuntime;
using PredictStarNumberMod.Map;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using PredictStarNumberMod.Utilities;

namespace PredictStarNumberMod.Star
{
    public class Star
    {
        public double SkipStarNumber { get; } = -1.0;
        public double ErrorStarNumber { get; } = -10.0;

        // 共有メモリ
        private double predictedStarNumber = double.MinValue;

        public Action<double> ChangedPredictedStarNumber;
        //　共有メモリここまで

        private readonly Object lockObject = new Object();
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
            lock (lockObject)
            {
                this.predictedStarNumber = newPredictedStarNumber;
                this.ChangedPredictedStarNumber?.Invoke(this.predictedStarNumber);
#if DEBUG
                Plugin.Log.Info($"predictedStarNumber Changed : newPredictedStarNumber=={newPredictedStarNumber}");
#endif
            }
        }

        public async Task<double> GetPredictedStarNumber()
        {
            await _orderedAsyncTaskQueue.WaitUntilQueueEmptyAsync();
            
            lock (lockObject)
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

        private async Task<double> PredictStarNumber()
        {
            try
            {
#if DEBUG
                var sw = new System.Diagnostics.Stopwatch();
                sw.Start();
#endif

                if (_model.ModelByte.Length == 1)
                {
                    _model.ModelByte = await _model.GetModel();
                }
                _mapDataContainer.Data = await _mapDataContainer.GetMapData(_mapDataContainer.MapHash, _mapDataContainer.BeatmapDifficulty, _mapDataContainer.Characteristic);
                if (_model.Session == null)
                {
                    _model.Session = new InferenceSession(_model.ModelByte);
                }
                string inputNoneName = _model.Session?.InputMetadata.First().Key;
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
#if DEBUG
                var innodedims = _model.Session?.InputMetadata.First().Value.Dimensions;
                // Plugin.Log.Info(string.Join(", ", innodedims));
                // Plugin.Log.Info(string.Join(". ", data));
#endif
                var inputTensor = new DenseTensor<double>(data, new int[] { 1, data.Length }, false);  // let's say data is fed into the Tensor objects
                List<NamedOnnxValue> inputs = new List<NamedOnnxValue>()
                    {
                        NamedOnnxValue.CreateFromTensor<double>(inputNoneName, inputTensor)
                    };
                using (var results = _model.Session?.Run(inputs))
                {
#if DEBUG
                    // Plugin.Log.Info(string.Join(". ", results));
                    sw.Stop();
                    Plugin.Log.Info("Elapsed : " + sw.Elapsed.ToString());
#endif
                    return results.First().AsTensor<double>()[0];
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.Error(ex);
                return this.ErrorStarNumber;
            }
        }
    }
}
