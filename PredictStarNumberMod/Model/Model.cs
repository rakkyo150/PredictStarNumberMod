using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PredictStarNumberMod.Model
{
    public class Model
    {
        internal byte[] ModelByte { get; set; } = new byte[] { 0 };

        internal string ModelEndpoint { get; } = "https://github.com/rakkyo150/PredictStarNumberHelper/releases/latest/download/model.onnx";

        internal async Task<byte[]> GetModel()
        {
            HttpClient client = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, this.ModelEndpoint);
            request.Headers.Add("User-Agent", "PredictStarNumberMod");
            HttpResponseMessage modelResponse = await client.SendAsync(request);
            client.Dispose();
            request.Dispose();
            return await modelResponse.Content.ReadAsByteArrayAsync();
        }

        public class LatestRelease
        {
            public List<DownloadUrl> assets;
        }

        public class DownloadUrl
        {
            public string browser_download_url;
        }
    }
}
