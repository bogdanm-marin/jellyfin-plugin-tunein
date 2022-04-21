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
    public class TuneInChannel_Pls_Tests
    {
        private readonly ServiceProvider _serviceProvider;

        public TuneInChannel_Pls_Tests(ITestOutputHelper output)
        {
            var serviceRegistrator = new PluginServiceRegistrator();

            var services = new ServiceCollection();
            serviceRegistrator.RegisterServices(services);

            services
                .AddLogging(b => b.AddXUnit(output))
                .AddTransient<IApplicationPaths>(_ => Substitute.For<IApplicationPaths>())
                .AddTransient<IXmlSerializer>(_ => Substitute.For<IXmlSerializer>())
                .AddSingleton<IServerApplicationHost>(s => {
                    var service = Substitute.For<IServerApplicationHost>();
                    service.GetLoopbackHttpApiUrl().ReturnsForAnyArgs("http://127.0.0.1:8096");
                    return service;
                })
                .AddSingleton<MockHttpMessageHandler>()
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
        public async Task Should_Process_Pls_With_Valid_MediaType_Audio_X_Scpls()
        {
            using var scope = _serviceProvider.CreateScope();

            var httpHandler = scope.ServiceProvider.GetRequiredService<MockHttpMessageHandler>();
            var url = "http://opml.radiotime.com/Tune.ashx?id=s15273&formats=hls&partnerId=TestPartnerId&username=TestUsername";

            httpHandler
                .WhenGet(url)
                .RespondOk(
                    content: "http://us4.internet-radio.com:8266/listen.pls",
                    contentType: "audio/x-mpegurl"
                );

            httpHandler
                .WhenGet("http://us4.internet-radio.com:8266/listen.pls")
                .RespondOk(
                    content: @"
[playlist]
NumberOfEntries=1
File1=http://us4.internet-radio.com:8266/stream
Title1=Smooth Jazz Florida WSJF-DB
Length1=-1
Version=2
".Trim(),
                    contentType: "audio/x-scpls"
                );

            httpHandler
                .WhenGet("http://us4.internet-radio.com:8266/stream")
                .RespondOk(
                    headers: new()
                    {
                        { "icy-notice1", "<BR>This stream requires <a href=\"http://www.winamp.com\">Winamp</a><BR>" },
                        { "icy-notice2", "Shoutcast DNAS/posix(linux x64) v2.6.0.753<BR>" },
                        { "icy-genre", "Smooth Jazz" },
                        { "icy-name", "Smooth Jazz Florida WSJF-DB" },
                        { "icy-br", "128" },
                        { "icy-sr", "44100" },
                        { "icy-url", "http://www.SmoothJazzFlorida.com" },
                        { "icy-pub", "1" },
                    },
                    contentType: "audio/mpeg"
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
                                Id =  "http://us4.internet-radio.com:8266/stream",
                                Name = "Smooth Jazz Florida WSJF-DB",
                                Path = "http://us4.internet-radio.com:8266/stream",
                                Container = "mp3",
                                Protocol = MediaProtocol.Http,
                                IsRemote = true,
                                SupportsDirectPlay = true,
                                SupportsDirectStream = true,
                                IsInfiniteStream = true,
                                Bitrate = 128,
                                MediaStreams = new()
                                {
                                    new MediaStream
                                    {
                                        Index = -1,
                                        Codec = "mp3",
                                        SampleRate = 44100,
                                        BitRate = 128
                                    },
                                }
                            },
            });
        }

        [Fact]
        public async Task Should_Process_Pls_With_Invalid_MediaType_Text_Html()
        {
            using var scope = _serviceProvider.CreateScope();

            var httpHandler = scope.ServiceProvider.GetRequiredService<MockHttpMessageHandler>();
            var url = "http://opml.radiotime.com/Tune.ashx?id=s97051&formats=mp3,aac,ogg,hls&partnerId=TestPartnerId&username=TestUsername";

            httpHandler
                .WhenGet(url)
                .RespondOk(
                    content: "http://us4.internet-radio.com:8266/listen.pls",
                    contentType: "audio/x-mpegurl"
                );

            httpHandler
                .WhenGet("http://us4.internet-radio.com:8266/listen.pls")
                .RespondOk(
                    content: @"
[playlist]
NumberOfEntries=1
File1=http://us4.internet-radio.com:8266/stream
Title1=Smooth Jazz Florida WSJF-DB
Length1=-1
Version=2
".Trim(),
                    contentType: "text/html"
                );

            httpHandler
                .WhenGet("http://us4.internet-radio.com:8266/stream")
                .RespondOk(
                    headers: new()
                    {
                        { "icy-notice1", "<BR>This stream requires <a href=\"http://www.winamp.com\">Winamp</a><BR>" },
                        { "icy-notice2", "Shoutcast DNAS/posix(linux x64) v2.6.0.753<BR>" },
                        { "icy-genre", "Smooth Jazz" },
                        { "icy-name", "Smooth Jazz Florida WSJF-DB" },
                        { "icy-br", "128" },
                        { "icy-sr", "44100" },
                        { "icy-url", "http://www.SmoothJazzFlorida.com" },
                        { "icy-pub", "1" },
                    },
                    contentType: "audio/mpeg"
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
                                Id =  "http://us4.internet-radio.com:8266/stream",
                                Name = "Smooth Jazz Florida WSJF-DB",
                                Path = "http://us4.internet-radio.com:8266/stream",
                                Container = "mp3",
                                Protocol = MediaProtocol.Http,
                                IsRemote = true,
                                SupportsDirectPlay = true,
                                SupportsDirectStream = true,
                                Bitrate = 128,
                                IsInfiniteStream = true,
                                MediaStreams = new()
                                {
                                    new MediaStream
                                    {
                                        Index = -1,
                                        Codec = "mp3",
                                        SampleRate = 44100,
                                        BitRate = 128
                                    },
                                }
                            },
            });
        }
    }
}
