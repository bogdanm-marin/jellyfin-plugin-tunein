using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.MediaTypeHandlers
{
    /// <summary>
    /// Handles Http requests with MediaType application/vnd.apple.mpegurl.
    /// </summary>
    public class AppleMpegUrlMediaTypeHandler : IHttpResponseMessageHandler
    {
        /// <inheritdoc/>
        public async IAsyncEnumerable<MediaSourceInfo> HandleAsync(
            HttpResponseMessage httpResponseMessage,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var contentType = httpResponseMessage.Content.Headers.ContentType;

            if (contentType?.MediaType?.Equals("application/vnd.apple.mpegurl", StringComparison.OrdinalIgnoreCase) != true)
            {
                yield break;
            }

            var requestedUri = httpResponseMessage.RequestMessage!.RequestUri?.ToString();

            var mediaStream = new MediaStream
            {
                Index = -1,
                Type = MediaStreamType.Audio,
                Codec = "aac"
            };

            var mediaSourceInfo = new MediaSourceInfo
            {
                Id = requestedUri,
                Name = requestedUri,
                Path = requestedUri,
                Protocol = MediaProtocol.Http,
                Container = "mpegts",
                IsRemote = true,
                SupportsProbing = true,
                SupportsTranscoding = true,
                IsInfiniteStream = true,
                SupportsDirectPlay = true,
                SupportsDirectStream = true,
                TranscodingSubProtocol = "hls",
                RunTimeTicks = 0,
            };
            mediaSourceInfo.MediaStreams = new[] { mediaStream };

            yield return await ValueTask.FromResult(mediaSourceInfo).ConfigureAwait(false);
        }
    }
}
