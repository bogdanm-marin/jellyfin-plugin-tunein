using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TuneIn.Filters;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TuneIn.Providers.MediaSourceInformation
{
    /// <summary>
    /// MediaSourceInfoService.
    /// </summary>
    public class MediaSourceInfoProviderManager
    {
        private readonly ILogger<MediaSourceInfoProvider> _logger;
        private readonly IEnumerable<IMediaSourceInfoFilter> _filters;
        private readonly IList<IMediaSourceInfoProvider> _handlers;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSourceInfoProviderManager"/> class.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="handlers"><see cref="IMediaSourceInfoProvider" /> handlers.</param>
        /// <param name="filters"><see cref="IMediaSourceInfoFilter" /> filters.</param>
        public MediaSourceInfoProviderManager(
            ILogger<MediaSourceInfoProvider> logger,
            IEnumerable<IMediaSourceInfoProvider> handlers,
            IEnumerable<IMediaSourceInfoFilter> filters)
        {
            _logger = logger;
            _handlers = handlers.OrderBy(_ => _.Order).ToList();
            _filters = filters.ToList();
        }

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
                    var source = handler.GetManyAsync(uri, cancellationToken)
                                        .WithCancellation(cancellationToken)
                                        .ConfigureAwait(false);

                    await foreach (var item in source)
                    {
                        if (_filters.All(f => f.IsAllowed(item)))
                        {
                            hasItems = true;
                            yield return item;
                        }
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
