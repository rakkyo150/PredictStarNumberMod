using Microsoft.ML.OnnxRuntime;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;
using static PredictStarNumberMod.HarmonyPatches.StarNumberSetter;

namespace PredictStarNumberMod.Model
{
    public class Model
    {
        internal byte[] ModelByte { get; set; } = new byte[] { 0 };
        internal InferenceSession Session { get; set; } = null;

        internal string ModelAssetEndpoint { get; } = "https://api.github.com/repos/rakkyo150/PredictStarNumberHelper/releases/latest";

        internal async Task<byte[]> GetModel()
        {
            HttpClient client = new HttpClient();
            var request = new HttpRequestMessage(HttpMethod.Get, this.ModelAssetEndpoint);
            request.Headers.Add("User-Agent", "C# App");
            var response = await client.SendAsync(request);
            string assetString = await response.Content.ReadAsStringAsync();
            LatestRelease latestRelease = JsonConvert.DeserializeObject<LatestRelease>(assetString);
            string modelDownloadUrl = latestRelease.assets[2].browser_download_url;
#if DEBUG
            Plugin.Log.Info(modelDownloadUrl);
#endif
            request = new HttpRequestMessage(HttpMethod.Get, modelDownloadUrl);
            request.Headers.Add("User-Agent", "C# App");
            var modelResponse = await client.SendAsync(request);
            client.Dispose();
            request.Dispose();
            return await modelResponse.Content.ReadAsByteArrayAsync();
        }
    }
}
