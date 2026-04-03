using System.Numerics;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Rinzler78.NetExtension.Json;
using Rinzler78.NetExtension.Linq;
using Rinzler78.NetExtension.Observable;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.Integration;

/// <summary>
/// Integration tests that verify end-to-end functionality by combining Collections with LINQ extensions.
/// These tests ensure that collection operations work correctly with LINQ extensions and complex data scenarios.
/// </summary>
[Trait("Category", "Integration")]
public class CollectionsLinqIntegrationTests
{
    /// <summary>
    /// Test data class for complex collection operations.
    /// </summary>
    private class TestDataItem
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<string> Tags { get; set; } = new();
        public Dictionary<string, object> Properties { get; set; } = new();
    }

    /// <summary>
    /// Observable test data item for testing observable collections with LINQ.
    /// </summary>
    private class ObservableTestDataItem : ObservableObject
    {
        private int _id;
        private string _name = string.Empty;
        private decimal _value;
        private DateTime _createdAt;
        private List<string> _tags = new();

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public decimal Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public DateTime CreatedAt
        {
            get => _createdAt;
            set => SetProperty(ref _createdAt, value);
        }

        public List<string> Tags
        {
            get => _tags;
            set => SetProperty(ref _tags, value);
        }
    }

    [Fact]
    public void EndToEnd_CollectionWithLinqAndJsonSerialization_ShouldWork()
    {
        // Arrange
        var testData = new List<TestDataItem>
        {
            new() { Id = 1, Name = "Item1", Value = 100.5m, CreatedAt = DateTime.Now.AddDays(-1), Tags = new List<string> { "tag1", "tag2" } },
            new() { Id = 2, Name = "Item2", Value = 200.75m, CreatedAt = DateTime.Now.AddDays(-2), Tags = new List<string> { "tag2", "tag3" } },
            new() { Id = 3, Name = "Item3", Value = 50.25m, CreatedAt = DateTime.Now.AddDays(-3), Tags = new List<string> { "tag1", "tag3" } },
            new() { Id = 4, Name = "Item4", Value = 0m, CreatedAt = DateTime.Now.AddDays(-4), Tags = new List<string>() }
        };

        // Act - Use LINQ operations combined with collections
        var filteredData = testData
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .Take(2)
            .ToList();

        // Use TryAggregate extension
        var totalValue = filteredData.Select(x => x.Value).TryAggregate((x, y) => x + y);

        // Serialize the result
        var json = filteredData.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<List<TestDataItem>>();

        // Assert
        filteredData.Should().HaveCount(2);
        filteredData[0].Name.Should().Be("Item2"); // Highest value
        filteredData[1].Name.Should().Be("Item1"); // Second highest value

        totalValue.Should().Be(301.25m);

        Assert.NotNull(restored);
        Assert.Equal(2, restored.Count);
        restored![0].Name.Should().Be("Item2");
        restored[1].Name.Should().Be("Item1");
    }

    [Fact]
    public void EndToEnd_ObservableCollectionWithLinqOperations_ShouldWork()
    {
        // Arrange
        var observableCollection = new ObservableRangeCollection<ObservableTestDataItem>();
        var collectionEvents = new List<string>();

        observableCollection.CollectionChanged += (sender, e) =>
        {
            collectionEvents.Add($"{e.Action}:{e.NewItems?.Count ?? 0}");
        };

        var testItems = new[]
        {
            new ObservableTestDataItem { Id = 1, Name = "Observable1", Value = 100m, Tags = new List<string> { "obs1", "test" } },
            new ObservableTestDataItem { Id = 2, Name = "Observable2", Value = 200m, Tags = new List<string> { "obs2", "test" } },
            new ObservableTestDataItem { Id = 3, Name = "Observable3", Value = 300m, Tags = new List<string> { "obs3", "prod" } }
        };

        // Act
        observableCollection.AddRange(testItems);

        // Use LINQ operations on observable collection
        var filteredItems = observableCollection
            .Where(x => x.Tags.Contains("test"))
            .OrderBy(x => x.Value)
            .ToList();

        // Use TryAggregate on filtered results
        var averageValue = filteredItems.Select(x => x.Value).TryAggregate((x, y) => (x + y) / 2);

        // Serialize the observable collection
        var json = observableCollection.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<List<ObservableTestDataItem>>();

        // Assert
        collectionEvents.Should().Contain("Add:3");

        filteredItems.Should().HaveCount(2);
        filteredItems[0].Name.Should().Be("Observable1");
        filteredItems[1].Name.Should().Be("Observable2");

        averageValue.Should().Be(150m);

        Assert.NotNull(restored);
        Assert.Equal(3, restored.Count);
        restored![0].Name.Should().Be("Observable1");
    }

    [Fact]
    public void EndToEnd_ComplexDataProcessingPipeline_ShouldWork()
    {
        // Arrange
        var rawData = Enumerable.Range(1, 100).Select(i => new TestDataItem
        {
            Id = i,
            Name = $"Item_{i}",
            Value = i * 10.5m,
            CreatedAt = DateTime.Now.AddDays(-i),
            Tags = new List<string> { $"tag_{i % 3}", $"category_{i % 5}" },
            Properties = new Dictionary<string, object> { { "index", i }, { "isEven", i % 2 == 0 } }
        }).ToList();

        // Act - Complex processing pipeline
        var processedData = rawData
            .Where(x => x.Value > 50) // Filter by value
            .Where(x => x.Tags.Any(tag => tag.StartsWith("tag_"))) // Filter by tag pattern
            .GroupBy(x => x.Properties["isEven"]) // Group by even/odd
            .Select(group => new
            {
                IsEven = (bool)group.Key,
                Count = group.Count(),
                TotalValue = group.Sum(x => x.Value),
                AverageValue = group.Average(x => x.Value),
                Items = group.OrderByDescending(x => x.Value).Take(3).ToList()
            })
            .OrderBy(x => x.IsEven)
            .ToList();

        // Use TryAggregate on the grouped data
        var totalProcessedValue = processedData.Select(x => x.TotalValue).TryAggregate((x, y) => x + y);

        // Serialize the complex result
        var json = processedData.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<JArray>();

        // Assert
        processedData.Should().HaveCount(2); // Even and odd groups
        processedData[0].IsEven.Should().BeFalse(); // Odd group first due to OrderBy
        processedData[1].IsEven.Should().BeTrue(); // Even group second

        totalProcessedValue.Should().BeGreaterThan(0);

        Assert.NotNull(restored);
        json.Should().Contain("IsEven");
        json.Should().Contain("TotalValue");
        json.Should().Contain("AverageValue");
    }

    [Fact]
    public void EndToEnd_CollectionOperationsWithEmptyAndNullData_ShouldHandleGracefully()
    {
        // Arrange
        var mixedData = new List<TestDataItem?>
        {
            new() { Id = 1, Name = "Valid1", Value = 100m },
            null,
            new() { Id = 2, Name = "Valid2", Value = 200m },
            new() { Id = 3, Name = "", Value = 0m }, // Empty name, zero value
            null
        };

        // Act
        var validItems = mixedData
            .Where(x => x != null)
            .Where(x => !string.IsNullOrEmpty(x!.Name))
            .Where(x => x!.Value > 0)
            .ToList();

        // Use TryAggregate on potentially empty collection
        var emptyCollection = new List<decimal>();
        var emptyResult = emptyCollection.TryAggregate((x, y) => x + y);

        // Use TryAggregate on valid collection
        var validResult = validItems.Select(x => x!.Value).TryAggregate((x, y) => x + y);

        // Serialize valid items
        var json = validItems.SerializeObject();
        var restored = json.Deserialize<List<TestDataItem>>();

        // Assert
        validItems.Should().HaveCount(2);
        validItems[0]!.Name.Should().Be("Valid1");
        validItems[1]!.Name.Should().Be("Valid2");

        emptyResult.Should().Be(0); // TryAggregate should return default for empty collection
        validResult.Should().Be(300m);

        Assert.NotNull(restored);
        Assert.Equal(2, restored.Count);
    }

    [Fact]
    public void EndToEnd_NestedCollectionsWithLinqOperations_ShouldWork()
    {
        // Arrange
        var nestedData = new List<TestDataItem>
        {
            new() { Id = 1, Name = "Parent1", Tags = new List<string> { "child1", "child2", "child3" } },
            new() { Id = 2, Name = "Parent2", Tags = new List<string> { "child2", "child4" } },
            new() { Id = 3, Name = "Parent3", Tags = new List<string> { "child1", "child4", "child5" } }
        };

        // Act - Flatten nested collections and perform operations
        var allTags = nestedData
            .SelectMany(x => x.Tags)
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        var tagCounts = nestedData
            .SelectMany(x => x.Tags)
            .GroupBy(tag => tag)
            .Select(group => new { Tag = group.Key, Count = group.Count() })
            .OrderByDescending(x => x.Count)
            .ToList();

        // Use TryAggregate on tag counts
        var totalTagCount = tagCounts.Select(x => x.Count).TryAggregate((x, y) => x + y);

        // Serialize the results
        var tagsJson = allTags.SerializeObject();
        var countsJson = tagCounts.SerializeObject();

        var restoredTags = tagsJson.Deserialize<List<string>>();
        var restoredCounts = countsJson.Deserialize<List<object>>();

        // Assert
        allTags.Should().HaveCount(5);
        allTags.Should().Contain("child1", "child2", "child3", "child4", "child5");
        allTags.Should().BeInAscendingOrder();

        tagCounts.Should().HaveCount(5);
        tagCounts[0].Count.Should().Be(2); // Most frequent tags appear first

        totalTagCount.Should().Be(8); // Total of all tag counts

        restoredTags.Should().NotBeNull();
        restoredTags.Should().HaveCount(5);
        restoredCounts.Should().NotBeNull();
        restoredCounts.Should().HaveCount(5);
    }

    [Fact]
    public void EndToEnd_PerformanceBenchmarkWithLargeCollections_ShouldPerformWell()
    {
        // Arrange
        var largeDataset = Enumerable.Range(1, 10000).Select(i => new TestDataItem
        {
            Id = i,
            Name = $"Item_{i}",
            Value = i * 1.5m,
            CreatedAt = DateTime.Now.AddMinutes(-i),
            Tags = new List<string> { $"tag_{i % 10}", $"category_{i % 5}" }
        }).ToList();

        // Act - Performance test with complex operations
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        var processedData = largeDataset
            .Where(x => x.Value > 5000)
            .GroupBy(x => x.Tags[0])
            .Select(group => new
            {
                TagGroup = group.Key,
                Count = group.Count(),
                AverageValue = group.Average(x => x.Value)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        var processingTime = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var json = processedData.SerializeObject();
        var serializationTime = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var restored = json.Deserialize<List<object>>();
        var deserializationTime = stopwatch.ElapsedMilliseconds;

        stopwatch.Stop();

        // Assert
        processedData.Should().HaveCount(10); // 10 different tag groups
        processedData.Should().BeInDescendingOrder(x => x.Count);

        // Performance assertions (reasonable thresholds for 10K items)
        processingTime.Should().BeLessThan(1000); // Processing should be under 1 second
        serializationTime.Should().BeLessThan(500); // Serialization should be under 0.5 seconds
        deserializationTime.Should().BeLessThan(500); // Deserialization should be under 0.5 seconds

        Assert.NotNull(restored);
        Assert.Equal(10, restored.Count);
    }

    [Fact]
    public void EndToEnd_ErrorHandlingInCollectionOperations_ShouldHandleGracefully()
    {
        // Arrange
        var dataWithErrors = new List<TestDataItem>
        {
            new() { Id = 1, Name = "Valid", Value = 100m },
            new() { Id = 2, Name = null!, Value = 200m }, // Null name
            new() { Id = 3, Name = "Valid2", Value = -50m } // Negative value
        };

        // Act - Operations that might fail
        var safeOperations = dataWithErrors
            .Where(x => x.Name != null) // Filter null names
            .Where(x => x.Value > 0) // Filter negative values
            .ToList();

        // TryAggregate with exception handling
        var values = new List<decimal> { 100m, 200m, 300m };
        var aggregateResult = values.TryAggregate((x, y) => x + y);

        // TryAggregate with empty collection (should not throw)
        var emptyValues = new List<decimal>();
        var emptyAggregateResult = emptyValues.TryAggregate((x, y) => x + y);

        // Serialize and deserialize with error handling
        var json = safeOperations.SerializeObject();
        var restored = json.Deserialize<List<TestDataItem>>();

        // Assert
        safeOperations.Should().HaveCount(1);
        safeOperations[0].Name.Should().Be("Valid");
        safeOperations[0].Value.Should().Be(100m);

        aggregateResult.Should().Be(600m);
        emptyAggregateResult.Should().Be(0m); // Default value for empty collection

        Assert.NotNull(restored);
        Assert.Single(restored!);
        restored![0].Name.Should().Be("Valid");
    }
}
