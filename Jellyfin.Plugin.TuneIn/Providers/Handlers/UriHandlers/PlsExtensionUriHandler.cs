using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TuneIn.Extensions;
using Jellyfin.Plugin.TuneIn.Providers.MediaSourceInformation;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.UriHandlers
{
    /// <summary>
    /// Handles URLs with ".pls" extension.
    /// </summary>
    public class PlsExtensionUriHandler : IUriHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<PlsExtensionUriHandler> _logger;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlsExtensionUriHandler"/> class.
        /// </summary>
        /// <param name="httpClientFactory"><see cref="HttpClient"/> Factory service.</param>
        /// <param name="logger"><see cref="ILogger"/> instance for <see cref="PlsExtensionUriHandler"/>.</param>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> instance.</param>
        public PlsExtensionUriHandler(
            IHttpClientFactory httpClientFactory,
            ILogger<PlsExtensionUriHandler> logger,
            IServiceProvider serviceProvider)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public int Order => 2;

        /// <inheritdoc/>
        public async IAsyncEnumerable<MediaSourceInfo> HandleAsync([NotNull] Uri uri, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (!uri.SchemeIsHttpOrHttps())
            {
                yield break;
            }

            if (!uri.AbsoluteUri.EndsWith(".pls", StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            using (_logger.BeginScope("Uri {Uri}", uri))
            {
                using var httpClient = _httpClientFactory.CreateClient("TuneIn");
                HttpResponseMessage? response = null;

                var hasException = false;

                try
                {
                    var responseTask = httpClient
                                            .GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                                            .ConfigureAwait(false);

                    response = await responseTask;
                }
                catch (HttpRequestException ex)
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

                    var content = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

                    var referencedUris = content
                                    .Split('\n')
                                    .Select(s => s.Trim())
                                    .Where(s => !string.IsNullOrEmpty(s))
                                    .Where(s => s.StartsWith("File", StringComparison.OrdinalIgnoreCase))
                                    .Select(s => s[(s.IndexOf('=', StringComparison.OrdinalIgnoreCase) + 1)..])
                                    .Select(s => new Uri(s));

                    var mediaSourceInfoProvider = _serviceProvider.GetRequiredService<MediaSourceInfoProvider>();
                    foreach (var childUri in referencedUris)
                    {
                        var sourceAsync = mediaSourceInfoProvider.GetManyAsync(childUri, cancellationToken)
                                            .WithCancellation(cancellationToken)
                                            .ConfigureAwait(false);

                        await foreach (var item in sourceAsync)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }
    }
}
