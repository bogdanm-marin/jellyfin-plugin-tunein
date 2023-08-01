using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Jellyfin.Plugin.TuneIn.Providers.Genres;
using Jellyfin.Plugin.TuneIn.Tests.Extensions;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Controller;
using MediaBrowser.Model.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.Core;
using RichardSzalay.MockHttp;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Jellyfin.Plugin.TuneIn.Tests
{
    public class GenresProvider_Tests
    {
        private readonly ServiceProvider _serviceProvider;

        public GenresProvider_Tests(ITestOutputHelper output)
        {
            var serviceRegistrator = new PluginServiceRegistrator();

            var services = new ServiceCollection();
            serviceRegistrator.RegisterServices(services);

            services
                .AddLogging(b => b.AddXUnit(output))
                .AddTransient<IApplicationPaths>(_ => Substitute.For<IApplicationPaths>())
                .AddTransient<IXmlSerializer>(_ => Substitute.For<IXmlSerializer>())
                .AddScoped<MockHttpMessageHandler>()
                .AddScoped<IServerApplicationHost>(s =>
                {
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
        public async Task Should_Retrieve_Genres_FromCache()
        {
            var embededReource = $"{typeof(GenresProvider_Tests).Namespace}.TestFiles.Genres.xml";
            int counters = 0;
            var cancellationToken = CancellationToken.None;

            // Arrange
            using var scope = _serviceProvider.CreateScope();

            var mockHttpMessageHander = scope.ServiceProvider.GetRequiredService<MockHttpMessageHandler>();
            var genreProvider = scope.ServiceProvider.GetRequiredService<GenresProvider>();

            mockHttpMessageHander
                .WhenGet("http://opml.radiotime.com/Describe.ashx?c=genres")
                .Respond(async () =>
                {
                    counters++;
                    return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StreamContent(typeof(GenresProvider_Tests).Assembly.GetManifestResourceStream(embededReource)!) };
                });
            ;

            var genre1 = await genreProvider.GetGenre("g76", cancellationToken);
            genre1
                .Should()
                .BeEquivalentTo(new Genre
                {
                    Id = "g76",
                    Name = "Children's Music"

                });
            var genre2 = await genreProvider.GetGenre("g3", cancellationToken);
            genre2
                .Should()
                .BeEquivalentTo(new Genre
                {
                    Id = "g3",
                    Name = "Adult Hits"

                });

            var genre3 = await genreProvider.GetGenre("g19", cancellationToken);
            genre3
                .Should()
                .BeEquivalentTo(new Genre
                {
                    Id = "g19",
                    Name = "Rock"

                });

            counters.Should().Be(1);

        }
    }
}
