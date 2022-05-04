using FluentAssertions;
using Jellyfin.Plugin.TuneIn.Channels;
using Jellyfin.Plugin.TuneIn.Tests.Extensions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Controller.Channels;
using MediaBrowser.Model.Channels;
using MediaBrowser.Model.Drawing;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.MediaInfo;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Jellyfin.Plugin.TuneIn.Tests
{
    public class TuneInChannelTests
    {
        private readonly ServiceProvider _serviceProvider;

        public TuneInChannelTests(ITestOutputHelper output)
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
                    service.GetApiUrlForLocalAccess().ReturnsForAnyArgs("http://127.0.0.1:8096");
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
        public async Task Should_Receive_Root_Categories()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();

            var mockHttpMessageHander = scope.ServiceProvider.GetRequiredService<MockHttpMessageHandler>();

            var url = "http://opml.radiotime.com/Browse.ashx?formats=mp3,aac,ogg,flash,hls&partnerid=TestPartnerId&username=TestUsername";
            var embededContent = $"{typeof(TuneInChannelTests).Namespace}.TestFiles.Root.xml";

            var cancellationToken = CancellationToken.None;


            mockHttpMessageHander
                .When(HttpMethod.Get, url)
                .Respond(() =>
                    Task.FromResult(
                           new HttpResponseMessage(HttpStatusCode.OK)
                           {
                               Content = new StreamContent(typeof(TuneInChannelTests).Assembly.GetManifestResourceStream(embededContent)!)
                           }));


            var target = scope.ServiceProvider.GetRequiredService<TuneInChannel>();
            var query = new InternalChannelItemQuery
            {
                FolderId = url
            };

            // Act
            var result = await target.GetChannelItems(query, cancellationToken)
                                    .ConfigureAwait(false);

            // Assert
            result.Items.Should().AllBeEquivalentTo(new
            {
                ContentType = default(ChannelMediaContentType),
                MediaType = ChannelMediaType.Audio,
                OriginalTitle = default(string),
            });

            result.Items.Should().BeEquivalentTo(new[]
            {
                new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Browse.ashx?c=presets&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Type = ChannelItemType.Folder,
                        Name = "Favorites",
                        IsLiveStream = false,
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/tunein-myfavs.png"
                },
                new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Browse.ashx?c=local&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Type = ChannelItemType.Folder,
                        Name = "Local Radio",
                        IsLiveStream = false,
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/tunein-localradio.png"
                },
                new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Browse.ashx?c=music&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Type = ChannelItemType.Folder,
                        Name = "Music",
                        IsLiveStream = false,
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/tunein-music.png"
                },
                new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Browse.ashx?c=talk&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Type = ChannelItemType.Folder,
                        Name = "Talk",
                        IsLiveStream = false,
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/tunein-talk.png"
                },
                new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Browse.ashx?c=sports&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Type = ChannelItemType.Folder,
                        Name = "Sports",
                        IsLiveStream = false,
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/tunein-sports.png"
                },
                new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r0&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Type = ChannelItemType.Folder,
                        Name = "By Location",
                        IsLiveStream = false,
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/tunein-bylocation.png"
                },
                new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Browse.ashx?c=lang&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Type = ChannelItemType.Folder,
                        Name = "By Language",
                        IsLiveStream = false,
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/tunein-bylanguage.png"
                },
                new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Browse.ashx?c=podcast&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Type = ChannelItemType.Folder,
                        Name = "Podcasts",
                        IsLiveStream = false,
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/tunein-podcasts.png"
                },
            });
        }

        private void MockRequests_Favorites(MockHttpMessageHandler mockHttpMessageHander)
        {
            mockHttpMessageHander
                .WhenGet("http://opml.radiotime.com/Browse.ashx?c=presets&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername")
                .RespondOk(typeof(TuneInChannelTests).Assembly.GetManifestResourceStream($"{typeof(TuneInChannelTests).Namespace}.TestFiles.Favorites.xml")!);

            mockHttpMessageHander
                .WhenGet("http://opml.radiotime.com/Tune.ashx?id=s87949&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername")
                .RespondOk("https://live.rockfm.ro/rockfm.aacp\nhttp://live.kissfm.ro:9128/rockfm.aacp");

            mockHttpMessageHander
                .WhenGet("http://opml.radiotime.com/Tune.ashx?id=s2770&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername")
                .RespondOk("http://edge126.rdsnet.ro:84/profm/profm.mp3");

            mockHttpMessageHander
                .WhenHead("https://live.rockfm.ro/rockfm.aacp")
                .RespondOk(headers: new()
                {
                    { "Content-Type", "audio/aac" },
                    { "icy-br", "80" },
                    { "ice-audio-info", "ice-samplerate=44100;ice-bitrate=128;ice-channels=2" },
                    { "icy-description", "" },
                    { "icy-genre", "" },
                    { "icy-name", "" },
                    { "icy-pub", "0" },
                    { "icy-samplerate", "44100" },
                    { "icy-url", "https://www.rockfm.ro" }
                });
            mockHttpMessageHander
                .WhenHead("http://live.kissfm.ro:9128/rockfm.aacp")
                .RespondOk(headers: new()
                {
                    { "Content-Type", "audio/aac" },
                    { "icy-br", "80" },
                    { "ice-audio-info", "ice-samplerate=44100;ice-bitrate=128;ice-channels=2" },
                    { "icy-description", "" },
                    { "icy-genre", "" },
                    { "icy-name", "" },
                    { "icy-pub", "0" },
                    { "icy-samplerate", "44100" },
                    { "icy-url", "https://www.rockfm.ro" }
                });

            mockHttpMessageHander
                .WhenHead("http://edge126.rdsnet.ro:84/profm/profm.mp3")
                .Respond(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Should_Receive_Favorites_Categories()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();

            var mockHttpMessageHander = scope.ServiceProvider.GetRequiredService<MockHttpMessageHandler>();

            var url = "http://opml.radiotime.com/Browse.ashx?c=presets&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername";

            var cancellationToken = CancellationToken.None;

            MockRequests_Favorites(mockHttpMessageHander);


            var target = ActivatorUtilities.CreateInstance<TuneInChannel>(scope.ServiceProvider);
            var query = new InternalChannelItemQuery
            {
                FolderId = url
            };

            // Act
            var result = await target.GetChannelItems(query, cancellationToken)
                                    .ConfigureAwait(false);

            // Assert
            result.Items.Should().BeEquivalentTo(new[]
            {
                    new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Tune.ashx?id=s87949&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "100.6 | Rock FM Romania (Alternative Rock)",
                        ImageUrl = "http://cdn-radiotime-logos.tunein.com/s87949q.png",
                        OriginalTitle = "Rockaholic cu Ciprian Muntele",
                        OfficialRating = "97",
                        CommunityRating = 97,
                        ContentType = ChannelMediaContentType.Song
                    },
                    new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Tune.ashx?id=s2770&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "102.8 | PROFM (Top 40 & Pop Music)",
                        ImageUrl = "http://cdn-profiles.tunein.com/s2770/images/logoq.jpg?t=162363",
                        OriginalTitle = "PROFM OPEN RADIO",
                        OfficialRating = "99",
                        CommunityRating = 99,
                        ContentType = ChannelMediaContentType.Song
                    }
            });
        }

        [Fact]
        public async Task Should_Receive_Romanian_Categories()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();

            var mockHttpMessageHander = scope.ServiceProvider.GetRequiredService<MockHttpMessageHandler>();

            var url = "http://opml.radiotime.com/Browse.ashx?id=r101287&formats=mp3,aac,ogg,flash,hls&partnerid=TestPartnerId&username=TestUsername";
            var embededContent = $"{typeof(TuneInChannelTests).Namespace}.TestFiles.Romania.xml";

            var cancellationToken = CancellationToken.None;


            mockHttpMessageHander
                .When(HttpMethod.Get, url)
                .Respond(() =>
                    Task.FromResult(
                       new HttpResponseMessage(HttpStatusCode.OK)
                       {
                           Content = new StreamContent(typeof(TuneInChannelTests).Assembly.GetManifestResourceStream(embededContent)!)
                       }));


            var target = ActivatorUtilities.CreateInstance<TuneInChannel>(scope.ServiceProvider);
            var query = new InternalChannelItemQuery
            {
                FolderId = url
            };

            // Act
            var result = await target.GetChannelItems(query, cancellationToken)
                                    .ConfigureAwait(false);

            // Assert
            result.Items.Should().AllBeEquivalentTo(new
            {
                Type = ChannelItemType.Folder,
                ContentType = default(ChannelMediaContentType),
                MediaType = ChannelMediaType.Audio,
                OriginalTitle = default(string),
            });

            result.Items.Should().BeEquivalentTo(new[]
            {
                new {
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r101287&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername&filter=s:popular",
                        Name = "Most Popular",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Most+Popular-w480-h480-fs36.png",
                },
                new {
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r101287&pivot=genre&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername&filter=country",
                        Name = "By Genre",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/By+Genre-w480-h480-fs36.png",
                },
                new {
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r101287&pivot=name&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername&filter=country",
                        Name = "Find by Name",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Find+by+Name-w480-h480-fs36.png",
                },
                new {
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r101052&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "Bucharest-Ilfov",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Bucharest-Ilfov-w480-h480-fs36.png",
                },
                new {
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r102182&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "Centru",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Centru-w480-h480-fs36.png",
                },
                new {
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r102179&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "Nord-Est",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Nord-Est-w480-h480-fs36.png",
                },
                new {
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r102181&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "Nord-Vest",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Nord-Vest-w480-h480-fs36.png",
                },
                new {
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r102184&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "Sud",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Sud-w480-h480-fs36.png",
                },
                new {
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r102183&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "Sud-Est",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Sud-Est-w480-h480-fs36.png",
                },
                new {
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r102185&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "Sud-Vest",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Sud-Vest-w480-h480-fs36.png",
                },
                new {
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r102180&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "Vest",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Vest-w480-h480-fs36.png",
                },
            });
        }

        private void MockRequests_Bucharest(MockHttpMessageHandler mockHttpMessageHander)
        {
            mockHttpMessageHander
                .WhenGet("http://opml.radiotime.com/Browse.ashx?id=r101052&formats=mp3,aac,ogg,flash,hls&partnerid=TestPartnerId&username=TestUsername")
                .RespondOk(typeof(TuneInChannelTests).Assembly.GetManifestResourceStream($"{typeof(TuneInChannelTests).Namespace}.TestFiles.Bucharest.xml")!);

            mockHttpMessageHander
                .WhenGet("http://opml.radiotime.com/Tune.ashx?id=s87949&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername")
                .RespondOk("https://live.rockfm.ro/rockfm.aacp\nhttp://live.kissfm.ro:9128/rockfm.aacp");

            mockHttpMessageHander
                .WhenGet("http://opml.radiotime.com/Tune.ashx?id=s2770&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername")
                .RespondOk("http://edge126.rdsnet.ro:84/profm/profm.mp3");

            mockHttpMessageHander
                .WhenGet("http://opml.radiotime.com/Tune.ashx?id=s257792&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername")
                .RespondOk("http://edge76.rdsnet.ro:84/digifm/digifm.mp3");

            mockHttpMessageHander
                .WhenGet("http://opml.radiotime.com/Tune.ashx?id=s8334&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername")
                .RespondOk("http://astreaming.edi.ro:8000/EuropaFM_aac\nhttp://astreaming.europafm.ro:8000/europafm_mp3_64k\nhttp://astreaming.europafm.ro:8000/europafm_aacp48k");

            mockHttpMessageHander
                    .WhenGet("http://opml.radiotime.com/Tune.ashx?id=s17700&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername")
                    .RespondOk("https://live.kissfm.ro/kissfm.aacp");

            mockHttpMessageHander
                .WhenHead("https://live.rockfm.ro/rockfm.aacp")
                .RespondOk(headers: new()
                {
                    { "Content-Type", "audio/aac" },
                    { "icy-br", "80" },
                    { "ice-audio-info", "ice-samplerate=44100;ice-bitrate=128;ice-channels=2" },
                    { "icy-description", "" },
                    { "icy-genre", "" },
                    { "icy-name", "" },
                    { "icy-pub", "0" },
                    { "icy-samplerate", "44100" },
                    { "icy-url", "https://www.rockfm.ro" }
                });

            mockHttpMessageHander
                .WhenHead("https://live.kissfm.ro/kissfm.aacp")
                .RespondOk(headers: new()
                {
                    { "Content-Type", "audio/aac" },
                    { "icy-br", "80" },
                    { "ice-audio-info", "ice-samplerate=44100;ice-bitrate=128;ice-channels=2" },
                    { "icy-description", "" },
                    { "icy-genre", "" },
                    { "icy-name", "" },
                    { "icy-pub", "0" },
                    { "icy-samplerate", "44100" },
                    { "icy-url", "https://www.kissfm.ro" }
                });

            mockHttpMessageHander
                .WhenHead("http://astreaming.edi.ro:8000/EuropaFM_aac")
                .Respond(HttpStatusCode.BadRequest);

            mockHttpMessageHander
                .WhenHead("http://astreaming.europafm.ro:8000/europafm_mp3_64k")
                .Respond(HttpStatusCode.BadRequest);

            mockHttpMessageHander
                .WhenHead("http://astreaming.europafm.ro:8000/europafm_aacp48k")
                .Respond(HttpStatusCode.BadRequest);


            mockHttpMessageHander
                .WhenHead("http://edge76.rdsnet.ro:84/digifm/digifm.mp3")
                .Respond(HttpStatusCode.BadRequest);
        }
        [Fact]
        public async Task Should_Receive_Bucharest_Stations()
        {
            // Arrange
            using var scope = _serviceProvider.CreateScope();

            var mockHttpMessageHander = scope.ServiceProvider.GetRequiredService<MockHttpMessageHandler>();

            var url = "http://opml.radiotime.com/Browse.ashx?id=r101052&formats=mp3,aac,ogg,flash,hls&partnerid=TestPartnerId&username=TestUsername";

            var cancellationToken = CancellationToken.None;

            MockRequests_Bucharest(mockHttpMessageHander);


            var target = ActivatorUtilities.CreateInstance<TuneInChannel>(scope.ServiceProvider);
            var query = new InternalChannelItemQuery
            {
                FolderId = url
            };

            // Act
            var result = await target.GetChannelItems(query, cancellationToken)
                                    .ConfigureAwait(false);

            // Assert
            result.Items.Should().AllBeEquivalentTo(new
            {
                MediaType = ChannelMediaType.Audio,
            });

            result.Items.Should().BeEquivalentTo(new[]
            {
                    new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Tune.ashx?id=s257792&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "Digi FM 97.9 (World Music)",
                        ImageUrl = "http://cdn-profiles.tunein.com/s257792/images/logoq.jpg?t=636419",
                        OriginalTitle = "Ramon Cotizo",
                        Type = ChannelItemType.Media,
                        ContentType = ChannelMediaContentType.Song,
                        OfficialRating = "99",
                        CommunityRating = 99
                    },
                    new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Tune.ashx?id=s8334&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "Europa FM 106.7 (Adult Hits)",
                        ImageUrl = "http://cdn-profiles.tunein.com/s8334/images/logoq.png",
                        OriginalTitle = "Europa Express",
                        Type = ChannelItemType.Media,
                        ContentType = ChannelMediaContentType.Song,
                        OfficialRating = "94",
                        CommunityRating = 94
                    },
                    new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Tune.ashx?id=s17700&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername",
                        Name = "Kiss FM Romania 96.1 (Top 40 & Pop Music)",
                        ImageUrl = "http://cdn-profiles.tunein.com/s17700/images/logoq.png?t=1",
                        OriginalTitle = "Daily Kiss - Andreea Berghea",
                        Type = ChannelItemType.Media,
                        ContentType = ChannelMediaContentType.Song,
                        OfficialRating = "98",
                        CommunityRating = 98
                    },
                    new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r101052&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername&filter=g3",
                        Name = "Adult Hits",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Adult+Hits-w480-h480-fs36.png",
                        OriginalTitle = default(String),
                        Type = ChannelItemType.Folder,
                        ContentType = default(ChannelMediaContentType),
                    },
                    new ChannelItemInfo{
                        Id = "http://opml.radiotime.com/Browse.ashx?id=r101052&formats=mp3,aac,ogg,flash,hls&partnerId=TestPartnerId&username=TestUsername&filter=g115",
                        Name = "Alternative Rock",
                        ImageUrl = "http://127.0.0.1:8096/api/v1/TuneIn/Image/generate/Alternative+Rock-w480-h480-fs36.png",
                        OriginalTitle = default(String),
                        Type = ChannelItemType.Folder,
                        ContentType = default(ChannelMediaContentType),
                    },
            });
        }

        [Fact]
        public void Should_Return_Supported_Immages()
        {
            using(var scope = _serviceProvider.CreateScope())
            {
                var target = scope.ServiceProvider.GetRequiredService<TuneInChannel>();

                var supportedChannelImages =  target.GetSupportedChannelImages();

                supportedChannelImages
                    .Should()
                    .BeEquivalentTo(new[]
                {
                    ImageType.Backdrop,
                    ImageType.Primary,
                    ImageType.Thumb
                });
            }
        }


        [Fact]
        public async Task Should_Return_Supported_Images()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var target = scope.ServiceProvider.GetRequiredService<TuneInChannel>();

                var supportedChannelImages = target.GetSupportedChannelImages();

                foreach(var imageType in supportedChannelImages)
                {
                    var image = await target.GetChannelImage(imageType, CancellationToken.None);

                    image.Should().NotBeNull("Null image {0}", imageType);

                    image.Should().BeEquivalentTo(new
                    {
                        Format = ImageFormat.Png,
                        HasImage = true,
                    });

                    image.Stream.Should().NotBeNull("Null Stream for {0}", imageType);
                }
            }
        }
    }
}
