using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TuneIn.Extensions;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.UriHandlers
{
    /// <summary>
    /// Handles URI with extension .m3u8.
    /// </summary>
    public class M3U8ExtensionUriHandler : IUriHandler
    {
        /// <inheritdoc/>
        public int Order => 1;

        /// <inheritdoc/>
        public async IAsyncEnumerable<MediaSourceInfo> HandleAsync(
            Uri uri,
            [EnumeratorCancellation]
            CancellationToken cancellationToken)
        {
            if (!uri.SchemeIsHttpOrHttps())
            {
                yield break;
            }

            if (!uri.AbsoluteUri.EndsWith(".m3u8", StringComparison.OrdinalIgnoreCase))
            {
                yield break;
            }

            var requestedUri = uri.ToString();

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
                Container = "aac",
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
