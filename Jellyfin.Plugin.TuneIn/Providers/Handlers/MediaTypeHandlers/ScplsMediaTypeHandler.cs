using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TuneIn.Providers.MediaSourceInformation;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.MediaTypeHandlers
{
    /// <summary>
    /// Handler for MediaType audio/x-scpls.
    /// </summary>
    public class ScplsMediaTypeHandler : IHttpResponseMessageHandler
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScplsMediaTypeHandler"/> class.
        /// </summary>
        /// <param name="serviceProvider"><see cref="IServiceProvider"/> instance.</param>
        public ScplsMediaTypeHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc/>
        public async IAsyncEnumerable<MediaSourceInfo> HandleAsync(HttpResponseMessage httpResponseMessage, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var contentType = httpResponseMessage.Content.Headers.ContentType;

            if (contentType?.MediaType?.Equals("audio/x-scpls", StringComparison.OrdinalIgnoreCase) != true)
            {
                yield break;
            }

            var content = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

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
