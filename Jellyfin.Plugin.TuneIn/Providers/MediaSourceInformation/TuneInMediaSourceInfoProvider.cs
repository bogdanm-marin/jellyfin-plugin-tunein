using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Threading;
using Jellyfin.Plugin.TuneIn.Providers.Handlers.TuneInMediaInfo;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TuneIn.Providers.MediaSourceInformation
{
    /// <summary>
    /// MediaSourceInfoTuneInProvider.
    /// </summary>
    public class TuneInMediaSourceInfoProvider : IMediaSourceInfoProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TuneInMediaSourceInfoProvider> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="TuneInMediaSourceInfoProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">HttpClientFactory.</param>
        /// <param name="logger">Logger.</param>
        public TuneInMediaSourceInfoProvider(
            IHttpClientFactory httpClientFactory,
            ILogger<TuneInMediaSourceInfoProvider> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        /// <summary>
        /// Gets the Order of the provider.
        /// </summary>
        public int Order => 0;

        /// <summary>
        /// Processes uri and returns an enumeration of <see cref="MediaSourceInfo"/>.
        /// </summary>
        /// <param name="uri">Media uri.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>Async Enumeration of <see cref="MediaSourceInfo"/>.</returns>
        public async IAsyncEnumerable<MediaSourceInfo> GetManyAsync(Uri uri, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            using (_logger.BeginScope("{Method} {Uri}", nameof(GetManyAsync), uri))
            {
                using var httpClient = _httpClientFactory.CreateClient("TuneIn");
                HttpResponseMessage? response = null;

                var hasException = false;

                try
                {
                    var uriBuilder = new UriBuilder(uri);
                    uriBuilder.Query += "&render=json";
                    var url = uriBuilder.Uri;

                    response = await httpClient
                                            .GetAsync(url, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                                            .ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "{Message}", ex);
                    hasException = true;
                }

                if (hasException)
                {
                    yield break;
                }

                using (response!)
                {
                    if (!response!.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("{StatusCode}", response.StatusCode);
                        yield break;
                    }

                    var mediaRoot = await response.Content
                        .ReadFromJsonAsync<TuneInMediaRoot>(cancellationToken: cancellationToken)
                        .ConfigureAwait(false);

                    if (mediaRoot is null || mediaRoot.Body is null)
                    {
                        yield break;
                    }

                    var requestedUri = uri.ToString();

                    foreach (var track in mediaRoot.Body.Where(mr => string.Equals(mr.Element, "audio", StringComparison.OrdinalIgnoreCase)))
                    {
                        yield return new MediaSourceInfo
                        {
                            Id = requestedUri,
                            Name = requestedUri,
                            Path = track.Url,
                            Container = track.MediaType,
                            Protocol = MediaProtocol.Http,
                            IsRemote = true,
                            IsInfiniteStream = true,
                            SupportsDirectPlay = true,
                            SupportsDirectStream = true,
                            MediaStreams = new[]
                            {
                                new MediaStream
                                {
                                    Index = -1,
                                    Type = MediaStreamType.Audio,
                                    Codec = track.MediaType,
                                    BitRate = track.Bitrate
                                }
                            }
                        };
                    }
                }
            }
        }
    }
}
