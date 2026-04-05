using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Rinzler78.NetExtension.Observable;

public sealed class ObservableRangeCollection<T> : ObservableCollection<T>
{
    #region Constants

    /// <summary>
    /// Default starting index for collection operations.
    /// </summary>
    private const int DefaultStartingIndex = -1;

    #endregion
    private readonly object _lock = new();

    public ObservableRangeCollection()
    {
    }

    public ObservableRangeCollection(IEnumerable<T> collection)
        : base(collection)
    {
    }

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

    public void Replace(T item)
    {
        ReplaceRange(new[] { item });
    }

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
