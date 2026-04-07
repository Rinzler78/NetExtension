using System;
using System.Collections.Generic;
using System.Linq;

namespace Rinzler78.NetExtension.Linq;

/// <summary>
/// Provides extension methods for <see cref="IEnumerable{T}"/> sequences.
/// </summary>
public static class EnumerableExtension
{
    /// <summary>
    /// Attempts to aggregate the values of a sequence using the specified function.
    /// Returns the default value if the operation fails.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence.</param>
    /// <param name="func">The aggregation function.</param>
    /// <returns>The aggregated value, or the default value if the operation fails.</returns>
    public static TSource? TryAggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
    {
        if (source == null || func == null)
            return default;

        try
        {
            return source.Aggregate(func);
        }
        catch (InvalidOperationException)
        {
            // Empty sequence
            return default;
        }
    }
}
