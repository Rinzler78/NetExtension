using System;
using System.Collections.Generic;
using System.Linq;

namespace Rinlzer78.NetExtension.Linq
{
    public static class EnumerableExtension
    {
        public static TSource TryAggregate<TSource>(this IEnumerable<TSource> source, Func<TSource, TSource, TSource> func)
        {
            try
            {
                return source.Aggregate(func);
            }
            catch(Exception ex)
            {

            }

            return default(TSource);
        }
    }
}
