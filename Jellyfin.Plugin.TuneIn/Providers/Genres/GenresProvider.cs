using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.TuneIn.Providers.Genres
{
    /// <summary>
    /// Genres Provider.
    /// </summary>
    public class GenresProvider
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TuneInUriProvider _tuneInUriProvider;
        private readonly ILogger<GenresProvider> _logger;
        private readonly CacheProvider _cacheProvider;

        /// <summary>
        /// Initializes a new instance of the <see cref="GenresProvider"/> class.
        /// </summary>
        /// <param name="httpClientFactory">Http Client factory.</param>
        /// <param name="tuneInUriProvider">TuneIn Provider.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="cacheProvider">Cache Provider.</param>
        public GenresProvider(IHttpClientFactory httpClientFactory, TuneInUriProvider tuneInUriProvider, ILogger<GenresProvider> logger, CacheProvider cacheProvider)
        {
            _httpClientFactory = httpClientFactory;
            _tuneInUriProvider = tuneInUriProvider;
            _logger = logger;
            _cacheProvider = cacheProvider;
        }

        /// <summary>
        /// Gets the genre for a given id.
        /// </summary>
        /// <param name="id">Genre identificaton.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>Genre of the <see cref="Genre"/>.</returns>
        public async Task<Genre?> GetGenre(string id, CancellationToken cancellationToken)
        {
            var genres = await GetGenres(cancellationToken)
                                .ConfigureAwait(false);

            if (genres is not null && genres.TryGetValue(id, out var genre))
            {
                return genre;
            }

            return null;
        }

        /// <summary>
        /// Returns the list of genres supported by TuneIn.
        /// </summary>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>Returns a dictionary of Genres.</returns>
        public async Task<IDictionary<string, Genre>> GetGenres(CancellationToken cancellationToken)
        {
            var items = await _cacheProvider
                                    .GetCached("TuneInGenres", FetchGenres, cancellationToken)
                                    .ConfigureAwait(false);
            return items;
        }

        private async Task<IDictionary<string, Genre>> FetchGenres(CancellationToken cancellationToken)
        {
            var url = _tuneInUriProvider.GenresUri;

            using (_logger.BeginScope("{Method} {Url}", nameof(GetGenres), url))
            {
                using var httpClient = _httpClientFactory.CreateClient("TuneIn");
                var response = await httpClient.GetAsync(url, cancellationToken).ConfigureAwait(false);

                response.EnsureSuccessStatusCode();

                using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
                var xmlDocument = await XElement.LoadAsync(stream, LoadOptions.None, cancellationToken).ConfigureAwait(false);

                var items = from outline in xmlDocument.Descendants("outline")
                            let guide_id = outline.Attribute("guide_id")
                            let text = outline.Attribute("text")
                            where guide_id is not null && !string.IsNullOrEmpty(guide_id.Value)
                            select new Genre
                            {
                                Id = guide_id.Value,
                                Name = text?.Value
                            };

                return items.ToDictionary(t => t.Id!, t => t);
            }
        }
    }
}
