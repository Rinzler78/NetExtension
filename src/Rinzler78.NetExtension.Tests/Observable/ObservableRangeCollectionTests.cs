using System.Collections.Specialized;
using System.ComponentModel;
using Rinzler78.NetExtension.Observable;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.Observable;

[Trait("Category", "Unit")]
public class ObservableRangeCollectionTests
{
    [Fact]
    public void Constructor_ShouldCreateEmptyCollection()
    {
        // Act
        var collection = new ObservableRangeCollection<int>();

        // Assert
        collection.Should().BeEmpty();
        collection.Count.Should().Be(0);
    }

    [Fact]
    public void Constructor_ShouldCreateCollectionFromEnumerable()
    {
        // Arrange
        var items = TestData.Collections.IntList;

        // Act
        var collection = new ObservableRangeCollection<int>(items);

        // Assert
        collection.Should().HaveCount(items.Count);
        collection.Should().BeEquivalentTo(items);
    }

    [Fact]
    public void AddRange_ShouldAddMultipleItems()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>();
        var itemsToAdd = new[] { 1, 2, 3, 4, 5 };

        // Act
        collection.AddRange(itemsToAdd);

        // Assert
        collection.Should().HaveCount(5);
        collection.Should().ContainInOrder(itemsToAdd);
    }

    [Fact]
    public void AddRange_ShouldRaiseCollectionChangedEvent()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>();
        var itemsToAdd = new[] { 1, 2, 3 };
        NotifyCollectionChangedEventArgs? eventArgs = null;

        collection.CollectionChanged += (sender, e) => eventArgs = e;

        // Act
        collection.AddRange(itemsToAdd);

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Add);
        eventArgs.NewItems.Should().BeEquivalentTo(itemsToAdd);
    }

    [Fact]
    public void AddRange_ShouldRaisePropertyChangedEvents()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>();
        var itemsToAdd = new[] { 1, 2, 3 };
        var propertyChangedEvents = new List<string>();

        ((INotifyPropertyChanged)collection).PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != null)
                propertyChangedEvents.Add(e.PropertyName);
        };

        // Act
        collection.AddRange(itemsToAdd);

        // Assert
        propertyChangedEvents.Should().Contain("Count");
        propertyChangedEvents.Should().Contain("Item[]");
    }

    [Fact]
    public void AddRange_ShouldHandleEmptyCollection()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>();
        var emptyItems = new int[0];

        // Act
        collection.AddRange(emptyItems);

        // Assert
        collection.Should().BeEmpty();
    }

    [Fact]
    public void AddRange_ShouldHandleNullCollection()
    {
        // Arrange
        var collection = new ObservableRangeCollection<string>();

        // Act & Assert
        collection.Invoking(x => x.AddRange(null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddRange_WithInvalidNotificationMode_ShouldThrowArgumentException()
    {
        var collection = new ObservableRangeCollection<int>();

        collection.Invoking(x => x.AddRange(new[] { 1, 2 }, NotifyCollectionChangedAction.Remove))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveRange_ShouldRemoveMultipleItems()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3, 4, 5 });
        var itemsToRemove = new[] { 2, 4 };

        // Act
        collection.RemoveRange(itemsToRemove);

        // Assert
        collection.Should().HaveCount(3);
        collection.Should().ContainInOrder(1, 3, 5);
    }

    [Fact]
    public void RemoveRange_ShouldRaiseCollectionChangedEvent()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3, 4, 5 });
        var itemsToRemove = new[] { 2, 4 };
        NotifyCollectionChangedEventArgs? eventArgs = null;

        collection.CollectionChanged += (sender, e) => eventArgs = e;

        // Act
        collection.RemoveRange(itemsToRemove);

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Remove);
        eventArgs.OldItems.Should().BeEquivalentTo(itemsToRemove);
    }

    [Fact]
    public void RemoveRange_ShouldHandleNonExistentItems()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3 });
        var itemsToRemove = new[] { 4, 5, 6 };

        // Act
        collection.RemoveRange(itemsToRemove);

        // Assert
        collection.Should().HaveCount(3);
        collection.Should().ContainInOrder(1, 2, 3);
    }

    [Fact]
    public void RemoveRange_ShouldHandleNullCollection()
    {
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3 });

        collection.Invoking(x => x.RemoveRange(null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveRange_WithInvalidNotificationMode_ShouldThrowArgumentException()
    {
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3 });

        collection.Invoking(x => x.RemoveRange(new[] { 1 }, NotifyCollectionChangedAction.Add))
            .Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveRange_WithResetNotification_ShouldRaiseSingleResetEvent()
    {
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3, 4 });
        NotifyCollectionChangedEventArgs? eventArgs = null;

        collection.CollectionChanged += (_, e) => eventArgs = e;

        collection.RemoveRange(new[] { 2, 4 }, NotifyCollectionChangedAction.Reset);

        collection.Should().ContainInOrder(1, 3);
        eventArgs.Should().NotBeNull();
        eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Reset);
    }

    [Fact]
    public void RemoveRange_WithResetNotificationAndNoMatches_ShouldStillRaiseResetEvent()
    {
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3 });
        NotifyCollectionChangedEventArgs? eventArgs = null;

        collection.CollectionChanged += (_, e) => eventArgs = e;

        collection.RemoveRange(new[] { 8, 9 }, NotifyCollectionChangedAction.Reset);

        collection.Should().ContainInOrder(1, 2, 3);
        eventArgs.Should().NotBeNull();
        eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Reset);
    }

    [Fact]
    public void ReplaceRange_ShouldReplaceAllItems()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3 });
        var newItems = new[] { 4, 5, 6, 7 };

        // Act
        collection.ReplaceRange(newItems);

        // Assert
        collection.Should().HaveCount(4);
        collection.Should().ContainInOrder(newItems);
    }

    [Fact]
    public void ReplaceRange_ShouldRaiseCollectionChangedEvent()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3 });
        var newItems = new[] { 4, 5, 6 };
        NotifyCollectionChangedEventArgs? eventArgs = null;

        collection.CollectionChanged += (sender, e) => eventArgs = e;

        // Act
        collection.ReplaceRange(newItems);

        // Assert
        eventArgs.Should().NotBeNull();
        eventArgs!.Action.Should().Be(NotifyCollectionChangedAction.Reset);
    }

    [Fact]
    public void ReplaceRange_ShouldHandleEmptyCollection()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3 });
        var emptyItems = new int[0];

        // Act
        collection.ReplaceRange(emptyItems);

        // Assert
        collection.Should().BeEmpty();
    }

    [Fact]
    public void Replace_ShouldReplaceWithSingleItem()
    {
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3 });

        collection.Replace(9);

        collection.Should().ContainSingle().Which.Should().Be(9);
    }

    [Fact]
    public void ReplaceRange_WithNullCollection_ShouldThrowArgumentNullException()
    {
        var collection = new ObservableRangeCollection<int>(new[] { 1, 2, 3 });

        collection.Invoking(x => x.ReplaceRange(null!)).Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ReplaceRange_WhenAlreadyEmptyAndReplacingWithEmptyCollection_ShouldNotRaiseEvent()
    {
        var collection = new ObservableRangeCollection<int>();
        var eventCount = 0;

        collection.CollectionChanged += (_, _) => eventCount++;

        collection.ReplaceRange(System.Array.Empty<int>());

        collection.Should().BeEmpty();
        eventCount.Should().Be(0);
    }

    [Fact]
    public async Task Operations_ShouldBeThreadSafe()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>();
        const int threadCount = 5;
        const int itemsPerThread = 100;

        // Act
        var tasks = new List<Task>();

        for (int i = 0; i < threadCount; i++)
        {
            var threadId = i;
            tasks.Add(Task.Run(() =>
            {
                var itemsToAdd = Enumerable.Range(threadId * itemsPerThread, itemsPerThread).ToArray();
                collection.AddRange(itemsToAdd);
            }));
        }

        await Task.WhenAll(tasks);

        // Assert
        collection.Should().HaveCount(threadCount * itemsPerThread);
    }

    [Fact]
    public void AddRange_WithNotificationMode_ShouldSuppressNotifications()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>();
        var eventCount = 0;

        collection.CollectionChanged += (sender, e) => eventCount++;

        // Act
        collection.AddRange(new[] { 1, 2, 3 }, NotifyCollectionChangedAction.Reset);

        // Assert
        collection.Should().HaveCount(3);
        eventCount.Should().Be(1); // Only one Reset event
    }

    [Fact]
    public void MultipleOperations_ShouldMaintainConsistency()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>();

        // Act
        collection.AddRange(new[] { 1, 2, 3, 4, 5 });
        collection.RemoveRange(new[] { 2, 4 });
        collection.AddRange(new[] { 6, 7 });
        collection.ReplaceRange(new[] { 10, 11, 12 });

        // Assert
        collection.Should().HaveCount(3);
        collection.Should().ContainInOrder(10, 11, 12);
    }

    [Fact]
    public void AddRange_ShouldHandleLargeCollections()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>();
        var largeCollection = Enumerable.Range(1, 10000).ToArray();

        // Act
        collection.AddRange(largeCollection);

        // Assert
        collection.Should().HaveCount(10000);
        collection.Should().ContainInOrder(largeCollection);
    }

    [Fact]
    public void CollectionChanged_ShouldProvideCorrectSender()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>();
        object? sender = null;

        collection.CollectionChanged += (s, e) => sender = s;

        // Act
        collection.AddRange(new[] { 1, 2, 3 });

        // Assert
        sender.Should().Be(collection);
    }

    [Fact]
    public void AddRange_WithForwardOnlyEnumerable_NotificationContainsAllItems()
    {
        // Arrange
        var collection = new ObservableRangeCollection<int>();
        List<int>? notifiedItems = null;
        collection.CollectionChanged += (_, e) =>
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                notifiedItems = e.NewItems?.Cast<int>().ToList();
        };

        // A forward-only LINQ enumerable (not a List)
        IEnumerable<int> ForwardOnly()
        {
            yield return 1;
            yield return 2;
            yield return 3;
        }

        // Act
        collection.AddRange(ForwardOnly());

        // Assert
        collection.Should().HaveCount(3);
        notifiedItems.Should().NotBeNull().And.BeEquivalentTo(new[] { 1, 2, 3 });
    }
}
