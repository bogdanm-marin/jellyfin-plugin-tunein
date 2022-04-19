using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.MediaTypeHandlers
{
    /// <summary>
    /// Handler for Http requests with MediaType audio/x-mpegurl.
    /// </summary>
    public class MpegUrlMediaTypeHandler : IHttpResponseMessageHandler
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="MpegUrlMediaTypeHandler"/> class.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> instance.</param>
        public MpegUrlMediaTypeHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<MediaSourceInfo> HandleAsync(
            HttpResponseMessage httpResponseMessage,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var mediaSourceInfoProvider = _serviceProvider.GetRequiredService<MediaSourceInfoProvider>();
            var contentType = httpResponseMessage.Content.Headers.ContentType;

            if (contentType?.MediaType?.Equals("audio/x-mpegurl", StringComparison.OrdinalIgnoreCase) != true)
            {
                yield break;
            }

            var content = await httpResponseMessage.Content
                                .ReadAsStringAsync(cancellationToken)
                                .ConfigureAwait(false);

            var referencedUris = content
                                        .Split('\n')
                                        .Select(s => s.Trim())
                                        .Where(s => !string.IsNullOrEmpty(s))
                                        .Select(s => new Uri(s));

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
