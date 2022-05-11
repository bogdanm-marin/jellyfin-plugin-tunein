using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TuneIn.Extensions;
using Jellyfin.Plugin.TuneIn.Providers.Handlers.MediaTypeHandlers;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.UriHandlers
{
    /// <summary>
    /// Handlers that fetches uri contents and processes the content.
    /// </summary>
    public class ProcessUriHandler : IUriHandler
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<ProcessUriHandler> _logger;
        private readonly ICollection<IHttpResponseMessageHandler> _handlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessUriHandler"/> class.
        /// </summary>
        /// <param name="httpClientFactory"><see cref="HttpClient" /> factory.</param>
        /// <param name="logger"><see cref="ILogger{ProcessUriHandler}" /> instance for <see cref="ProcessUriHandler"/>.</param>
        /// <param name="handlers">Collection of <see cref="IHttpResponseMessageHandler"/> to process URI content.</param>
        public ProcessUriHandler(
            IHttpClientFactory httpClientFactory,
            ILogger<ProcessUriHandler> logger,
            IEnumerable<IHttpResponseMessageHandler> handlers)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _handlers = handlers.ToList();
        }

        /// <inheritdoc/>
        public int Order => 99;

        /// <inheritdoc/>
        public async IAsyncEnumerable<MediaSourceInfo> HandleAsync(Uri uri, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (!uri.SchemeIsHttpOrHttps())
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

                    foreach (var handler in _handlers)
                    {
                        var source = handler.HandleAsync(response, cancellationToken)
                                            .WithCancellation(cancellationToken)
                                            .ConfigureAwait(false);

                        await foreach (var item in source)
                        {
                            yield return item;
                        }
                    }
                }
            }
        }
    }
}
