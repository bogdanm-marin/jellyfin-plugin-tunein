using System;
using System.Collections.Generic;
using System.Linq;

namespace Jellyfin.Plugin.TuneIn.Extensions
{
    /// <summary>
    /// Collection of extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class IEnumerableExtensions
    {
        /// <summary>
        /// Invokes action for each item in the enumeration.
        /// </summary>
        /// <typeparam name="T">Type.</typeparam>
        /// <param name="source">Enumeration.</param>
        /// <param name="action">Action to be invoked for each item.</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (var item in source)
            {
                action.Invoke(item);
            }
        }

        /// <summary>
        /// Converts an enumeration of strings to an enumeration of Int32.
        /// </summary>
        /// <param name="source">Enumeration of <see cref="string"/>.</param>
        /// <returns>Enumeration of <see cref="int"/>.</returns>
        public static IEnumerable<int> ToInt32(this IEnumerable<string> source)
        {
            return source
                    .Select(_ => int.TryParse(_, out var v)
                                ? (Valid: true, Value: v)
                                : (Valid: false, Value: default))
                    .Where(_ => _.Valid)
                    .Select(_ => _.Value);
        }
    }
}
