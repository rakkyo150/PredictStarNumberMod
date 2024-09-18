using IPA.Config.Stores;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace PredictStarNumberCounter.Configuration
{
    internal class PluginConfig
    {
        public static PluginConfig Instance { get; set; }
        public virtual int DecimalPrecision { get; set; } = 2;
        public virtual bool EnableLabel { get; set; } = true;
        public virtual float LabelFontSize { get; set; } = 3f;
        public virtual float FigureFontSize { get; set; } = 4f;
        public virtual float OffsetX { get; set; } = 0f;
        public virtual float OffsetY { get; set; } = 0f;
        public virtual float OffsetZ { get; set; } = 0f;

        /// <summary>
        /// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
        /// </summary>
        public virtual void OnReload()
        {
            // Do stuff after config is read from disk.
        }

        /// <summary>
        /// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
        /// </summary>
        public virtual void Changed()
        {
            // Do stuff when the config is changed.
        }

        /// <summary>
        /// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
        /// </summary>
        public virtual void CopyFrom(PluginConfig other)
        {
            // This instance's members populated from other
            this.DecimalPrecision = other.DecimalPrecision;
            this.EnableLabel = other.EnableLabel;
            this.LabelFontSize = other.LabelFontSize;
            this.FigureFontSize = other.FigureFontSize;
            this.OffsetX = other.OffsetX;
            this.OffsetY = other.OffsetY;
            this.OffsetZ = other.OffsetZ;
        }
    }
}
