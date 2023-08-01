using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Jellyfin.Plugin.TuneIn.Providers
{
    /// <summary>
    /// Cache Provider.
    /// </summary>
    public class CacheProvider
    {
        private readonly IMemoryCache _memoryCache;
        private static readonly SemaphoreSlim _semaphoreSlim = new(1, 1);

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheProvider"/> class..
        /// </summary>
        /// <param name="memoryCache">Memory Cache handler.</param>
        public CacheProvider(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        /// <summary>
        /// Gets items from cache.
        /// </summary>
        /// <typeparam name="TOurput">Output type of cached item.</typeparam>
        /// <param name="cacheKey">Cache Key.</param>
        /// <param name="resolveItemFunc">Function returns item to be cached.</param>
        /// <param name="cancellationToken">Cancellation Token.</param>
        /// <returns>Cached item.</returns>
        public async Task<TOurput> GetCached<TOurput>(string cacheKey, Func<CancellationToken, Task<TOurput>> resolveItemFunc, CancellationToken cancellationToken)
        {
            if (_memoryCache.TryGetValue<TOurput>(cacheKey, out TOurput value1))
            {
                return value1;
            }

            await _semaphoreSlim
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
            try
            {
                if (_memoryCache.TryGetValue<TOurput>(cacheKey, out TOurput value2))
                {
                    return value2;
                }

                var item = await resolveItemFunc
                                    .Invoke(cancellationToken)
                                    .ConfigureAwait(false);

                _memoryCache.Set(cacheKey, item);

                return item;
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }
    }
}
