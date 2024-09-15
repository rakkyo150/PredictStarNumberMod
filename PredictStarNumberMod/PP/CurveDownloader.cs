using Newtonsoft.Json;
using PredictStarNumberMod.HarmonyPatches;
using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Zenject;
using static PredictStarNumberMod.PP.PPCalculatorData;

namespace PredictStarNumberMod.PP
{
    public class CurveDownloader: IInitializable
    {
        public bool CurvesDownloadFinished { get; private set; } = false;
        public Curves Curves { get; private set; } = null;

        private const string URI_PREFIX = "https://cdn.pulselane.dev/";
        private const string CURVE_FILE_NAME = "curves.json";

        private static HttpClient _client;
        private static HttpClient Client
        {
            get
            {
                if (_client == null)
                {
                    Connect();
                }

                return _client;
            }
        }

        public void Initialize()
        {
            try
            {
                this.CurvesDownloadFinished = false;
                GetCurves();
            }
            catch (Exception)
            {
                Plugin.Log.Error("Failed to initialize CurveDownloader");
            }
        }

        private async Task GetCurves()
        {
            string uri = URI_PREFIX + CURVE_FILE_NAME;
            Curves result = await this.MakeWebRequest<Curves>(uri);
            this.Curves = result;
            this.CurvesDownloadFinished = true;
        }

        private async Task<T> MakeWebRequest<T>(string uri)
        {
            HttpResponseMessage result = await Client.GetAsync(uri);
            if (result == null || !result.IsSuccessStatusCode)
            {
                Plugin.Log?.Error($"Failed to download {uri}");
                return default;
            }
            string jsonString = await result.Content.ReadAsStringAsync();
            Plugin.Log?.Info(jsonString);
            return JsonConvert.DeserializeObject<T>(jsonString);
        }

        private static void Connect()
        {
            _client = new HttpClient()
            {
                Timeout = new TimeSpan(0, 0, 15)
            };
            _ = _client.DefaultRequestHeaders.UserAgent.TryParseAdd($"{Assembly.GetExecutingAssembly().GetName().Name}/{Assembly.GetExecutingAssembly().GetName().Version}");
        }
    }
}
