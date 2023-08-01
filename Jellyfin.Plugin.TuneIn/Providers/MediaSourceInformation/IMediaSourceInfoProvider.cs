using System;
using System.Collections.Generic;
using System.Threading;
using MediaBrowser.Model.Dto;

namespace Jellyfin.Plugin.TuneIn.Providers.MediaSourceInformation
{
    /// <summary>
    /// Media Source Info Provider interface.
    /// </summary>
    public interface IMediaSourceInfoProvider
    {
        /// <summary>
        /// Gets the Order of the provider.
        /// </summary>
        int Order { get; }

        /// <summary>
        /// Returns Media Source Information for given URI.
        /// </summary>
        /// <param name="uri">URI.</param>
        /// <param name="cancellationToken">EnumeratorCancellation token.</param>
        /// <returns>MediaSourceInfo collection.</returns>
        IAsyncEnumerable<MediaSourceInfo> GetManyAsync(Uri uri, CancellationToken cancellationToken);
    }
}
