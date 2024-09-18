using Microsoft.ML.OnnxRuntime;

namespace PredictStarNumberMod.Model
{
    public class Model
    {
        internal byte[] ModelByte { get; set; } = new byte[] { 0 };
        internal InferenceSession Session { get; set; } = null;

        internal string ModelAssetEndpoint { get; } = "https://api.github.com/repos/rakkyo150/PredictStarNumberHelper/releases/latest";
    }
}
