using System.ComponentModel;
using System.Numerics;
using Newtonsoft.Json;
using Rinzler78.NetExtension.Json;
using Rinzler78.NetExtension.Observable;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.Integration;

/// <summary>
/// Integration tests that verify end-to-end functionality by combining JSON serialization with Observable objects.
/// These tests ensure that the JSON extensions work correctly with observable objects and their property change notifications.
/// </summary>
[Trait("Category", "Integration")]
public class JsonObservableIntegrationTests
{
    /// <summary>
    /// Test observable object that includes complex properties for comprehensive testing.
    /// </summary>
    private class ComplexObservableObject : ObservableObject
    {
        private string _name = string.Empty;
        private int _value;
        private bool _isActive;
        private BigInteger _bigNumber;
        private DateTime _timestamp;
        private List<string> _items = new();
        private Dictionary<string, object> _metadata = new();

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public BigInteger BigNumber
        {
            get => _bigNumber;
            set => SetProperty(ref _bigNumber, value);
        }

        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        public List<string> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        public Dictionary<string, object> Metadata
        {
            get => _metadata;
            set => SetProperty(ref _metadata, value);
        }

        public string DisplayName => $"{Name} ({Value})";
    }

    [Fact]
    public void EndToEnd_JsonSerializationWithObservableObjects_ShouldWork()
    {
        // Arrange
        var viewModel = new ComplexObservableObject();
        var eventsFired = new List<string>();

        viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != null)
                eventsFired.Add(e.PropertyName);
        };

        // Act
        viewModel.Name = "Test Observable";
        viewModel.Value = 42;
        viewModel.IsActive = true;
        viewModel.BigNumber = new BigInteger(123456789012345);
        viewModel.Timestamp = new DateTime(2023, 12, 25, 10, 30, 0);
        viewModel.Items = new List<string> { "Item1", "Item2", "Item3" };
        viewModel.Metadata = new Dictionary<string, object>
        {
            { "category", "test" },
            { "priority", 5 }
        };

        var json = viewModel.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<ComplexObservableObject>();

        // Assert
        restored.Should().NotBeNull();
        restored!.Name.Should().Be("Test Observable");
        restored.Value.Should().Be(42);
        restored.IsActive.Should().BeTrue();
        restored.BigNumber.Should().Be(new BigInteger(123456789012345));
        restored.Timestamp.Should().Be(new DateTime(2023, 12, 25, 10, 30, 0));
        restored.Items.Should().BeEquivalentTo(new[] { "Item1", "Item2", "Item3" });
        restored.Metadata.Should().ContainKey("category");
        restored.Metadata.Should().ContainKey("priority");
        restored.DisplayName.Should().Be("Test Observable (42)");

        // Verify property change events were fired
        eventsFired.Should().Contain("Name");
        eventsFired.Should().Contain("Value");
        eventsFired.Should().Contain("IsActive");
        eventsFired.Should().Contain("BigNumber");
        eventsFired.Should().Contain("Timestamp");
        eventsFired.Should().Contain("Items");
        eventsFired.Should().Contain("Metadata");
    }

    [Fact]
    public void EndToEnd_ObservableObjectWithCustomJsonSettings_ShouldWork()
    {
        // Arrange
        var viewModel = new ComplexObservableObject();
        var settings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-dd",
            NullValueHandling = NullValueHandling.Ignore
        };

        // Act
        viewModel.Name = "Custom Settings Test";
        viewModel.Timestamp = new DateTime(2023, 12, 25);
        viewModel.Items = null!; // This should be ignored due to NullValueHandling

        var json = viewModel.SerializeObject(Formatting.Indented, settings);
        var restored = json.Deserialize<ComplexObservableObject>(settings);

        // Assert
        restored.Should().NotBeNull();
        restored!.Name.Should().Be("Custom Settings Test");
        restored.Timestamp.Should().Be(new DateTime(2023, 12, 25));
        json.Should().NotContain("Items"); // Should be ignored due to null value handling
        json.Should().Contain("2023-12-25"); // Should use custom date format
    }

    [Fact]
    public void EndToEnd_ObservableCollectionWithJsonSerialization_ShouldWork()
    {
        // Arrange
        var collection = new ObservableRangeCollection<ComplexObservableObject>();
        var collectionEvents = new List<string>();

        collection.CollectionChanged += (sender, e) =>
        {
            collectionEvents.Add(e.Action.ToString());
        };

        // Act
        var items = new[]
        {
            new ComplexObservableObject { Name = "Item1", Value = 1, IsActive = true },
            new ComplexObservableObject { Name = "Item2", Value = 2, IsActive = false },
            new ComplexObservableObject { Name = "Item3", Value = 3, IsActive = true }
        };

        collection.AddRange(items);

        var json = collection.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<List<ComplexObservableObject>>();

        // Assert
        restored.Should().NotBeNull();
        restored.Should().HaveCount(3);
        restored![0].Name.Should().Be("Item1");
        restored[1].Name.Should().Be("Item2");
        restored[2].Name.Should().Be("Item3");

        // Verify collection events were fired
        collectionEvents.Should().Contain("Add");
    }

    [Fact]
    public async Task EndToEnd_ObservableObjectWithAsyncJsonOperations_ShouldWork()
    {
        // Arrange
        var viewModel = new ComplexObservableObject();
        var tempFile = MockHelpers.CreateTempFile("", ".json");
        var eventCount = 0;

        viewModel.PropertyChanged += (sender, e) => eventCount++;

        try
        {
            // Act
            viewModel.Name = "Async Test";
            viewModel.Value = 999;
            viewModel.BigNumber = new BigInteger(999999999999999);
            viewModel.Items = new List<string> { "AsyncItem1", "AsyncItem2" };

            var json = viewModel.SerializeObject(Formatting.Indented);
            await File.WriteAllTextAsync(tempFile, json);

            // Read back from file
            var restored = tempFile.DeserializeObjectFromFile<ComplexObservableObject>();

            // Assert
            restored.Should().NotBeNull();
            restored!.Name.Should().Be("Async Test");
            restored.Value.Should().Be(999);
            restored.BigNumber.Should().Be(new BigInteger(999999999999999));
            restored.Items.Should().BeEquivalentTo(new[] { "AsyncItem1", "AsyncItem2" });

            // Verify property change events
            eventCount.Should().Be(4); // Name, Value, BigNumber, Items
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void EndToEnd_PropertyChangedWithJsonRoundTrip_ShouldMaintainState()
    {
        // Arrange
        var original = new ComplexObservableObject();
        var propertyChanges = new List<string>();

        // Act - Set initial state
        original.Name = "Original";
        original.Value = 100;
        original.IsActive = true;

        // Serialize to JSON
        var json = original.SerializeObject();

        // Deserialize to new object
        var restored = json.Deserialize<ComplexObservableObject>();

        // Set up property change tracking on restored object
        restored!.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != null)
                propertyChanges.Add(e.PropertyName);
        };

        // Modify restored object
        restored.Name = "Modified";
        restored.Value = 200;
        restored.IsActive = false;

        // Assert
        restored.Name.Should().Be("Modified");
        restored.Value.Should().Be(200);
        restored.IsActive.Should().BeFalse();

        // Verify property change events work on deserialized object
        propertyChanges.Should().Contain("Name");
        propertyChanges.Should().Contain("Value");
        propertyChanges.Should().Contain("IsActive");
    }

    [Fact]
    public void EndToEnd_ComplexNestedObservableStructure_ShouldSerializeCorrectly()
    {
        // Arrange
        var parentObject = new ComplexObservableObject();
        var childObjects = new List<ComplexObservableObject>
        {
            new() { Name = "Child1", Value = 10 },
            new() { Name = "Child2", Value = 20 }
        };

        // Act
        parentObject.Name = "Parent";
        parentObject.Value = 1;
        parentObject.Metadata["children"] = childObjects;

        var json = parentObject.SerializeObject(Formatting.Indented);
        var restored = json.Deserialize<ComplexObservableObject>();

        // Assert
        restored.Should().NotBeNull();
        restored!.Name.Should().Be("Parent");
        restored.Value.Should().Be(1);
        restored.Metadata.Should().ContainKey("children");

        // Verify nested structure
        json.Should().Contain("Child1");
        json.Should().Contain("Child2");
    }

    [Fact]
    public void EndToEnd_ObservableObjectWithLargeData_ShouldHandlePerformance()
    {
        // Arrange
        var viewModel = new ComplexObservableObject();
        var largeItemList = Enumerable.Range(1, 1000).Select(i => $"Item_{i}").ToList();
        var largeMetadata = Enumerable.Range(1, 100).ToDictionary(i => $"key_{i}", i => (object)$"value_{i}");

        // Act
        viewModel.Name = "Performance Test";
        viewModel.Items = largeItemList;
        viewModel.Metadata = largeMetadata;

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        var json = viewModel.SerializeObject();
        var serializationTime = stopwatch.ElapsedMilliseconds;

        stopwatch.Restart();
        var restored = json.Deserialize<ComplexObservableObject>();
        var deserializationTime = stopwatch.ElapsedMilliseconds;
        stopwatch.Stop();

        // Assert
        restored.Should().NotBeNull();
        restored!.Name.Should().Be("Performance Test");
        restored.Items.Should().HaveCount(1000);
        restored.Metadata.Should().HaveCount(100);

        // Performance assertions (reasonable thresholds)
        serializationTime.Should().BeLessThan(1000); // Less than 1 second
        deserializationTime.Should().BeLessThan(1000); // Less than 1 second
    }

    [Fact]
    public void EndToEnd_ObservableObjectErrorHandling_ShouldHandleInvalidJson()
    {
        // Arrange
        var malformedJson = "{\"Name\":\"Test\",\"Value\":";
        var emptyJson = "";
        var nullJson = (string?)null;

        // Act & Assert - Should handle gracefully
        var act1 = () => malformedJson.Deserialize<ComplexObservableObject>();
        act1.Should().Throw<Newtonsoft.Json.JsonException>(); // Invalid JSON should throw

        var emptyResult = emptyJson.Deserialize<ComplexObservableObject>();
        emptyResult.Should().BeNull(); // Empty JSON should return null

        var nullResult = nullJson.Deserialize<ComplexObservableObject>();
        nullResult.Should().BeNull(); // Null JSON should return null
    }
}
