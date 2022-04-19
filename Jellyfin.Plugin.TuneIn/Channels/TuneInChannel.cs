using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Jellyfin.Plugin.TuneIn.Providers;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Controller.Providers;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TuneIn.Channels
{
    /// <summary>
    /// TuneInChannel.
    /// </summary>
    public class TuneInChannel : IChannel, IRequiresMediaInfoCallback, IHasCacheKey, ISupportsMediaProbe
    {
#pragma warning disable S1075 // URIs should not be hardcoded
        private const string BaseUri = "http://opml.radiotime.com/Browse.ashx";
#pragma warning restore S1075 // URIs should not be hardcoded
        private readonly ChannelItemInfoProvider _opmlProcessor;
        private readonly MediaSourceInfoProvider _mediaSourceInfoProvider;
        private readonly Plugin _plugin;
        private readonly ILogger<TuneInChannel> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TuneInChannel"/> class.
        /// </summary>
        /// <param name="opmlProcessor">OpmlProcessor.</param>
        /// <param name="mediaSourceInfoProvider">MediaSourceInfoProvider.</param>
        /// <param name="plugin">Plugin Instance.</param>
        /// <param name="logger">ILogger.</param>
        public TuneInChannel(
            ChannelItemInfoProvider opmlProcessor,
            MediaSourceInfoProvider mediaSourceInfoProvider,
            Plugin plugin,
            ILogger<TuneInChannel> logger)
        {
            _opmlProcessor = opmlProcessor;
            _mediaSourceInfoProvider = mediaSourceInfoProvider;
            _plugin = plugin;
            _logger = logger;
        }

        /// <inheritdoc/>
        public string Name => "New TuneIn";

        /// <inheritdoc/>
        public string Description => "Listen to online radio, find streaming music radio and streaming talk radio with TuneIn.";

        /// <inheritdoc/>
        public string DataVersion => "1.5";

        /// <inheritdoc/>
        public string HomePageUrl => "https://www.tunein.com/";

        /// <inheritdoc/>
        public ChannelParentalRating ParentalRating => ChannelParentalRating.GeneralAudience;

        /// <inheritdoc/>
        public InternalChannelFeatures GetChannelFeatures()
        {
            return new InternalChannelFeatures
            {
                ContentTypes = new List<ChannelMediaContentType>
                {
                    ChannelMediaContentType.Song,
                    ChannelMediaContentType.Podcast,
                    ChannelMediaContentType.Episode,
                },
                MediaTypes = new List<ChannelMediaType> { ChannelMediaType.Audio }
            };
        }

        /// <inheritdoc/>
        public Task<DynamicImageResponse> GetChannelImage(ImageType type, CancellationToken cancellationToken)
        {
            switch (type)
            {
                case ImageType.Backdrop:
                case ImageType.Primary:
                case ImageType.Thumb:
                    var imagePath = $"{typeof(Plugin).Namespace}.Images.{type}.png";
                    var response = new DynamicImageResponse
                    {
                        Format = ImageFormat.Png,
                        HasImage = true,
                        Stream = GetType().Assembly.GetManifestResourceStream(imagePath)
                    };

                    return Task.FromResult(response);
                default:
                    throw new ArgumentException($"Unsupperted image type {type}");
            }
        }

        /// <inheritdoc/>
        public IEnumerable<ImageType> GetSupportedChannelImages()
        {
            return new[]
            {
                ImageType.Backdrop,
                ImageType.Primary,
                ImageType.Thumb
            };
        }

        /// <inheritdoc/>
        public async Task<ChannelItemResult> GetChannelItems(InternalChannelItemQuery query, CancellationToken cancellationToken)
        {
            using (_logger.BeginScope("GetChannelItems {FolderId}", query.FolderId))
            {
                Uri uri;
                if (!string.IsNullOrEmpty(query.FolderId))
                {
                    uri = new Uri(query.FolderId);
                }
                else
                {
                    var uriBuilder = new UriBuilder(BaseUri);

                    var queryParams = HttpUtility.ParseQueryString(uriBuilder.Query);
                    queryParams.Add("formats", "mp3,aac,ogg,hls");
                    queryParams.Add("partnerId", "uD1X52pA");
                    queryParams.Add("username", _plugin.Configuration.Username);

                    uriBuilder.Query = queryParams.ToString();
                    uri = uriBuilder.Uri;
                }

                try
                {
                    var results = await _opmlProcessor.GetManyAsync(uri, cancellationToken)
                                    .ConfigureAwait(false);

                    var source = results
                        .OrderByDescending(t => t.Type == ChannelItemType.Folder);

                    source = query.SortBy switch
                    {
                        ChannelItemSortField.Name => query.SortDescending switch
                        {
                            true => source.ThenByDescending(p => p.Name),
                            false => source.ThenBy(p => p.Name),
                        },
                        ChannelItemSortField.CommunityRating => query.SortDescending switch
                        {
                            true => source.ThenByDescending(p => p.CommunityRating),
                            false => source.ThenBy(p => p.CommunityRating),
                        },
                        _ => source,
                    };

                    var items = source.ToList();
                    return new ChannelItemResult
                    {
                        Items = items
                    };
                }
                catch (AggregateException aggEx)
                {
                    foreach (var ex in aggEx.Flatten().InnerExceptions)
                    {
                        _logger.LogError(ex.GetBaseException(), "{Message}", ex.Message);
                    }

                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.GetBaseException(), "{Message}", ex.Message);
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public bool IsEnabledFor(string userId)
        {
            return true;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<MediaSourceInfo>> GetChannelItemMediaInfo(string id, CancellationToken cancellationToken)
        {
            var uri = new Uri(id);
            using (_logger.BeginScope("GetChannelItemMediaInfo {Uri}", uri))
            {
                try
                {
                    var items = await _mediaSourceInfoProvider
                                        .GetManyAsync(uri, cancellationToken)
                                        .ToListAsync(cancellationToken)
                                        .ConfigureAwait(false);

                    return items;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Message}", ex.Message);
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public string GetCacheKey(string userId)
        {
            return $"{Name}-{userId}-{_plugin.Configuration.Username}-{DataVersion}";
        }
    }
}
