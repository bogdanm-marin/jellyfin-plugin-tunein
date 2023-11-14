using System;
using Jellyfin.Plugin.TuneIn.Channels;
using Jellyfin.Plugin.TuneIn.Configuration;
using Jellyfin.Plugin.TuneIn.Filters;
using Jellyfin.Plugin.TuneIn.Providers;
using Jellyfin.Plugin.TuneIn.Providers.Genres;
using Jellyfin.Plugin.TuneIn.Providers.Handlers.MediaTypeHandlers;
using Jellyfin.Plugin.TuneIn.Providers.Handlers.UriHandlers;
using Jellyfin.Plugin.TuneIn.Providers.MediaSourceInformation;
using MediaBrowser.Common.Plugins;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

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
            serviceCollection
                .AddMemoryCache()
                .AddSingleton<CacheProvider>()
                ;

            serviceCollection
                .AddScoped<TuneInChannel>()
                .AddScoped<Plugin>();

            serviceCollection
                .AddScoped<ChannelItemInfoProvider>()
                .AddScoped<TuneInUriProvider>()
                .AddScoped<MediaSourceInfoProviderManager>()
                .AddScoped<IMediaSourceInfoProvider, MediaSourceInfoProvider>()
                .AddScoped<MediaSourceInfoProvider>()
                .AddScoped<IMediaSourceInfoProvider, TuneInMediaSourceInfoProvider>()
                .AddScoped<GenresProvider>()
                ;

            serviceCollection
                .AddOptions<MediaSourceInfoUrlFilterOptions>()
                .Configure<Plugin>((o, p) => o.AddFilters(p.Configuration?.FilterUrls))
                ;

            serviceCollection
                .AddScoped<IMediaSourceInfoFilter, MediaSourceInfoUrlFilter>()
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
