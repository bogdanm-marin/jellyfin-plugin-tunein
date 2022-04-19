using Jellyfin.Plugin.TuneIn.Channels;
using Jellyfin.Plugin.TuneIn.Providers;
using Jellyfin.Plugin.TuneIn.Providers.Handlers.MediaTypeHandlers;
using Jellyfin.Plugin.TuneIn.Providers.Handlers.UriHandlers;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.DependencyInjection;

namespace Jellyfin.Plugin.TuneIn
{
    /// <summary>
    /// PluginServiceRegistrator.
    /// </summary>
    public class PluginServiceRegistrator : IPluginServiceRegistrator
    {
        /// <inheritdoc/>
        public void RegisterServices(IServiceCollection serviceCollection)
        {
            serviceCollection.AddScoped<TuneInChannel>();
            serviceCollection.AddScoped<Plugin>();

            serviceCollection
                .AddSingleton<ChannelItemInfoProvider>()
                .AddSingleton<MediaSourceInfoProvider>();

            serviceCollection
                .AddSingleton<IUriHandler, KnownExtensionsUriHandler>()
                .AddSingleton<IUriHandler, M3U8ExtensionUriHandler>()
                .AddSingleton<IUriHandler, PlsExtensionUriHandler>()
                .AddSingleton<IUriHandler, ProcessUriHandler>()
                ;

            serviceCollection
                .AddSingleton<IHttpResponseMessageHandler, MpegUrlMediaTypeHandler>()
                .AddSingleton<IHttpResponseMessageHandler, AppleMpegUrlMediaTypeHandler>()
                .AddSingleton<IHttpResponseMessageHandler, KnownMediaTypeHandler>()
                .AddSingleton<IHttpResponseMessageHandler, ScplsMediaTypeHandler>()
                ;
        }
    }
}
