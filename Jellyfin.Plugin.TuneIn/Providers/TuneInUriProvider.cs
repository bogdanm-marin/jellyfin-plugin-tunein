using System;
using System.Collections.Specialized;
using System.Web;

namespace Jellyfin.Plugin.TuneIn.Providers
{
    /// <summary>
    /// TuneInUriProvider provides supported API uris for TuneIn.
    /// </summary>
    public class TuneInUriProvider
    {
        private readonly Uri _browseUri;
        private readonly Uri _searchUri;
        private readonly Plugin _plugin;

        /// <summary>
        /// Initializes a new instance of the <see cref="TuneInUriProvider"/> class.
        /// </summary>
        /// <param name="plugin">Instance of <see cref="Plugin"/>.</param>
        public TuneInUriProvider(Plugin plugin)
        {
            _plugin = plugin;

            var rootUri = new Uri(RootUri);
            _browseUri = new Uri(rootUri, "Browse.ashx");
            _searchUri = new Uri(rootUri, "Search.ashx");
        }

        /// <summary>
        /// Gets TuneIn API root uri.
        /// </summary>
        public string RootUri => "http://opml.radiotime.com";

        /// <summary>
        /// Gets browse Uri for TuneIn.
        /// </summary>
        public Uri BrowseUri
        {
            get
            {
                var uriBuilder = new UriBuilder(_browseUri);

                uriBuilder.Query = GetQueryParams(uriBuilder.Query).ToString();

                return uriBuilder.Uri;
            }
        }

        /// <summary>
        /// Gets Local Uri for TuneIn.
        /// </summary>
        public Uri LocalUri
        {
            get
            {
                var uriBuilder = new UriBuilder(_browseUri);

                var queryParams = GetQueryParams(uriBuilder.Query);
                queryParams.Add("c", "local");

                uriBuilder.Query = queryParams.ToString();

                return uriBuilder.Uri;
            }
        }

        /// <summary>
        /// Gets Favorites Uri for TuneIn.
        /// </summary>
        public Uri? FavoritesUri
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_plugin.Configuration.Username))
                {
                    return default;
                }

                var uriBuilder = new UriBuilder(_browseUri);

                var queryParams = GetQueryParams(uriBuilder.Query);
                queryParams.Add("c", "presets");

                uriBuilder.Query = queryParams.ToString();

                return uriBuilder.Uri;
            }
        }

        /// <summary>
        /// Gets Popular Uri for TuneIn.
        /// </summary>
        public Uri PopularUri
        {
            get
            {
                var uriBuilder = new UriBuilder(_browseUri);

                var queryParams = GetQueryParams(uriBuilder.Query);
                queryParams.Add("c", "popular");

                uriBuilder.Query = queryParams.ToString();

                return uriBuilder.Uri;
            }
        }

        /// <summary>
        /// Returns the search URI.
        /// </summary>
        /// <param name="searchTerm">Search Term.</param>
        /// <returns><see cref="Uri"/>.</returns>
        public Uri GetSearchUri(string searchTerm)
        {
            var uriBuilder = new UriBuilder(_searchUri);

            var queryParams = GetQueryParams(uriBuilder.Query);
            queryParams.Add("query", searchTerm);

            uriBuilder.Query = queryParams.ToString();

            return uriBuilder.Uri;
        }

        private NameValueCollection GetQueryParams(string query)
        {
            var queryParams = HttpUtility.ParseQueryString(query);
            queryParams.Add("formats", "mp3,aac,ogg,hls,flac,wma,wav");

            if (!string.IsNullOrEmpty(_plugin.Configuration.PartnerId))
            {
                queryParams.Add("partnerId", _plugin.Configuration.PartnerId);
            }

            if (!string.IsNullOrWhiteSpace(_plugin.Configuration.Username))
            {
                queryParams.Add("username", _plugin.Configuration.Username);
            }

            if (!string.IsNullOrEmpty(_plugin.Configuration.LatitudeLongitude))
            {
                queryParams.Add("latlon", _plugin.Configuration.LatitudeLongitude);
            }

            return queryParams;
        }
    }
}
