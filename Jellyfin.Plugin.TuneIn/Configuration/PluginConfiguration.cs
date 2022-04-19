using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TuneIn.Configuration
{
    /// <summary>
    /// PluginConfiguration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or Sets TuneIn username.
        /// </summary>
        public string? Username { get; set; }
    }
}
