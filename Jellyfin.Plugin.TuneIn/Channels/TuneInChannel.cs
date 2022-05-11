using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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
    /// TuneIn Channel provides Channel items metadata.
    /// </summary>
    public class TuneInChannel : IChannel, IRequiresMediaInfoCallback, IHasCacheKey, ISupportsMediaProbe, ISupportsLatestMedia, ISearchableChannel
    {
        private readonly ChannelItemInfoProvider _opmlProcessor;
        private readonly MediaSourceInfoProvider _mediaSourceInfoProvider;
        private readonly TuneInUriProvider _tuneInUriProvider;
        private readonly Plugin _plugin;
        private readonly ILogger<TuneInChannel> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TuneInChannel"/> class.
        /// </summary>
        /// <param name="opmlProcessor">OpmlProcessor.</param>
        /// <param name="mediaSourceInfoProvider">MediaSourceInfoProvider.</param>
        /// <param name="tuneInUriProvider">TuneIn Uri Provider.</param>
        /// <param name="plugin">Plugin instance.</param>
        /// <param name="logger">ILogger.</param>
        public TuneInChannel(
            ChannelItemInfoProvider opmlProcessor,
            MediaSourceInfoProvider mediaSourceInfoProvider,
            TuneInUriProvider tuneInUriProvider,
            Plugin plugin,
            ILogger<TuneInChannel> logger)
        {
            _opmlProcessor = opmlProcessor;
            _mediaSourceInfoProvider = mediaSourceInfoProvider;
            _tuneInUriProvider = tuneInUriProvider;
            _plugin = plugin;
            _logger = logger;
        }

        /// <inheritdoc/>
        public string Name => "Tune In";

        /// <inheritdoc/>
        public string Description => "Listen to online radio, find streaming music radio and streaming talk radio with TuneIn.";

        /// <inheritdoc/>
        public string DataVersion { get; set; } = typeof(TuneInChannel).Assembly.GetName()!.Version!.ToString();

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
            using (_logger.BeginScope("{Method} {ImageType}", nameof(GetChannelImage), type))
            {
                switch (type)
                {
                    case ImageType.Backdrop:
                    case ImageType.Primary:
                    case ImageType.Thumb:
                        var imagePath = $"{typeof(Plugin).Namespace}.Images.{type}.png";

                        _logger.LogDebug("{ImagePath}", imagePath);

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
            using (_logger.BeginScope("{Method} {FolderId}", nameof(GetChannelItems), query.FolderId))
            {
                Uri uri;
                if (!string.IsNullOrEmpty(query.FolderId))
                {
                    uri = new Uri(query.FolderId);
                }
                else
                {
                    uri = _tuneInUriProvider.BrowseUri;
                }

                _logger.LogDebug("{RequestUri}", uri);

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

                    _logger.LogDebug("{Method} results {Count}", nameof(GetChannelItems), items?.Count);

                    var result = new ChannelItemResult();

                    if (items is not null)
                    {
                        result.Items = items;
                        result.TotalRecordCount = items.Count;
                    }

                    return result;
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
            using (_logger.BeginScope("{Method} {Uri}", nameof(GetChannelItemMediaInfo), id))
            {
                _logger.LogDebug("{GetChannelItemId}", id);

                try
                {
                    var uri = new Uri(id);
                    var items = await _mediaSourceInfoProvider
                                        .GetManyAsync(uri, cancellationToken)
                                        .ToListAsync(cancellationToken)
                                        .ConfigureAwait(false);

                    _logger.LogDebug("{Method} results {Count}", nameof(GetChannelItemMediaInfo), items?.Count);

                    if (items is null)
                    {
                        return Enumerable.Empty<MediaSourceInfo>();
                    }

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
            return $"{Name}-{userId}-{_plugin.Configuration}-{DataVersion}";
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChannelItemInfo>> GetLatestMedia(ChannelLatestMediaSearch request, CancellationToken cancellationToken)
        {
            using (_logger.BeginScope("{Method} {UserId}", nameof(GetLatestMedia), request?.UserId))
            {
                var uri = _tuneInUriProvider.FavoritesUri ?? _tuneInUriProvider.PopularUri;

                _logger.LogDebug("ChannelLatestMediaSearch {Uri}", uri);

                var query = new InternalChannelItemQuery
                {
                    FolderId = uri.ToString()
                };

                var results = await GetChannelItems(query, cancellationToken).ConfigureAwait(false);

                _logger.LogDebug("{Method} results {Count}", nameof(GetLatestMedia), results?.Items.Count);
                if (results is null)
                {
                    return Enumerable.Empty<ChannelItemInfo>();
                }

                return results.Items;
            }
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ChannelItemInfo>> Search(ChannelSearchInfo searchInfo, CancellationToken cancellationToken)
        {
            using (_logger.BeginScope("{Method} {UserId} {SearchTerm}", nameof(Search), searchInfo?.UserId, searchInfo?.SearchTerm))
            {
                if (searchInfo == null)
                {
                    return Enumerable.Empty<ChannelItemInfo>();
                }

                _logger.LogDebug("{SearchTerm}", searchInfo.SearchTerm);

                var query = new InternalChannelItemQuery
                {
                    FolderId = _tuneInUriProvider.GetSearchUri(searchInfo.SearchTerm).ToString()
                };

                var results = await GetChannelItems(query, cancellationToken).ConfigureAwait(false);

                _logger.LogDebug("{Method} results {Count}", nameof(Search), results?.Items.Count);
                if (results is null)
                {
                    return Enumerable.Empty<ChannelItemInfo>();
                }

                return results.Items;
            }
        }
    }
}
