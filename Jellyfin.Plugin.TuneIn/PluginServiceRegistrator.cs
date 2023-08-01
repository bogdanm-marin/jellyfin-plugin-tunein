using Jellyfin.Plugin.TuneIn.Channels;
using Jellyfin.Plugin.TuneIn.Providers;
using Jellyfin.Plugin.TuneIn.Providers.Handlers.MediaTypeHandlers;
using Jellyfin.Plugin.TuneIn.Providers.Handlers.UriHandlers;
using Jellyfin.Plugin.TuneIn.Providers.MediaSourceInformation;
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
                .AddScoped<ChannelItemInfoProvider>()
                .AddScoped<TuneInUriProvider>()
                .AddScoped<MediaSourceInfoProviderManager>()
                .AddScoped<IMediaSourceInfoProvider, MediaSourceInfoProvider>()
                .AddScoped<MediaSourceInfoProvider>()
                .AddScoped<IMediaSourceInfoProvider, TuneInMediaSourceInfoProvider>()

                ;

            serviceCollection
                .AddScoped<IUriHandler, KnownExtensionsUriHandler>()
                .AddScoped<IUriHandler, M3U8ExtensionUriHandler>()
                .AddScoped<IUriHandler, PlsExtensionUriHandler>()
                .AddScoped<IUriHandler, ProcessUriHandler>()
                ;

            serviceCollection
                .AddScoped<IHttpResponseMessageHandler, MpegUrlMediaTypeHandler>()
                .AddScoped<IHttpResponseMessageHandler, AppleMpegUrlMediaTypeHandler>()
                .AddScoped<IHttpResponseMessageHandler, KnownMediaTypeHandler>()
                .AddScoped<IHttpResponseMessageHandler, ScplsMediaTypeHandler>()
                ;
        }
    }
}
