using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Plugin.TuneIn.Extensions;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.MediaTypeHandlers
{
    /// <summary>
    /// Handles http requests with MediaTypes: audio/x-aac, audio/aacp, audio/aac, audio/mpeg, audio/ogg and etc.
    /// </summary>
    public class KnownMediaTypeHandler : IHttpResponseMessageHandler
    {
        private Dictionary<string, (string Container, string Codec)> SupportedMediaTypes { get; } = new()
        {
            { "audio/x-aac", ("aac", "aac") },
            { "audio/aacp", ("aac", "aac") },
            { "audio/aac", ("aac", "aac") },
            { "audio/mpeg", ("mp3", "mp3") },
            { "audio/ogg", ("ogg", "ogg") },
        };

        /// <inheritdoc/>
        public async IAsyncEnumerable<MediaSourceInfo> HandleAsync(
            HttpResponseMessage httpResponseMessage,
            [EnumeratorCancellation]
            CancellationToken cancellationToken)
        {
            if (!SupportedMediaTypes.TryGetValue(httpResponseMessage.Content.Headers.ContentType!.MediaType!.ToLowerInvariant(), out var mediaType))
            {
                yield break;
            }

            var requestedUri = httpResponseMessage.RequestMessage!.RequestUri?.ToString();

            var mediaStream = new MediaStream
            {
                Index = -1,
                Type = MediaStreamType.Audio,
                Codec = mediaType.Codec,
            };

            var mediaSourceInfo = new MediaSourceInfo
            {
                Id = requestedUri,
                Name = requestedUri,
                Path = requestedUri,
                Container = mediaType.Container,
                Protocol = MediaProtocol.Http,
                IsRemote = true,
                SupportsProbing = true,
                SupportsTranscoding = true,
                IsInfiniteStream = true,
                SupportsDirectPlay = true,
                SupportsDirectStream = true,
            };
            mediaSourceInfo.MediaStreams = new[] { mediaStream };

            var headers = httpResponseMessage.Headers.Union(httpResponseMessage.Content!.Headers).ToList();

            var headersDictionary = headers.ToDictionary(t => t.Key, t => t.Value);

            var audioProperties = headers
                                    .Where(s => s.Key.Equals("ice-audio-info", StringComparison.OrdinalIgnoreCase))
                                    .SelectMany(s => s.Value)
                                    .SelectMany(s => s.Split(';'))
                                    .Select(s => s.Trim())
                                    .Select(s => s.Split('='))
                                    .Where(s => s.Length == 2)
                                    .ToDictionary(s => s[0], s => s[1]);

            if (audioProperties.TryGetValue("ice-bitrate", out var iceBitRate) && int.TryParse(iceBitRate, out var bitRate))
            {
                mediaStream.BitRate = bitRate;
            }

            if (audioProperties.TryGetValue("ice-channels", out var iceChannels) && int.TryParse(iceChannels, out var channels))
            {
                mediaStream.Channels = channels;
            }

            if (audioProperties.TryGetValue("ice-samplerate", out var iceSampleRate) && int.TryParse(iceSampleRate, out var sampleRate))
            {
                mediaStream.SampleRate = sampleRate;
            }

            if (headersDictionary.TryGetValue("icy-name", out var names))
            {
                names
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ForEach(s => mediaSourceInfo.Name = s);
            }

            if (headersDictionary.TryGetValue("icy-description", out var descriptions))
            {
                descriptions
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ForEach(s => mediaSourceInfo.Name = s);
            }

            if (headersDictionary.TryGetValue("icy-sr", out var sampleRates))
            {
                sampleRates
                    .ToInt32()
                    .ForEach(v => mediaStream.SampleRate = v);
            }

            if (headersDictionary.TryGetValue("icy-br", out var brs))
            {
                brs
                    .ToInt32()
                    .ForEach(v => mediaStream.BitRate = v);
            }

            mediaSourceInfo.InferTotalBitrate();

            yield return await ValueTask.FromResult(mediaSourceInfo).ConfigureAwait(false);
        }
    }
}
