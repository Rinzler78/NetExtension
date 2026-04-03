using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Collections;

/// <summary>
/// Tests for CollectionHelper extension methods, including performance optimizations.
/// </summary>
[Trait("Category", "Unit")]
public class CollectionHelperTests
{
    [Fact]
    public void Set_WithNullCurrentItems_ShouldDoNothing()
    {
        // Arrange
        List<int>? currentItems = null;
        var newItems = new[] { 1, 2, 3 };

        // Act
        CollectionHelper.Set(currentItems!, newItems);

        // Assert
        currentItems.Should().BeNull();
    }

    [Fact]
    public void Set_WithNullNewItems_ShouldClearCurrentItems()
    {
        // Arrange
        var currentItems = new List<int> { 1, 2, 3 };
        IEnumerable<int>? newItems = null;

        // Act
        currentItems.Set(newItems!);

        // Assert
        currentItems.Should().BeEmpty();
    }

    [Fact]
    public void Set_WithEmptyNewItems_ShouldClearCurrentItems()
    {
        // Arrange
        var currentItems = new List<int> { 1, 2, 3 };
        var newItems = new int[0];

        // Act
        currentItems.Set(newItems);

        // Assert
        currentItems.Should().BeEmpty();
    }

    [Fact]
    public void Set_WithSameItems_ShouldNotChangeCollection()
    {
        // Arrange
        var currentItems = new List<int> { 1, 2, 3 };
        var newItems = new[] { 1, 2, 3 };

        // Act
        currentItems.Set(newItems);

        // Assert
        currentItems.Should().HaveCount(3);
        currentItems.Should().Contain(1);
        currentItems.Should().Contain(2);
        currentItems.Should().Contain(3);
    }

    [Fact]
    public void Set_WithDifferentItems_ShouldSynchronizeCollection()
    {
        // Arrange
        var currentItems = new List<int> { 1, 2, 3 };
        var newItems = new[] { 2, 3, 4 };

        // Act
        currentItems.Set(newItems);

        // Assert
        currentItems.Should().HaveCount(3);
        currentItems.Should().NotContain(1);
        currentItems.Should().Contain(2);
        currentItems.Should().Contain(3);
        currentItems.Should().Contain(4);
    }

    [Fact]
    public void Set_WithCompletelyDifferentItems_ShouldReplaceAllItems()
    {
        // Arrange
        var currentItems = new List<int> { 1, 2, 3 };
        var newItems = new[] { 4, 5, 6 };

        // Act
        currentItems.Set(newItems);

        // Assert
        currentItems.Should().HaveCount(3);
        currentItems.Should().Contain(4);
        currentItems.Should().Contain(5);
        currentItems.Should().Contain(6);
        currentItems.Should().NotContain(1);
        currentItems.Should().NotContain(2);
        currentItems.Should().NotContain(3);
    }

    [Fact]
    public void Set_WithLargerNewCollection_ShouldAddItems()
    {
        // Arrange
        var currentItems = new List<int> { 1, 2 };
        var newItems = new[] { 1, 2, 3, 4, 5 };

        // Act
        currentItems.Set(newItems);

        // Assert
        currentItems.Should().HaveCount(5);
        currentItems.Should().Contain(new[] { 1, 2, 3, 4, 5 });
    }

    [Fact]
    public void Set_WithSmallerNewCollection_ShouldRemoveItems()
    {
        // Arrange
        var currentItems = new List<int> { 1, 2, 3, 4, 5 };
        var newItems = new[] { 1, 2 };

        // Act
        currentItems.Set(newItems);

        // Assert
        currentItems.Should().HaveCount(2);
        currentItems.Should().Contain(1);
        currentItems.Should().Contain(2);
        currentItems.Should().NotContain(3);
        currentItems.Should().NotContain(4);
        currentItems.Should().NotContain(5);
    }

    /// <summary>
    /// P0-6: Set uses a "not already present" guard when adding items. Starting from an
    /// empty collection every element of newItems passes the guard, so duplicates in
    /// newItems are preserved verbatim in the result.
    /// </summary>
    [Fact]
    public void Set_WithDuplicatesInNewItems_ShouldPreserveDuplicates()
    {
        // Arrange
        var currentItems = new List<int>();
        var newItems = new[] { 1, 2, 2, 3, 3, 3 };

        // Act
        currentItems.Set(newItems);

        // Assert — starting from empty, every element of newItems is "not yet in collection"
        // and is therefore added; duplicates survive.
        currentItems.Should().HaveCount(6, "all 6 entries from newItems are added when starting from an empty collection");
        currentItems.Count(x => x == 2).Should().Be(2, "both occurrences of 2 from newItems are preserved");
        currentItems.Count(x => x == 3).Should().Be(3, "all three occurrences of 3 from newItems are preserved");
    }

    [Theory]
    [InlineData(5)]    // Small collection - should use O(n²) algorithm
    [InlineData(15)]   // Large collection - should use O(n) HashSet algorithm
    [InlineData(100)]  // Very large collection - should use O(n) HashSet algorithm
    public void Set_WithVariousCollectionSizes_ShouldWorkCorrectly(int size)
    {
        // Arrange
        var currentItems = new List<int>(Enumerable.Range(1, size));
        var newItems = Enumerable.Range(size / 2, size).ToArray(); // Overlap half the items

        // Act
        var stopwatch = Stopwatch.StartNew();
        currentItems.Set(newItems);
        stopwatch.Stop();

        // Assert
        currentItems.Should().HaveCount(size);

        // Verify all items from newItems are present
        currentItems.Should().Contain(newItems);

        // Verify items not in newItems are not present
        for (int i = 1; i < size / 2; i++)
        {
            currentItems.Should().NotContain(i);
        }

        // Performance assertion - larger collections should complete in reasonable time
        if (size >= 100)
        {
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(100,
                $"Set operation took {stopwatch.ElapsedMilliseconds}ms for {size} items, expected < 100ms");
        }
    }

    [Fact]
    public void Set_PerformanceTest_LargeCollections()
    {
        // Arrange
        const int size = 1000;
        var currentItems = new List<int>(Enumerable.Range(1, size));
        var newItems = Enumerable.Range(500, size).ToArray(); // 50% overlap

        // Act
        var stopwatch = Stopwatch.StartNew();
        currentItems.Set(newItems);
        stopwatch.Stop();

        // Assert
        currentItems.Should().HaveCount(size);

        // Performance assertion - should complete quickly with HashSet optimization
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50,
            $"Set operation took {stopwatch.ElapsedMilliseconds}ms for {size} items, expected < 50ms with HashSet optimization");
    }

    /// <summary>
    /// P2-8: Verifies that concurrent Set calls do not throw and that the final collection
    /// contains exactly the expected values (data-integrity check, not just no-exception).
    /// Assertions are placed after Task.WhenAll (all tasks complete) and inside a lock on
    /// currentItems to document thread-safety intent explicitly.
    /// </summary>
    [Fact]
    public async Task Set_ThreadSafety_WithConcurrentAccess()
    {
        // Arrange
        var currentItems = new List<int> { 1, 2, 3 };
        var newItems = new[] { 2, 3, 4 };
        var exceptions = new List<Exception>();

        // Act
        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    currentItems.Set(newItems);
                }
                catch (Exception ex)
                {
                    lock (exceptions)
                    {
                        exceptions.Add(ex);
                    }
                }
            }));
        }

        await Task.WhenAll(tasks.ToArray());

        // Assert — evaluated after all tasks have completed; lock guards memory-visibility.
        exceptions.Should().BeEmpty("no task should throw during concurrent Set calls");
        lock (currentItems)
        {
            // Data-integrity: the collection must contain exactly {2, 3, 4} and nothing else.
            currentItems.Should().BeEquivalentTo(new[] { 2, 3, 4 },
                "after all concurrent Set calls settle, the collection must equal newItems exactly");
        }
    }

    [Fact]
    public void Set_WithStringCollection_ShouldWorkCorrectly()
    {
        // Arrange
        var currentItems = new List<string> { "apple", "banana", "cherry" };
        var newItems = new[] { "banana", "cherry", "date" };

        // Act
        currentItems.Set(newItems);

        // Assert
        currentItems.Should().HaveCount(3);
        currentItems.Should().NotContain("apple");
        currentItems.Should().Contain("banana");
        currentItems.Should().Contain("cherry");
        currentItems.Should().Contain("date");
    }

    [Fact]
    public void Set_WithComplexObjects_ShouldWorkCorrectly()
    {
        // Arrange
        var person1 = new Person { Id = 1, Name = "John" };
        var person2 = new Person { Id = 2, Name = "Jane" };
        var person3 = new Person { Id = 3, Name = "Bob" };
        var person4 = new Person { Id = 4, Name = "Alice" };

        var currentItems = new List<Person> { person1, person2, person3 };
        var newItems = new[] { person2, person3, person4 };

        // Act
        currentItems.Set(newItems);

        // Assert
        currentItems.Should().HaveCount(3);
        currentItems.Should().NotContain(person1);
        currentItems.Should().Contain(person2);
        currentItems.Should().Contain(person3);
        currentItems.Should().Contain(person4);
    }

    private class Person
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
