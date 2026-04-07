using System.Collections.Generic;
using System.Linq;

namespace Rinzler78.NetExtension;

/// <summary>
/// Provides utility methods for collection manipulation and synchronization operations.
/// </summary>
public static class CollectionHelper
{
    #region Constants

    /// <summary>
    /// Threshold for using HashSet optimization in collection operations.
    /// For collections smaller than this, direct Contains operations are used.
    /// For larger collections, HashSet is used for O(1) lookups.
    /// </summary>
    private const int HashSetOptimizationThreshold = 10;

    #endregion

    /// <summary>
    /// Synchronizes a collection to match a new set of items by removing items not in the new set
    /// and adding items that are missing.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection</typeparam>
    /// <param name="currentItems">The collection to synchronize. If null, the operation is ignored</param>
    /// <param name="newItems">The new set of items to synchronize to. If null, the collection will be cleared</param>
    /// <remarks>
    /// <para>Performance characteristics:</para>
    /// <list type="bullet">
    /// <item>Small collections (&lt; 10 items): O(n²) using direct Contains operations</item>
    /// <item>Large collections (≥ 10 items): O(n) using HashSet for lookups</item>
    /// </list>
    /// <para>This method is not thread-safe. Callers must synchronize access externally if used concurrently.</para>
    /// <para>Items are first removed, then added to minimize collection change notifications.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var currentList = new List&lt;int&gt; { 1, 2, 3 };
    /// var newItems = new[] { 2, 3, 4 };
    /// currentList.Set(newItems); // currentList now contains { 2, 3, 4 }
    ///
    /// currentList.Set(null); // currentList is now empty
    /// </code>
    /// </example>
    public static void Set<T>(this ICollection<T> currentItems, IEnumerable<T> newItems)
    {
        if (currentItems is null)
            return;

        if (newItems is null)
        {
            currentItems.Clear();
            return;
        }

        // Convert to list to avoid multiple enumeration
        var newItemsList = newItems as IList<T> ?? newItems.ToList();

        // Use HashSet optimization for larger collections
        if (currentItems.Count >= HashSetOptimizationThreshold || newItemsList.Count >= HashSetOptimizationThreshold)
        {
            var newItemsSet = new HashSet<T>(newItemsList);
            var currentItemsSet = new HashSet<T>(currentItems);

            var itemsToDelete = currentItems.Where(arg => !newItemsSet.Contains(arg)).ToList();
            foreach (var item in itemsToDelete)
                currentItems.Remove(item);

            var itemsToAdd = newItemsList.Where(arg => !currentItemsSet.Contains(arg)).ToList();
            foreach (var item in itemsToAdd)
                currentItems.Add(item);
        }
        else
        {
            var itemsToDelete = currentItems.Where(arg => !newItemsList.Contains(arg)).ToList();
            foreach (var item in itemsToDelete)
                currentItems.Remove(item);

            var itemsToAdd = newItemsList.Where(arg => !currentItems.Contains(arg)).ToList();
            foreach (var item in itemsToAdd)
                currentItems.Add(item);
        }
    }
}
