using System;
using MediaBrowser.Model.Dto;

namespace Jellyfin.Plugin.TuneIn.Filters
{
    /// <summary>
    /// Filters Media Source Info containing TuneIn ads.
    /// </summary>
    public class TuneInAdsMediaSourceInfoFilter : IMediaSourceInfoFilter
    {
        /// <summary>
        /// Checks if mediasourceinfo is allowed.
        /// </summary>
        /// <param name="mediaSourceInfo">media source info.</param>
        /// <returns>True/False.</returns>
        public bool IsAllowed(MediaSourceInfo mediaSourceInfo)
        {
            if (mediaSourceInfo.Path.Contains("fns.tunein.com", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            return true;
        }
    }
}
