using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TuneIn.Providers.Handlers.UriHandlers;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TuneIn.Providers.MediaSourceInformation
{
    /// <summary>
    /// MediaSourceInfoProvider.
    /// </summary>
    public class MediaSourceInfoProvider : IMediaSourceInfoProvider
    {
        private readonly ILogger<MediaSourceInfoProvider> _logger;
        private readonly IList<IUriHandler> _handlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSourceInfoProvider"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="handlers"><see cref="IUriHandler" /> handlers.</param>
        public MediaSourceInfoProvider(
            ILogger<MediaSourceInfoProvider> logger,
            IEnumerable<IUriHandler> handlers)
        {
            _logger = logger;
            _handlers = handlers.OrderBy(_ => _.Order).ToList();
        }

        /// <summary>
        /// Gets the Order of the provider.
        /// </summary>
        public int Order => 1;

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
                foreach (var handler in _handlers)
                {
                    var hasItems = false;
                    var source = handler.HandleAsync(uri, cancellationToken)
                                        .WithCancellation(cancellationToken)
                                        .ConfigureAwait(false);

                    await foreach (var item in source)
                    {
                        hasItems = true;
                        yield return item;
                    }

                    if (hasItems)
                    {
                        yield break;
                    }
                }
            }
        }
    }
}
