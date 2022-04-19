using System;
using System.Collections.Generic;
using System.Threading;
using MediaBrowser.Model.Dto;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.UriHandlers
{
    /// <summary>
    /// Interface definition for URI handlers.
    /// </summary>
    public interface IUriHandler
    {
        /// <summary>
        /// Gets the order in which <see cref="IUriHandler"/> handlers should be processed.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Handles URI requests.
        /// </summary>
        /// <param name="uri"><see cref="Uri"/> instance to process.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken" /> cancellation token.</param>
        /// <returns>Returns a collection of <see cref="MediaSourceInfo"/>.</returns>
        IAsyncEnumerable<MediaSourceInfo> HandleAsync(Uri uri, CancellationToken cancellationToken);
    }
}
