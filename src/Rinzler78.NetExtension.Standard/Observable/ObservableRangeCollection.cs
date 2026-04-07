using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Rinzler78.NetExtension.Observable;

/// <summary>
/// An <see cref="ObservableCollection{T}"/> that supports bulk add, remove, and replace operations with batched change notifications.
/// </summary>
/// <typeparam name="T">The type of elements in the collection.</typeparam>
public sealed class ObservableRangeCollection<T> : ObservableCollection<T>
{
    #region Constants

    /// <summary>
    /// Default starting index for collection operations.
    /// </summary>
    private const int DefaultStartingIndex = -1;

    #endregion
    private readonly object _lock = new();

    /// <summary>
    /// Initializes a new empty instance of <see cref="ObservableRangeCollection{T}"/>.
    /// </summary>
    public ObservableRangeCollection()
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ObservableRangeCollection{T}"/> with items copied from the specified collection.
    /// </summary>
    /// <param name="collection">The collection whose elements are copied to the new list.</param>
    public ObservableRangeCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

    /// <summary>
    /// Adds a range of items to the collection, raising a single change notification.
    /// </summary>
    /// <param name="collection">The items to add.</param>
    /// <param name="notificationMode">The notification mode (<see cref="NotifyCollectionChangedAction.Add"/> or <see cref="NotifyCollectionChangedAction.Reset"/>).</param>
    public void AddRange(IEnumerable<T>? collection,
        NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Add)
    {
        if (notificationMode != NotifyCollectionChangedAction.Add &&
            notificationMode != NotifyCollectionChangedAction.Reset)
        {
            throw new ArgumentException("Mode must be either Add or Reset for AddRange.", nameof(notificationMode));
        }

        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        lock (_lock)
        {
            CheckReentrancy();

            var startIndex = Count;

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {
                var itemsAdded = AddRangeCore(collection);
                if (!itemsAdded)
                    return;

                RaiseChangeNotificationEvents(NotifyCollectionChangedAction.Reset);
                return;
            }

            // Snapshot once to avoid double-enumeration on forward-only IEnumerable sources
            var snapshot = collection is List<T> list ? list : new List<T>(collection);

            if (!AddRangeCore(snapshot))
                return;

            RaiseChangeNotificationEvents(
                NotifyCollectionChangedAction.Add,
                snapshot,
                startIndex);
        }
    }

    /// <summary>
    /// Removes a range of items from the collection, raising a single change notification.
    /// </summary>
    /// <param name="collection">The items to remove.</param>
    /// <param name="notificationMode">The notification mode (<see cref="NotifyCollectionChangedAction.Remove"/> or <see cref="NotifyCollectionChangedAction.Reset"/>).</param>
    public void RemoveRange(IEnumerable<T> collection,
        NotifyCollectionChangedAction notificationMode = NotifyCollectionChangedAction.Remove)
    {
        if (notificationMode != NotifyCollectionChangedAction.Remove &&
            notificationMode != NotifyCollectionChangedAction.Reset)
        {
            throw new ArgumentException("Mode must be either Remove or Reset for RemoveRange.",
                nameof(notificationMode));
        }

        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        lock (_lock)
        {
            CheckReentrancy();

            if (notificationMode == NotifyCollectionChangedAction.Reset)
            {
                var raiseEvents = false;
                foreach (var item in collection)
                {
                    Items.Remove(item);
                    raiseEvents = true;
                }

                if (raiseEvents)
                    RaiseChangeNotificationEvents(NotifyCollectionChangedAction.Reset);

                return;
            }

            var changedItems = new List<T>(collection);
            for (var i = 0; i < changedItems.Count; i++)
            {
                if (!Items.Remove(changedItems[i]))
                {
                    changedItems
                        .RemoveAt(i); //Can't use a foreach because changedItems is intended to be (carefully) modified
                    i--;
                }
            }

            if (changedItems.Count == 0)
                return;

            RaiseChangeNotificationEvents(
                NotifyCollectionChangedAction.Remove,
                changedItems);
        }
    }

    /// <summary>
    /// Replaces the entire collection with a single item.
    /// </summary>
    /// <param name="item">The item to replace the collection contents with.</param>
    public void Replace(T item)
    {
        ReplaceRange(new[] { item });
    }

    /// <summary>
    /// Clears the collection and replaces it with the specified items, raising a single reset notification.
    /// </summary>
    /// <param name="collection">The items to replace the collection contents with.</param>
    public void ReplaceRange(IEnumerable<T>? collection)
    {
        if (collection is null)
            throw new ArgumentNullException(nameof(collection));

        lock (_lock)
        {
            CheckReentrancy();

            var previouslyEmpty = Items.Count == 0;

            Items.Clear();

            AddRangeCore(collection);

            var currentlyEmpty = Items.Count == 0;

            if (previouslyEmpty && currentlyEmpty)
                return;

            RaiseChangeNotificationEvents(NotifyCollectionChangedAction.Reset);
        }
    }

    private bool AddRangeCore(IEnumerable<T>? collection)
    {
        var itemAdded = false;
        if (collection is not null)
            foreach (var item in collection)
            {
                Items.Add(item);
                itemAdded = true;
            }

        return itemAdded;
    }

    private void RaiseChangeNotificationEvents(NotifyCollectionChangedAction action, List<T>? changedItems = null,
        int startingIndex = DefaultStartingIndex)
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Count)));
        OnPropertyChanged(new PropertyChangedEventArgs("Item[]"));

        if (changedItems is null)
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action));
        else
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, changedItems, startingIndex));
    }
}
