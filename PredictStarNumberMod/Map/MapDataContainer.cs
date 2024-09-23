using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace PredictStarNumberMod.Map
{
    public class MapDataContainer
    {
        internal MapData NoMapData { get; } = new MapData(float.MinValue, float.MinValue, int.MinValue,
            int.MinValue, float.MinValue, float.MinValue, int.MinValue, int.MinValue, int.MinValue, float.MinValue, int.MinValue,
            int.MinValue, int.MinValue, int.MinValue, int.MinValue);
        
        internal string MapHash { get; set; } = string.Empty;
        internal BeatmapDifficulty BeatmapDifficulty { get; set; } = BeatmapDifficulty.Easy;
        internal BeatmapCharacteristicSO Characteristic { get; set; } = null;

        internal MapData Data { get; set; } = new MapData(float.MinValue, float.MinValue, int.MinValue,
            int.MinValue, float.MinValue, float.MinValue, int.MinValue, int.MinValue, int.MinValue, float.MinValue, int.MinValue,
            int.MinValue, int.MinValue, int.MinValue, int.MinValue);

        internal async Task<MapData> GetMapData(string hash, BeatmapDifficulty beatmapDifficulty, BeatmapCharacteristicSO characteristic)
        {
            string endpoint = $"https://api.beatsaver.com/maps/hash/{hash}";

            float bpm = float.MinValue;
            float duration = float.MinValue;
            int difficulty = int.MinValue;
            int sageScore = int.MinValue;
            float njs = float.MinValue;
            float offset = float.MinValue;
            int notes = int.MinValue;
            int bombs = int.MinValue;
            int obstacles = int.MinValue;
            float nps = float.MinValue;
            int events = int.MinValue;
            int chroma = int.MinValue;
            int errors = int.MinValue;
            int warns = int.MinValue;
            int resets = int.MinValue;

            HttpClient client = new HttpClient();
            var response = await client.GetAsync(endpoint);
            string jsonString = await response.Content.ReadAsStringAsync();
            MapDetail mapDetail = JsonConvert.DeserializeObject<MapDetail>(jsonString);
            client.Dispose();

            if (mapDetail.versions == null) return NoMapData;

            IList<Version> versions = mapDetail.versions;
            IList<Difficulty> difficulties = versions[versions.Count - 1].diffs;

            foreach (Difficulty eachDifficulty in difficulties)
            {
                if (eachDifficulty.difficulty != beatmapDifficulty.ToString()
                    || eachDifficulty.characteristic != characteristic.serializedName)
                {
                    continue;
                }

                bpm = mapDetail.metadata.bpm;
                duration = mapDetail.metadata.duration;
                difficulty = (int)beatmapDifficulty;
#if DEBUG
                Plugin.Log.Info("Difficulty : " + difficulty.ToString());
#endif
                if (versions[versions.Count - 1].sageScore != null)
                {
                    sageScore = (int)versions[versions.Count - 1].sageScore;
                }
                else
                {
                    sageScore = 0;
                }
                njs = eachDifficulty.njs;
                offset = eachDifficulty.offset;
                notes = eachDifficulty.notes;
                bombs = eachDifficulty.bombs;
                obstacles = eachDifficulty.obstacles;
                nps = eachDifficulty.nps;
                events = eachDifficulty.events;
                chroma = eachDifficulty.chroma ? 1 : 0;
                errors = eachDifficulty.paritySummary.errors;
                warns = eachDifficulty.paritySummary.warns;
                resets = eachDifficulty.paritySummary.resets;
                break;
            }

            return new MapData(bpm, duration, difficulty, sageScore, njs, offset, notes, bombs, obstacles,
                nps, events, chroma, errors, warns, resets);
        }

        internal class MapData
        {
            float bpm;
            float duration;
            int difficulty;
            int sageScore;
            float njs;
            float offset;
            int notes;
            int bombs;
            int obstacles;
            float nps;
            int events;
            int chroma;
            int errors;
            int warns;
            int resets;

            internal float Bpm => bpm;
            internal float Duration => duration;
            internal int Difficulty => difficulty;
            internal int SageScore => sageScore;
            internal float Njs => njs;
            internal float Offset => offset;
            internal int Notes => notes;
            internal int Bombs => bombs;
            internal int Obstacles => obstacles;
            internal float Nps => nps;
            internal int Events => events;
            internal int Chroma => chroma;
            internal int Errors => errors;
            internal int Warns => warns;
            internal int Resets => resets;

            internal MapData(float bpm, float duration, int difficulty, int sageScore, float njs, float offset,
                int notes, int bombs, int obstacles, float nps, int events, int chroma, int errors,
                int warns, int resets)
            {
                this.bpm = bpm;
                this.duration = duration;
                this.difficulty = difficulty;
                this.sageScore = sageScore;
                this.njs = njs;
                this.offset = offset;
                this.notes = notes;
                this.bombs = bombs;
                this.obstacles = obstacles;
                this.nps = nps;
                this.events = events;
                this.chroma = chroma;
                this.errors = errors;
                this.warns = warns;
                this.resets = resets;
            }
        }

        public class MapDetail
        {
            public Metadata metadata { get; set; }
            public IList<Version> versions { get; set; }
        }

        public class Metadata
        {
            public float bpm { get; set; }
            public float duration { get; set; }
        }

        public class Version
        {
            public int? sageScore { get; set; }
            public IList<Difficulty> diffs { get; set; }
        }

        public class Difficulty
        {
            public float njs { get; set; }
            public float offset { get; set; }
            public int notes { get; set; }
            public int bombs { get; set; }
            public int obstacles { get; set; }
            public float nps { get; set; }
            public string characteristic { get; set; }
            public string difficulty { get; set; }
            public int events { get; set; }
            public bool chroma { get; set; }
            public ParitySummary paritySummary { get; set; }
        }

        public class ParitySummary
        {
            public int errors { get; set; }
            public int warns { get; set; }
            public int resets { get; set; }
        }
    }
}
