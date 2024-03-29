using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.TuneIn.Configuration
{
    /// <summary>
    /// TuneIn plugin configuration.
    /// </summary>
    public class PluginConfiguration : BasePluginConfiguration
    {
        /// <summary>
        /// Gets or Sets TuneIn partner id.
        /// </summary>
        public string? PartnerId { get; set; }

        /// <summary>
        /// Gets or Sets TuneIn username.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// Gets or Sets User latitude or longitude.
        /// </summary>
        public string? LatitudeLongitude { get; set; }

        /// <summary>
        /// Gets or Sets Ads filters.
        /// </summary>
        public string? FilterUrls { get; set; } = "ads.cust_params;ads_partner_alias;ads.stationId";

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"{PartnerId}-{Username}-{LatitudeLongitude}-{FilterUrls}";
        }
    }
}
