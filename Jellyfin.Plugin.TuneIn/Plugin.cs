using System;
using System.Collections.Generic;
using Jellyfin.Plugin.TuneIn.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.TuneIn
{
    /// <summary>
    /// Plugin.
    /// </summary>
    public class Plugin : BasePlugin<PluginConfiguration>, IHasWebPages
    {
        /// <summary>
        /// Plugin Identifier.
        /// </summary>
        public const string Identifier = "9bbd4510-64c9-2cf5-54a5-098af5075895";

        /// <summary>
        /// Initializes a new instance of the <see cref="Plugin"/> class.
        /// </summary>
        /// <param name="applicationPaths">IApplicationPaths.</param>
        /// <param name="xmlSerializer">IXmlSerializer.</param>
        public Plugin(
            IApplicationPaths applicationPaths,
            IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
        }

        /// <inheritdoc/>
        public override Guid Id { get; } = Guid.Parse(Identifier);

        /// <inheritdoc/>
        public override string Name => "New TuneIn";

        /// <inheritdoc/>
        public IEnumerable<PluginPageInfo> GetPages()
        {
            return new[]
            {
                new PluginPageInfo
                {
                    Name = "Tune In",
                    EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
                }
            };
        }
    }
}
