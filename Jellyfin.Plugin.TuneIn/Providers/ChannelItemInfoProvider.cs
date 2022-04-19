using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ChannelItemInfoProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">IHttpClientFactory.</param>
        /// <param name="logger">ILogger.</param>
        public ChannelItemInfoProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<ChannelItemInfoProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Process media url and returns list of <see cref="ChannelItemInfo"/>.
        /// </summary>
        /// <param name="mediaUrl">Media Url.</param>
        /// <param name="cancellationToken">CancellationToken.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task<IEnumerable<ChannelItemInfo>> GetManyAsync(Uri mediaUrl, CancellationToken cancellationToken)
        {
            _logger.LogDebug("TuneIn url {Url}", mediaUrl);
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                var response = await httpClient.GetAsync(mediaUrl, cancellationToken).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                using (var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false))
                {
                    var xmlDocument = await XElement.LoadAsync(stream, LoadOptions.None, cancellationToken).ConfigureAwait(false);

                    var types = new HashSet<string> { "link", "audio" };
                    var items = from outline in xmlDocument.Descendants("outline")
                                let type = outline.Attribute("type")
                                let url = outline.Attribute("URL")
                                where type != null && url != null
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
                                let key = outline.Attribute("key")
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
                                    ImageUrl = image?.Value ?? GetDefaultImage(key?.Value),
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
            }
        }

        private static string? GetDefaultImage(string? name) => name switch
        {
            "presets" => "https://raw.githubusercontent.com/snazy2000/MediaBrowser.Channels/master/MediaBrowser.Plugins.TuneIn/Images/tunein-myfavs.png",
            "local" => "https://raw.githubusercontent.com/snazy2000/MediaBrowser.Channels/master/MediaBrowser.Plugins.TuneIn/Images/tunein-localradio.png",
            "music" => "https://raw.githubusercontent.com/snazy2000/MediaBrowser.Channels/master/MediaBrowser.Plugins.TuneIn/Images/tunein-music.png",
            "sports" => "https://raw.githubusercontent.com/snazy2000/MediaBrowser.Channels/master/MediaBrowser.Plugins.TuneIn/Images/tunein-sports.png",
            "location" => "https://raw.githubusercontent.com/snazy2000/MediaBrowser.Channels/master/MediaBrowser.Plugins.TuneIn/Images/tunein-bylocation.png",
            "language" => "https://raw.githubusercontent.com/snazy2000/MediaBrowser.Channels/master/MediaBrowser.Plugins.TuneIn/Images/tunein-bylanguage.png",
            "podcast" => "https://raw.githubusercontent.com/snazy2000/MediaBrowser.Channels/master/MediaBrowser.Plugins.TuneIn/Images/tunein-podcasts.png",
            "talk" => "https://raw.githubusercontent.com/snazy2000/MediaBrowser.Channels/master/MediaBrowser.Plugins.TuneIn/Images/tunein-talk.png",
            null => default,
            _ => default,
        };
    }
}
