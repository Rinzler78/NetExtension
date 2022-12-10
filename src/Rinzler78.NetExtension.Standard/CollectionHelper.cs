using System.Collections.Generic;
using System.Linq;

namespace Rinzler78.NetExtension;

public static class CollectionHelper
{
    public static void Set<T>(this ICollection<T> currentItems, IEnumerable<T> newItems)
    {
        if (currentItems is null)
            return;

        lock (currentItems)
        {
            if (newItems is null)
            {
                currentItems.Clear();
            }
            else
            {
                var itemsToDelete = currentItems.Where(arg => !newItems.Contains(arg)).ToList();

                foreach (var item in itemsToDelete)
                    currentItems.Remove(item);

                var itemsToAdd = newItems.Where(arg => !currentItems.Contains(arg)).ToList();

                foreach (var item in itemsToAdd)
                    currentItems.Add(item);
            }
        }
    }
}