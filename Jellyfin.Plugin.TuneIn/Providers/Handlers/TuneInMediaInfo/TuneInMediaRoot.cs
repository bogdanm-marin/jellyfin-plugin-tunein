using System.Collections.Generic;

namespace Jellyfin.Plugin.TuneIn.Providers.Handlers.TuneInMediaInfo
{
    /// <summary>
    /// TuneInMedia root.
    /// </summary>
    /// <typeparam name="TBody">Body Type.</typeparam>
    public class TuneInMediaRoot<TBody>
    {
        /// <summary>
        /// Gets or Sets Head section.
        /// </summary>
        public Head? Head { get; set; }

        /// <summary>
        /// Gets or Sets Body section.
        /// </summary>
        public IReadOnlyList<TBody>? Body { get; set; }
    }
}
