using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Jellyfin.Plugin.TuneIn.Channels;
using Jellyfin.Plugin.TuneIn.Tests.Extensions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;

namespace Jellyfin.Plugin.TuneIn.Tests
{
    public class TuneInChannel_M3u8_Tests
    {
        private readonly ServiceProvider _serviceProvider;

        public TuneInChannel_M3u8_Tests(ITestOutputHelper output)
        {
            var serviceRegistrator = new PluginServiceRegistrator();

            var services = new ServiceCollection();
            serviceRegistrator.RegisterServices(services);

            services
                .AddLogging(b => b.AddXUnit(output))
                .AddTransient<IApplicationPaths>(_ => Substitute.For<IApplicationPaths>())
                .AddTransient<IXmlSerializer>(_ => Substitute.For<IXmlSerializer>())
                .AddSingleton<MockHttpMessageHandler>()
                .AddSingleton<IServerApplicationHost>(s => {
                    var service = Substitute.For<IServerApplicationHost>();
                    service.GetApiUrlForLocalAccess().ReturnsForAnyArgs("http://127.0.0.1:8096");
                    return service;
                    })
                .AddTransient<HttpClient>(_ => new HttpClient(_.GetRequiredService<MockHttpMessageHandler>()))
                .AddScoped<IHttpClientFactory>(s =>
                {
                    var httpClientFactory = Substitute.For<IHttpClientFactory>();
                    httpClientFactory
                        .CreateClient()
                        .ReturnsForAnyArgs(_ => s.GetRequiredService<HttpClient>());

                    return httpClientFactory;
                });



            _serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task Should_Process_M3u8_With_Valid_M3u9_MediaType_Audio_X_Mpegurl()
        {
            using var scope = _serviceProvider.CreateScope();

            var httpHandler = scope.ServiceProvider.GetRequiredService<MockHttpMessageHandler>();
            var url = "http://opml.radiotime.com/Tune.ashx?id=s15273&formats=hls&partnerId=TestPartnerId&username=TestUsername";

            httpHandler
                .WhenGet(url)
                .RespondOk(
                    content: "http://cdnlive.shooowit.net/rtvalive/smil:channel5.smil/playlist.m3u8",
                    contentType: "audio/x-mpegurl"
                );

            httpHandler
                .WhenGet("http://cdnlive.shooowit.net/rtvalive/smil:channel5.smil/playlist.m3u8")
                .RespondOk(
                    content: @"
#EXTM3U
#EXT-X-VERSION:3
#EXT-X-STREAM-INF:BANDWIDTH=250000
chunklist_b250000.m3u8
".Trim(),
                    contentType: "application/vnd.apple.mpegurl"
                );

            httpHandler
                .WhenGet("http://cdnlive.shooowit.net/rtvalive/smil:channel5.smil/chunklist_b250000.m3u8")
                .RespondOk(
                    content: @"
#EXTM3U
#EXT-X-VERSION:3
#EXT-X-TARGETDURATION:11
#EXT-X-MEDIA-SEQUENCE:311471
#EXT-X-DISCONTINUITY-SEQUENCE:0
#EXTINF:9.984,
media-ujg8jo7xj_b250000_311471.aac
#EXTINF:10.112,
media-ujg8jo7xj_b250000_311472.aac
#EXTINF:9.984,
media-ujg8jo7xj_b250000_311473.aac
".Trim(),
                    contentType: "application/vnd.apple.mpegurl"
                );

            httpHandler
                .WhenGet("http://cdnlive.shooowit.net/rtvalive/smil:channel5.smil/media-ujg8jo7xj_b250000_311471.aac")
                .RespondOk( contentType: "audio/x-aac" );

            httpHandler
                .WhenGet("http://cdnlive.shooowit.net/rtvalive/smil:channel5.smil/media-ujg8jo7xj_b250000_311472.aac")
                .RespondOk(contentType: "audio/x-aac");

            httpHandler
                .WhenGet("http://cdnlive.shooowit.net/rtvalive/smil:channel5.smil/media-ujg8jo7xj_b250000_311473.aac")
                .RespondOk(contentType: "audio/x-aac");

            var cancellationToken = CancellationToken.None;

            var target = scope.ServiceProvider.GetRequiredService<TuneInChannel>();

            // Act
            var result = await target.GetChannelItemMediaInfo(url, cancellationToken)
                                    .ConfigureAwait(false);

            // Assert

            result.Should().BeEquivalentTo(new[]
            {
                            new MediaSourceInfo
                            {
                                Id =  "http://cdnlive.shooowit.net/rtvalive/smil:channel5.smil/playlist.m3u8",
                                Name = "http://cdnlive.shooowit.net/rtvalive/smil:channel5.smil/playlist.m3u8",
                                Path = "http://cdnlive.shooowit.net/rtvalive/smil:channel5.smil/playlist.m3u8",
                                Container = "aac",
                                Protocol = MediaProtocol.Http,
                                IsRemote = true,
                                IsInfiniteStream = true,
                                SupportsDirectPlay = true,
                                SupportsDirectStream = true,
                                RunTimeTicks = 0,
                                TranscodingSubProtocol = "hls",
                                MediaStreams = new []
                                {
                                    new MediaStream
                                    {
                                        Index = -1,
                                        Codec = "aac",
                                    },
                                }
                            },
            });
        }

        [Fact]
        public async Task Should_Process_M3u8_With_Invalid_M3u9_MediaType_Text_Html()
        {
            using var scope = _serviceProvider.CreateScope();

            var httpHandler = scope.ServiceProvider.GetRequiredService<MockHttpMessageHandler>();
            var url = "http://opml.radiotime.com/Tune.ashx?id=s97051&formats=mp3,aac,ogg,hls&partnerId=TestPartnerId&username=TestUsername";

            httpHandler
                .WhenGet(url)
                .RespondOk(
                    content: "https://ivm.antenaplay.ro/liveaudio/radiozu/playlist.m3u8",
                    contentType: "audio/x-mpegurl"
                );

            httpHandler
                .WhenGet("https://ivm.antenaplay.ro/liveaudio/radiozu/playlist.m3u8")
                .RespondOk(
                    content: @"
#EXTM3U
#EXT-X-VERSION:3
#EXT-X-ALLOW-CACHE:NO
## Created with Z/IPStream R/2 v1.03.20
#EXT-X-STREAM-INF:BANDWIDTH=51914,CODECS=""mp4a.40.5""
https://live4ro.antenaplay.ro/radiozu/radiozu-48000.m3u8
".Trim(),
                    contentType: "text/html; charset=UTF-8"
                );
            var cancellationToken = CancellationToken.None;

            var target = scope.ServiceProvider.GetRequiredService<TuneInChannel>();

            // Act
            var result = await target.GetChannelItemMediaInfo(url, cancellationToken)
                                    .ConfigureAwait(false);

            // Assert

            result.Should().BeEquivalentTo(new[]
            {
                 new MediaSourceInfo
            {
                Id = "https://ivm.antenaplay.ro/liveaudio/radiozu/playlist.m3u8",
                Name = "https://ivm.antenaplay.ro/liveaudio/radiozu/playlist.m3u8",
                Path = "https://ivm.antenaplay.ro/liveaudio/radiozu/playlist.m3u8",
                Protocol = MediaProtocol.Http,
                Container = "aac",
                IsRemote = true,
                IsInfiniteStream = true,
                SupportsDirectPlay = true,
                SupportsDirectStream = true,
                TranscodingSubProtocol = "hls",
                RunTimeTicks = 0,
                MediaStreams = new []
                {
                            new MediaStream
                            {
                                Index = -1,
                                Type = MediaStreamType.Audio,
                                Codec = "aac"
                            }
                },
            }
        });
        }
    }
}
