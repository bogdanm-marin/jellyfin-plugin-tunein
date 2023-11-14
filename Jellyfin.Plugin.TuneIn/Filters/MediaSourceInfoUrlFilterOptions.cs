using System;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.Plugin.TuneIn.Filters
{
    /// <summary>
    /// Options for filtering Media Source Info Urls.
    /// </summary>
    public class MediaSourceInfoUrlFilterOptions
    {
        /// <summary>
        /// Gets urls to filter.
        /// </summary>
        public IList<string> FilterUrls { get; } = new List<string>();

        /// <summary>
        /// Adds filters to Filtering Urls.
        /// </summary>
        /// <param name="filters">urls to filter.</param>
        public void AddFilters(string? filters)
        {
            if (filters is null)
            {
                return;
            }

            foreach (var filter in filters
                .Split(new char[] { ';', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                this.FilterUrls.Add(filter);
            }
        }
    }
}
