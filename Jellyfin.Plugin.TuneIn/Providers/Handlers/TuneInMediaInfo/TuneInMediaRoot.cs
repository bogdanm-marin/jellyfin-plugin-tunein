using System.Collections.Generic;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.TuneInMediaInfo
{
    /// <summary>
    /// TuneInMedia root.
    /// </summary>
    public class TuneInMediaRoot
    {
        /// <summary>
        /// Gets or Sets Head section.
        /// </summary>
        public Head? Head { get; set; }

        /// <summary>
        /// Gets or Sets Body section.
        /// </summary>
        public IReadOnlyList<Body>? Body { get; set; }
    }
}
