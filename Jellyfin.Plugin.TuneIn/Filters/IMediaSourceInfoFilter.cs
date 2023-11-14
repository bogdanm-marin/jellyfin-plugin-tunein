using MediaBrowser.Model.Dto;

namespace Jellyfin.Plugin.TuneIn.Filters
{
    /// <summary>
    /// Media Source Info filter interface.
    /// </summary>
    public interface IMediaSourceInfoFilter
    {
        /// <summary>
        /// Checks if Media Source Info is allowed.
        /// </summary>
        /// <param name="mediaSourceInfo">Media Source Info item.</param>
        /// <returns>True/False.</returns>
        bool IsAllowed(MediaSourceInfo mediaSourceInfo);
    }
}
