using System;

namespace Jellyfin.Plugin.TuneIn.Extensions
{
    /// <summary>
    /// Extension methods for Uri.
    /// </summary>
    public static class UriExtensions
    {
        /// <summary>
        /// Checks if Uri Scheme is http.
        /// </summary>
        /// <param name="uri">Uri.</param>
        /// <returns>Returns true if Scheme is http, false otherwise.</returns>
        public static bool SchemeIsHttp(this Uri uri)
        {
            return uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if Uri Scheme is https.
        /// </summary>
        /// <param name="uri">Uri.</param>
        /// <returns>Returns true if Scheme is https, false otherwise.</returns>
        public static bool SchemeIsHttps(this Uri uri)
        {
            return uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Checks if Uri Scheme is http or https.
        /// </summary>
        /// <param name="uri">Uri.</param>
        /// <returns>Returns true if Scheme is http or https, false otherwise.</returns>
        public static bool SchemeIsHttpOrHttps(this Uri uri)
        {
            return SchemeIsHttp(uri) || SchemeIsHttps(uri);
        }
    }
}
