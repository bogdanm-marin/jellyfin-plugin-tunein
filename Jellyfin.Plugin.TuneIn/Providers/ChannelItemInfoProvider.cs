using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Linq;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Model.Channels;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TuneIn.Providers
{
    /// <summary>
    /// OpmlProcessor.
    /// </summary>
    public class ChannelItemInfoProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ChannelItemInfoProvider> _logger;
        private readonly string _localUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelItemInfoProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">IHttpClientFactory.</param>
        /// <param name="serverApplicationHost"><see cref="IServerApplicationHost"/> instance.</param>
        /// <param name="logger">ILogger.</param>
        public ChannelItemInfoProvider(
            IHttpClientFactory httpClientFactory,
            [NotNull] IServerApplicationHost serverApplicationHost,
            ILogger<ChannelItemInfoProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;

            _localUrl = serverApplicationHost.GetLoopbackHttpApiUrl();
        }

        /// <summary>
        /// Process media url and returns list of <see cref="ChannelItemInfo"/>.
        /// </summary>
        /// <param name="mediaUrl">Media Url.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<ChannelItemInfo>> GetManyAsync(Uri mediaUrl, CancellationToken cancellationToken)
        {
            const string Unavailable = "unavailable";
            _logger.LogDebug("TuneIn url {Url}", mediaUrl);
            using var httpClient = _httpClientFactory.CreateClient();
            var response = await httpClient.GetAsync(mediaUrl, cancellationToken).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var xmlDocument = await XElement.LoadAsync(stream, LoadOptions.None, cancellationToken).ConfigureAwait(false);

            var types = new HashSet<string> { "link", "audio" };
            var items = from outline in xmlDocument.Descendants("outline")
                        let type = outline.Attribute("type")
                        let url = outline.Attribute("URL")
                        let key = outline.Attribute("key")
                        where type != null && url != null && !Unavailable.Equals(key?.Value, StringComparison.OrdinalIgnoreCase)
                        where types.Contains(type.Value)
                        let text = outline.Attribute("text")
                        let image = outline.Attribute("image")
                        let subtext = outline.Attribute("subtext")
                        let bitrare = outline.Attribute("bitrate")
                        let reliability = outline.Attribute("reliability")
                        let formats = outline.Attribute("formats")
                        let current_track = outline.Attribute("current_track")
                        let item = outline.Attribute("item")
                        let stream_type = outline.Attribute("stream_type")
                        let playing_image = outline.Attribute("playing_image")
                        select new ChannelItemInfo
                        {
                            Id = url?.Value,
                            Name = text?.Value,

                            Type = type.Value switch
                            {
                                "link" => ChannelItemType.Folder,
                                "audio" => ChannelItemType.Media,
                                _ => default
                            },
                            ImageUrl = image?.Value ?? GetDefaultImage(key?.Value) ?? GetDynamicImage(text?.Value),
                            ContentType = type.Value switch
                            {
                                "audio" => item?.Value switch
                                {
                                    "station" => ChannelMediaContentType.Song,
                                    "topic" => ChannelMediaContentType.Podcast,
                                    _ => default,
                                },
                                "link" => item?.Value switch
                                {
                                    "show" => ChannelMediaContentType.Podcast,
                                    _ => default,
                                },
                                _ => default
                            },
                            MediaType = ChannelMediaType.Audio,
                            OriginalTitle = current_track?.Value,
                            CommunityRating = (int?)reliability,
                            OfficialRating = reliability?.Value,
                        };

            return items;
        }

        private string? GetDefaultImage(string? name) => name switch
        {
            "presets" => $"{_localUrl}/api/v1/TuneIn/Image/tunein-myfavs.png",
            "local" => $"{_localUrl}/api/v1/TuneIn/Image/tunein-localradio.png",
            "music" => $"{_localUrl}/api/v1/TuneIn/Image/tunein-music.png",
            "sports" => $"{_localUrl}/api/v1/TuneIn/Image/tunein-sports.png",
            "location" => $"{_localUrl}/api/v1/TuneIn/Image/tunein-bylocation.png",
            "language" => $"{_localUrl}/api/v1/TuneIn/Image/tunein-bylanguage.png",
            "podcast" => $"{_localUrl}/api/v1/TuneIn/Image/tunein-podcasts.png",
            "talk" => $"{_localUrl}/api/v1/TuneIn/Image/tunein-talk.png",
            null => default,
            _ => default,
        };

        private string? GetDynamicImage(string? name, int width = 480, int height = 480, float fontSize = 36, string format = "png", bool increaseFontSizeSingleNameCharacter = true)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return default;
            }

            if (name.Length == 1 && increaseFontSizeSingleNameCharacter)
            {
                fontSize *= 1.5f;
            }

            var encodedName = HttpUtility.UrlEncode(name);

            return $"{_localUrl}/api/v1/TuneIn/Image/generate/{encodedName}-w{width}-h{height}-fs{fontSize}.{format}";
        }
    }
}
