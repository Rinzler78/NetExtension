using System.Numerics;
using System.Text;
using Newtonsoft.Json;
using Rinzler78.NetExtension.Json;
using Rinzler78.NetExtension.Observable;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.Integration;

/// <summary>
/// Integration tests that verify end-to-end functionality for file-based JSON operations.
/// These tests ensure that JSON serialization and deserialization work correctly with file I/O operations.
/// </summary>
[Trait("Category", "Integration")]
public class FileBasedJsonIntegrationTests
{
    /// <summary>
    /// Complex data structure for testing file-based JSON operations.
    /// </summary>
    private class ComplexDataStructure
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public BigInteger LargeNumber { get; set; }
        public List<NestedItem> Items { get; set; } = new();
        public Dictionary<string, object> Metadata { get; set; } = new();
        public NestedConfiguration Configuration { get; set; } = new();
    }

    /// <summary>
    /// Nested item class for testing complex structures.
    /// </summary>
    private class NestedItem
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = string.Empty;
        public double Value { get; set; }
        public bool IsActive { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    /// <summary>
    /// Configuration class for testing nested objects.
    /// </summary>
    private class NestedConfiguration
    {
        public string Environment { get; set; } = "Test";
        public int MaxRetries { get; set; } = 3;
        public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);
        public Dictionary<string, string> Settings { get; set; } = new();
    }

    /// <summary>
    /// Observable data structure for testing file operations with observable objects.
    /// </summary>
    private class ObservableDataStructure : ObservableObject
    {
        private string _name = string.Empty;
        private int _value;
        private DateTime _lastModified;
        private List<string> _items = new();

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

        public DateTime LastModified
        {
            get => _lastModified;
            set => SetProperty(ref _lastModified, value);
        }

        public List<string> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }
    }

    [Fact]
    public void EndToEnd_ComplexDataStructureFileOperations_ShouldWork()
    {
        // Arrange
        var complexData = new ComplexDataStructure
        {
            Id = 1,
            Name = "Test Data Structure",
            CreatedAt = DateTime.Now,
            Amount = 1234.56m,
            LargeNumber = new BigInteger(123456789012345678),
            Items = new List<NestedItem>
            {
                new() { ItemId = 1, ItemName = "Item 1", Value = 10.5, IsActive = true, Tags = new List<string> { "tag1", "tag2" } },
                new() { ItemId = 2, ItemName = "Item 2", Value = 20.5, IsActive = false, Tags = new List<string> { "tag3", "tag4" } }
            },
            Metadata = new Dictionary<string, object>
            {
                { "version", "1.0" },
                { "author", "Test Author" },
                { "isProduction", false }
            },
            Configuration = new NestedConfiguration
            {
                Environment = "Development",
                MaxRetries = 5,
                Timeout = TimeSpan.FromMinutes(2),
                Settings = new Dictionary<string, string>
                {
                    { "database", "test_db" },
                    { "apiKey", "test_key" }
                }
            }
        };

        var tempFile = MockHelpers.CreateTempFile("", ".json");

        try
        {
            // Act - Write complex data to file
            var json = complexData.SerializeObject(Formatting.Indented);
            File.WriteAllText(tempFile, json);

            // Read back from file
            var restored = tempFile.DeserializeObjectFromFile<ComplexDataStructure>();

            // Assert
            restored.Should().NotBeNull();
            restored!.Id.Should().Be(complexData.Id);
            restored.Name.Should().Be(complexData.Name);
            restored.Amount.Should().Be(complexData.Amount);
            restored.LargeNumber.Should().Be(complexData.LargeNumber);

            restored.Items.Should().HaveCount(2);
            restored.Items[0].ItemName.Should().Be("Item 1");
            restored.Items[1].ItemName.Should().Be("Item 2");

            restored.Metadata.Should().ContainKey("version");
            restored.Metadata["version"].Should().Be("1.0");

            restored.Configuration.Environment.Should().Be("Development");
            restored.Configuration.MaxRetries.Should().Be(5);
            restored.Configuration.Settings.Should().ContainKey("database");
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void EndToEnd_ObservableObjectFileOperations_ShouldWork()
    {
        // Arrange
        var observableData = new ObservableDataStructure();
        var propertyChanges = new List<string>();

        observableData.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName != null)
                propertyChanges.Add(e.PropertyName);
        };

        var tempFile = MockHelpers.CreateTempFile("", ".json");

        try
        {
            // Act - Set properties and save to file
            observableData.Name = "Observable Test";
            observableData.Value = 42;
            observableData.LastModified = DateTime.Now;
            observableData.Items = new List<string> { "Item1", "Item2", "Item3" };

            var json = observableData.SerializeObject(Formatting.Indented);
            File.WriteAllText(tempFile, json);

            // Read back from file
            var restored = tempFile.DeserializeObjectFromFile<ObservableDataStructure>();

            // Set up property change tracking on restored object
            var restoredPropertyChanges = new List<string>();
            restored!.PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName != null)
                    restoredPropertyChanges.Add(e.PropertyName);
            };

            // Modify restored object
            restored.Name = "Modified Observable";
            restored.Value = 100;

            // Assert
            restored.Should().NotBeNull();
            restored!.Name.Should().Be("Modified Observable");
            restored.Value.Should().Be(100);
            restored.Items.Should().BeEquivalentTo(new[] { "Item1", "Item2", "Item3" });

            // Verify property changes worked on both objects
            propertyChanges.Should().Contain("Name");
            propertyChanges.Should().Contain("Value");
            propertyChanges.Should().Contain("LastModified");
            propertyChanges.Should().Contain("Items");

            restoredPropertyChanges.Should().Contain("Name");
            restoredPropertyChanges.Should().Contain("Value");
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void EndToEnd_MultipleFileOperationsWithBatching_ShouldWork()
    {
        // Arrange
        var dataItems = Enumerable.Range(1, 10).Select(i => new ComplexDataStructure
        {
            Id = i,
            Name = $"Data Item {i}",
            CreatedAt = DateTime.Now.AddDays(-i),
            Amount = i * 100.5m,
            LargeNumber = new BigInteger(i * 1000000),
            Items = new List<NestedItem>
            {
                new() { ItemId = i * 10, ItemName = $"Nested Item {i}", Value = i * 5.5, IsActive = i % 2 == 0 }
            },
            Metadata = new Dictionary<string, object>
            {
                { "index", i },
                { "isEven", i % 2 == 0 }
            }
        }).ToList();

        var tempFiles = new List<string>();

        try
        {
            // Act - Write multiple files
            foreach (var item in dataItems)
            {
                var tempFile = MockHelpers.CreateTempFile("", ".json");
                tempFiles.Add(tempFile);

                var json = item.SerializeObject(Formatting.Indented);
                File.WriteAllText(tempFile, json);
            }

            // Read all files back
            var restoredItems = tempFiles
                .Select(file => file.DeserializeObjectFromFile<ComplexDataStructure>())
                .ToList();

            // Process the restored data
            var processedItems = restoredItems
                .Where(item => item != null)
                .OrderBy(item => item!.Id)
                .Select(item => new
                {
                    Id = item!.Id,
                    Name = item.Name,
                    TotalValue = item.Amount + (decimal)item.Items.Sum(nested => nested.Value),
                    ItemCount = item.Items.Count,
                    IsEven = (bool)item.Metadata["isEven"]
                })
                .ToList();

            // Assert
            restoredItems.Should().HaveCount(10);
            restoredItems.Should().NotContain(item => item == null);

            processedItems.Should().HaveCount(10);
            processedItems.Should().BeInAscendingOrder(item => item.Id);

            processedItems[0].Id.Should().Be(1);
            processedItems[0].Name.Should().Be("Data Item 1");
            processedItems[0].IsEven.Should().BeFalse();

            processedItems[1].Id.Should().Be(2);
            processedItems[1].IsEven.Should().BeTrue();
        }
        finally
        {
            foreach (var file in tempFiles)
            {
                MockHelpers.CleanupTempFile(file);
            }
        }
    }

    [Fact]
    public void EndToEnd_FileOperationsWithCustomJsonSettings_ShouldWork()
    {
        // Arrange
        var data = new ComplexDataStructure
        {
            Id = 1,
            Name = "Custom Settings Test",
            CreatedAt = new DateTime(2023, 12, 25, 10, 30, 0),
            Amount = 1234.5600m, // Test decimal precision
            LargeNumber = BigInteger.Parse("999999999999999999999999999999"),
            Items = new List<NestedItem>
            {
                new() { ItemId = 1, ItemName = "Item 1", Value = 10.123456789 }
            }
        };

        var settings = new JsonSerializerSettings
        {
            DateFormatString = "yyyy-MM-dd HH:mm:ss",
            FloatFormatHandling = FloatFormatHandling.String,
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented
        };

        var tempFile = MockHelpers.CreateTempFile("", ".json");

        try
        {
            // Act - Write with custom settings
            var json = data.SerializeObject(Formatting.Indented, settings);
            File.WriteAllText(tempFile, json);

            // Read back with custom settings
            var restored = tempFile.DeserializeObjectFromFile<ComplexDataStructure>(settings);

            // Also test without custom settings to see the difference
            var jsonWithoutSettings = data.SerializeObject(Formatting.Indented);

            // Assert
            restored.Should().NotBeNull();
            restored!.Id.Should().Be(data.Id);
            restored.Name.Should().Be(data.Name);
            restored.CreatedAt.Should().Be(data.CreatedAt);
            restored.Amount.Should().Be(data.Amount);
            restored.LargeNumber.Should().Be(data.LargeNumber);

            // Verify custom date format is used
            json.Should().Contain("2023-12-25 10:30:00");
            json.Should().NotContain("2023-12-25T10:30:00");

            // Verify that different settings produce different JSON
            jsonWithoutSettings.Should().NotBe(json);
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public async Task EndToEnd_ConcurrentFileOperations_ShouldWork()
    {
        // Arrange
        var dataItems = Enumerable.Range(1, 20).Select(i => new ComplexDataStructure
        {
            Id = i,
            Name = $"Concurrent Item {i}",
            CreatedAt = DateTime.Now,
            Amount = i * 50.5m,
            Items = new List<NestedItem>
            {
                new() { ItemId = i, ItemName = $"Nested {i}", Value = i * 2.5 }
            }
        }).ToList();

        var tempFiles = new List<string>();

        try
        {
            // Act - Write files concurrently
            var writeTasks = dataItems.Select(async item =>
            {
                var tempFile = MockHelpers.CreateTempFile("", ".json");
                tempFiles.Add(tempFile);

                var json = item.SerializeObject(Formatting.Indented);
                await File.WriteAllTextAsync(tempFile, json);

                return tempFile;
            });

            var fileResults = await Task.WhenAll(writeTasks);

            // Read files concurrently
            var readTasks = fileResults.Select(async file =>
            {
                await Task.Delay(10); // Small delay to simulate real-world scenario
                return file.DeserializeObjectFromFile<ComplexDataStructure>();
            });

            var readResults = await Task.WhenAll(readTasks);

            // Assert
            readResults.Should().HaveCount(20);
            readResults.Should().NotContain(item => item == null);

            var sortedResults = readResults.OrderBy(item => item!.Id).ToList();

            for (int i = 0; i < sortedResults.Count; i++)
            {
                sortedResults[i]!.Id.Should().Be(i + 1);
                sortedResults[i]!.Name.Should().Be($"Concurrent Item {i + 1}");
                sortedResults[i]!.Amount.Should().Be((i + 1) * 50.5m);
            }
        }
        finally
        {
            foreach (var file in tempFiles)
            {
                MockHelpers.CleanupTempFile(file);
            }
        }
    }

    [Fact]
    public void EndToEnd_LargeFileOperations_ShouldHandlePerformance()
    {
        // Arrange
        var largeDataStructure = new ComplexDataStructure
        {
            Id = 1,
            Name = "Large Data Test",
            CreatedAt = DateTime.Now,
            Amount = 999999.99m,
            LargeNumber = BigInteger.Parse("123456789012345678901234567890123456789012345678901234567890"),
            Items = Enumerable.Range(1, 1000).Select(i => new NestedItem
            {
                ItemId = i,
                ItemName = $"Large Item {i}",
                Value = i * 1.5,
                IsActive = i % 2 == 0,
                Tags = new List<string> { $"tag_{i}", $"category_{i % 10}" }
            }).ToList(),
            Metadata = Enumerable.Range(1, 100).ToDictionary(i => $"key_{i}", i => (object)$"value_{i}")
        };

        var tempFile = MockHelpers.CreateTempFile("", ".json");

        try
        {
            // Act - Performance test
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            var json = largeDataStructure.SerializeObject(Formatting.Indented);
            var serializationTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            File.WriteAllText(tempFile, json);
            var writeTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Restart();
            var restored = tempFile.DeserializeObjectFromFile<ComplexDataStructure>();
            var readTime = stopwatch.ElapsedMilliseconds;

            stopwatch.Stop();

            // Verify file size
            var fileInfo = new FileInfo(tempFile);
            var fileSizeKB = fileInfo.Length / 1024;

            // Assert
            restored.Should().NotBeNull();
            restored!.Id.Should().Be(largeDataStructure.Id);
            restored.Items.Should().HaveCount(1000);
            restored.Metadata.Should().HaveCount(100);

            // Performance assertions (reasonable thresholds)
            serializationTime.Should().BeLessThan(2000); // Less than 2 seconds
            writeTime.Should().BeLessThan(1000); // Less than 1 second
            readTime.Should().BeLessThan(2000); // Less than 2 seconds

            fileSizeKB.Should().BeGreaterThan(0);
            fileSizeKB.Should().BeLessThan(5000); // Less than 5MB for reasonable test data
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void EndToEnd_FileOperationsErrorHandling_ShouldHandleGracefully()
    {
        // Arrange
        var validData = new ComplexDataStructure { Id = 1, Name = "Valid Data" };
        var tempFile = MockHelpers.CreateTempFile("", ".json");
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        try
        {
            // Act & Assert - Test various error scenarios

            // 1. Valid file operations should work
            var json = validData.SerializeObject();
            File.WriteAllText(tempFile, json);
            var restored = tempFile.DeserializeObjectFromFile<ComplexDataStructure>();
            restored.Should().NotBeNull();

            // 2. Non-existent file should throw
            var act1 = () => nonExistentFile.DeserializeObjectFromFile<ComplexDataStructure>();
            act1.Should().Throw<FileNotFoundException>();

            // 3. Empty file should return null
            var emptyFile = MockHelpers.CreateTempFile("", ".json");
            var emptyResult = emptyFile.DeserializeObjectFromFile<ComplexDataStructure>();
            emptyResult.Should().BeNull();

            // 4. Malformed JSON file should throw
            var malformedFile = MockHelpers.CreateTempFile("{\"id\":1,\"name\":\"test\",\"invalid\":}", ".json");
            var act2 = () => malformedFile.DeserializeObjectFromFile<ComplexDataStructure>();
            act2.Should().Throw<JsonException>();

            // 5. File with invalid path characters should throw
            var invalidPath = "invalid\0path";
            var act3 = () => invalidPath.DeserializeObjectFromFile<ComplexDataStructure>();
            act3.Should().Throw<ArgumentException>();

            // 6. Test path traversal protection
            var maliciousPath = "../../../etc/passwd";
            var act4 = () => maliciousPath.DeserializeObjectFromFile<ComplexDataStructure>();
            act4.Should().Throw<ArgumentException>();

            // Cleanup additional temp files
            MockHelpers.CleanupTempFile(emptyFile);
            MockHelpers.CleanupTempFile(malformedFile);
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void EndToEnd_ConfigurationFileManagement_ShouldWork()
    {
        // Arrange
        var configurations = new List<NestedConfiguration>
        {
            new()
            {
                Environment = "Development",
                MaxRetries = 3,
                Timeout = TimeSpan.FromSeconds(30),
                Settings = new Dictionary<string, string>
                {
                    { "database", "dev_db" },
                    { "logLevel", "Debug" }
                }
            },
            new()
            {
                Environment = "Production",
                MaxRetries = 5,
                Timeout = TimeSpan.FromMinutes(1),
                Settings = new Dictionary<string, string>
                {
                    { "database", "prod_db" },
                    { "logLevel", "Info" }
                }
            }
        };

        var tempFiles = new List<string>();

        try
        {
            // Act - Save configurations to separate files
            foreach (var config in configurations)
            {
                var tempFile = MockHelpers.CreateTempFile("", ".json");
                tempFiles.Add(tempFile);

                var json = config.SerializeObject(Formatting.Indented);
                File.WriteAllText(tempFile, json);
            }

            // Load configurations back
            var loadedConfigs = tempFiles
                .Select(file => file.DeserializeObjectFromFile<NestedConfiguration>())
                .ToList();

            // Process configuration data
            var configSummary = loadedConfigs
                .Select(config => new
                {
                    Environment = config!.Environment,
                    MaxRetries = config.MaxRetries,
                    TimeoutSeconds = config.Timeout.TotalSeconds,
                    SettingsCount = config.Settings.Count,
                    HasDatabase = config.Settings.ContainsKey("database")
                })
                .ToList();

            // Serialize summary
            var summaryJson = configSummary.SerializeObject(Formatting.Indented);

            // Assert
            loadedConfigs.Should().HaveCount(2);
            loadedConfigs.Should().NotContain(config => config == null);

            loadedConfigs[0]!.Environment.Should().Be("Development");
            loadedConfigs[0]!.MaxRetries.Should().Be(3);
            loadedConfigs[0]!.Settings["database"].Should().Be("dev_db");

            loadedConfigs[1]!.Environment.Should().Be("Production");
            loadedConfigs[1]!.MaxRetries.Should().Be(5);
            loadedConfigs[1]!.Settings["database"].Should().Be("prod_db");

            configSummary.Should().HaveCount(2);
            configSummary[0].HasDatabase.Should().BeTrue();
            configSummary[1].HasDatabase.Should().BeTrue();

            summaryJson.Should().Contain("Development");
            summaryJson.Should().Contain("Production");
        }
        finally
        {
            foreach (var file in tempFiles)
            {
                MockHelpers.CleanupTempFile(file);
            }
        }
    }
}
