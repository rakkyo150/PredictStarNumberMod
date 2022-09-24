namespace PredictStarNumberMod
{
    internal class MapDataDeliverer : PersistentSingleton<MapDataDeliverer>
    {
        private string hash = string.Empty;
        private string mapType = string.Empty;

        public string Hash => hash;
        public string MapType => mapType;

        internal void SetHash(string hash)
        {
            this.hash = hash;
        }

        internal void SetMapType(string mapType)
        {
            this.mapType = mapType;
        }
    }
}
