using System;
using System.Linq;
using MediaBrowser.Model.Dto;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Jellyfin.Plugin.TuneIn.Filters
{
    /// <summary>
    /// Filters Media Source Info containing TuneIn ads.
    /// </summary>
    public class MediaSourceInfoUrlFilter : IMediaSourceInfoFilter
    {
        private readonly ILogger<MediaSourceInfoUrlFilter> _logger;
        private readonly MediaSourceInfoUrlFilterOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="MediaSourceInfoUrlFilter"/> class.
        /// Creates a new instance of .
        /// </summary>
        /// <param name="logger">logger options.</param>
        /// <param name="options">filtering options.</param>
        public MediaSourceInfoUrlFilter(
            ILogger<MediaSourceInfoUrlFilter> logger,
            IOptions<MediaSourceInfoUrlFilterOptions> options)
        {
            _logger = logger;
            _options = options.Value;
        }

        /// <summary>
        /// Checks if mediasourceinfo is allowed.
        /// </summary>
        /// <param name="mediaSourceInfo">media source info.</param>
        /// <returns>True/False.</returns>
        public bool IsAllowed(MediaSourceInfo mediaSourceInfo)
        {
            if (_options.FilterUrls.Any(f => mediaSourceInfo.Path.Contains(f, StringComparison.InvariantCultureIgnoreCase)))
            {
                _logger.LogInformation("Filter out: {Path}", mediaSourceInfo.Path);
                return false;
            }

            _logger.LogInformation("Filter in: {Path}", mediaSourceInfo.Path);
            return true;
        }
    }
}
