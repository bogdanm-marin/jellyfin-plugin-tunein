using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using MediaBrowser.Model.Dto;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.MediaTypeHandlers
{
    /// <summary>
    /// Interface definition for HttpResponseMessage handlers.
    /// </summary>
    public interface IHttpResponseMessageHandler
    {
        /// <summary>
        /// Handles <see cref="HttpResponseMessage"/> reqsponses.
        /// </summary>
        /// <param name="httpResponseMessage"><see cref="HttpResponseMessage"/> reqsponse to process.</param>
        /// <param name="cancellationToken"><see cref="CancellationToken"/> cancellation token.</param>
        /// <returns>Enumeration of <see cref="MediaSourceInfo"/>.</returns>
        IAsyncEnumerable<MediaSourceInfo> HandleAsync(HttpResponseMessage httpResponseMessage, CancellationToken cancellationToken);
    }
}
