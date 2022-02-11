using System;
using System.Collections.Generic;
using System.Linq;

namespace Rinzler78.NetExtension
{
    public static class ICollectionHelper
    {
        public static void Set<T>(this ICollection<T> currentItems, IEnumerable<T> newItems)
        {
            if (currentItems == null)
                return;

            lock (currentItems)
            {
                if (newItems == null)
                    currentItems.Clear();
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
}