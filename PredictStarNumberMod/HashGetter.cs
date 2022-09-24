using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredictStarNumberMod
{
    internal static class HashGetter
    {
        public static string GetHashOfPreview(IPreviewBeatmapLevel preview)
        {
            if (preview.levelID.Length < 53)
                return null;

            if (preview.levelID[12] != '_') // custom_level_<hash, 40 chars>
                return null;

            return preview.levelID.Substring(13, 40);
        }
    }
}
