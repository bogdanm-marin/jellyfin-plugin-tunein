using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.UriHandlers
{
    /// <summary>
    /// Handles URI with extensions: .aac, .aacp, .mp3, .ogg and etc.
    /// </summary>
    public class KnownExtensionsUriHandler : IUriHandler
    {
        /// <inheritdoc/>
        public int Order => 0;

        private Dictionary<string, (string Container, string Codec)> SupportedExtensions { get; } = new()
        {
            { ".aac", ("aac", "aac") },
            { ".aacp", ("aac", "aac") },
            { ".mp3", ("mp3", "mp3") },
            { ".ogg", ("ogg", "ogg") },
        };

        /// <inheritdoc/>
        public async IAsyncEnumerable<MediaSourceInfo> HandleAsync(
            Uri uri,
            [EnumeratorCancellation]
            CancellationToken cancellationToken)
        {
            var requestedUri = uri.ToString();

            var extensions = requestedUri.Substring(requestedUri.LastIndexOf('.'));

            if (!SupportedExtensions.TryGetValue(extensions, out var extensionType))
            {
                yield break;
            }

            var mediaStream = new MediaStream
            {
                Index = -1,
                Type = MediaStreamType.Audio,
                Codec = extensionType.Codec,
            };

            var mediaSourceInfo = new MediaSourceInfo
            {
                Id = requestedUri,
                Name = requestedUri,
                Path = requestedUri,
                Container = extensionType.Container,
                Protocol = MediaProtocol.Http,
                IsRemote = true,
                SupportsProbing = true,
                SupportsTranscoding = true,
                IsInfiniteStream = true,
                SupportsDirectPlay = true,
                SupportsDirectStream = true,
            };
            mediaSourceInfo.MediaStreams.Add(mediaStream);

            mediaSourceInfo.InferTotalBitrate();

            yield return await ValueTask.FromResult(mediaSourceInfo).ConfigureAwait(false);
        }
    }
}
