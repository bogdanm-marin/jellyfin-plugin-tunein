using System.Text.Json.Serialization;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.TuneInMediaInfo
{
    /// <summary>
    /// Body section definition.
    /// </summary>
    public class Body
    {
        /// <summary>
        /// Gets or Sets Element.
        /// </summary>
        public string? Element { get; set; }

        /// <summary>
        /// Gets or Sets Url.
        /// </summary>
        public string? Url { get; set; }

        /// <summary>
        /// Gets or Sets Reliability.
        /// </summary>
        public int Reliability { get; set; }

        /// <summary>
        /// Gets or Sets Bitrate.
        /// </summary>
        public int Bitrate { get; set; }

        /// <summary>
        /// Gets or Sets MediaType.
        /// </summary>
        [JsonPropertyName("media_type")]
        public string? MediaType { get; set; }

        /// <summary>
        /// Gets or Sets Position.
        /// </summary>
        public int Position { get; set; }

        /// <summary>
        /// Gets or Sets PlayerWidth.
        /// </summary>
        [JsonPropertyName("player_width")]
        public int PlayerWidth { get; set; }

        /// <summary>
        /// Gets or Sets PlayerHeight.
        /// </summary>
        [JsonPropertyName("player_height")]
        public int PlayerHeight { get; set; }

        /// <summary>
        /// Gets or Sets IsHlsAdvanced.
        /// </summary>
        [JsonPropertyName("is_hls_advanced")]
        public string? IsHlsAdvanced { get; set; }

        /// <summary>
        /// Gets or Sets LiveSeekStream.
        /// </summary>
        [JsonPropertyName("live_seek_stream")]
        public string? LiveSeekStream { get; set; }

        /// <summary>
        /// Gets or Sets GuideId.
        /// </summary>
        [JsonPropertyName("guide_id")]
        public string? GuideId { get; set; }

        /// <summary>
        /// Gets or Sets IsAdClippedContentEnabled.
        /// </summary>
        [JsonPropertyName("is_ad_clipped_content_enabled")]
        public string? IsAdClippedContentEnabled { get; set; }

        /// <summary>
        /// Gets or Sets IsDirect.
        /// </summary>
        [JsonPropertyName("is_direct")]
        public bool? IsDirect { get; set; }
    }
}
