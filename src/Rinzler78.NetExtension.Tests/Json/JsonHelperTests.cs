using System.Numerics;
using Newtonsoft.Json;
using Rinzler78.NetExtension.Json;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.Json;

[Trait("Category", "Unit")]
public class JsonHelperTests
{
    [Fact]
    public void SerializeObject_ShouldSerializeSimpleObject()
    {
        // Arrange
        var obj = TestData.TestObjects.SimpleRecordInstance;

        // Act
        var json = obj.SerializeObject();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"Name\":\"Test\"");
        json.Should().Contain("\"Value\":42");
    }

    [Fact]
    public void SerializeObject_WithFormattingAndSettings_ShouldHonorConfiguration()
    {
        var obj = new { Name = "Test", Optional = (string?)null };
        var settings = new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore };

        var json = obj.SerializeObject(Formatting.Indented, settings);

        json.Should().Contain("\n");
        json.Should().Contain("Name");
        json.Should().NotContain("Optional");
    }

    [Fact]
    public void SerializeObject_WithFormattingOnly_ShouldApplyFormatting()
    {
        var obj = new { Name = "Test", Value = 42 };

        var indented = obj.SerializeObject(Formatting.Indented);
        var compact = obj.SerializeObject(Formatting.None);

        indented.Should().Contain("\n", "Indented formatting adds newlines");
        compact.Should().NotContain("\n", "None formatting produces single-line JSON");
        indented.Should().Contain("Test");
        compact.Should().Contain("Test");
    }

    [Fact]
    public void DeserializeObject_ShouldDeserializeDynamicObject()
    {
        var result = "{\"Name\":\"Test\"}".DeserializeObject();

        result.Should().NotBeNull();
    }

    [Fact]
    public void DeSerialize_ArrayOverloads_ShouldDeserializeValues()
    {
        var values = new[] { "{\"Name\":\"One\",\"Value\":1}", "{\"Name\":\"Two\",\"Value\":2}" };

        var result = values.Deserialize<TestData.TestObjects.SimpleRecord>();

        result.Should().HaveCount(2);
        result[0]!.Name.Should().Be("One");
        result[1]!.Value.Should().Be(2);
    }

    [Fact]
    public void DeSerialize_WithSettingsOverloads_ShouldDeserializeValues()
    {
        var settings = new JsonSerializerSettings();
        var json = "{\"Name\":\"Test\",\"Value\":42}";
        var values = new[] { json };

        json.Deserialize<TestData.TestObjects.SimpleRecord>(settings)!.Value.Should().Be(42);
        values.Deserialize<TestData.TestObjects.SimpleRecord>(settings).Should().ContainSingle();
    }

    [Fact]
    public void SerializeObject_ShouldSerializeComplexObject()
    {
        // Arrange
        var obj = TestData.TestObjects.ComplexRecordInstance;

        // Act
        var json = obj.SerializeObject();

        // Assert
        json.Should().NotBeNullOrEmpty();
        json.Should().Contain("\"Name\":\"Complex\"");
        json.Should().Contain("\"Value\":100");
        json.Should().Contain("\"Items\":");
        json.Should().Contain("\"Properties\":");
    }

    [Fact]
    public void SerializeObject_ShouldHandleNullObject()
    {
        // Arrange
        object? obj = null;

        // Act
        var json = obj!.SerializeObject();

        // Assert
        json.Should().Be("null");
    }

    [Fact]
    public void SerializeObject_ShouldHandleEmptyString()
    {
        // Arrange
        var obj = TestData.Strings.Empty;

        // Act
        var json = obj.SerializeObject();

        // Assert
        json.Should().Be("\"\"");
    }

    [Fact]
    public void DeSerialize_ShouldDeserializeSimpleObject()
    {
        // Arrange
        var json = "{\"Name\":\"Test\",\"Value\":42}";

        // Act
        var obj = json.Deserialize<TestData.TestObjects.SimpleRecord>();

        // Assert
        obj.Should().NotBeNull();
        obj!.Name.Should().Be("Test");
        obj.Value.Should().Be(42);
    }

    [Fact]
    public void DeSerialize_ShouldDeserializeComplexObject()
    {
        // Arrange
        var original = TestData.TestObjects.ComplexRecordInstance;
        var json = original.SerializeObject();

        // Act
        var obj = json.Deserialize<TestData.TestObjects.ComplexRecord>();

        // Assert
        obj.Should().NotBeNull();
        obj!.Name.Should().Be(original.Name);
        obj.Value.Should().Be(original.Value);
        obj.Items.Should().BeEquivalentTo(original.Items);
        obj.Properties.Should().BeEquivalentTo(original.Properties);
    }

    [Fact]
    public void DeSerialize_ShouldHandleNullJson()
    {
        // Arrange
        string? json = null;

        // Act
        var obj = json!.Deserialize<TestData.TestObjects.SimpleRecord>();

        // Assert
        obj.Should().BeNull();
    }

    [Fact]
    public void DeSerialize_ShouldHandleEmptyJson()
    {
        // Arrange
        var json = string.Empty;

        // Act
        var obj = json.Deserialize<TestData.TestObjects.SimpleRecord>();

        // Assert
        obj.Should().BeNull();
    }

    [Fact]
    public void DeSerialize_ShouldThrowOnMalformedJson()
    {
        // Arrange
        var json = TestData.Strings.MalformedJson;

        // Act & Assert
        var act = () => json.Deserialize<TestData.TestObjects.SimpleRecord>();
        act.Should().Throw<Newtonsoft.Json.JsonSerializationException>();
    }

    [Fact]
    public void SerializeObjectWithoutQuote_ShouldRemoveQuotes()
    {
        // Arrange
        var str = "hello world";

        // Act
        var result = str.SerializeObjectWithoutQuote();

        // Assert
        result.Should().Be("hello world");
        result.Should().NotContain("\"");
    }

    [Fact]
    public void SerializeObjectWithoutQuote_ShouldHandleNullString()
    {
        // Arrange
        string? str = null;

        // Act
        var result = str!.SerializeObjectWithoutQuote();

        // Assert
        result.Should().Be("null");
    }

    [Fact]
    public void SerializeObjectWithoutQuote_ShouldHandleComplexObject()
    {
        // Arrange
        var obj = TestData.TestObjects.SimpleRecordInstance;

        // Act
        var result = obj.SerializeObjectWithoutQuote();

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().NotStartWith("\"");
        result.Should().NotEndWith("\"");
        result.Should().Contain("Name");
        result.Should().Contain("Value");
    }

    [Fact]
    public void DeserializeObjectFromFile_ShouldDeserializeFromValidFile()
    {
        // Arrange
        var obj = TestData.TestObjects.SimpleRecordInstance;
        var json = obj.SerializeObject();
        var tempFile = MockHelpers.CreateTempFile(json, ".json");

        try
        {
            // Act
            var result = tempFile.DeserializeObjectFromFile<TestData.TestObjects.SimpleRecord>();

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(obj.Name);
            result.Value.Should().Be(obj.Value);
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void DeserializeObjectFromFile_ShouldThrowOnNonExistentFile()
    {
        // Arrange
        var nonExistentFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.json");

        // Act & Assert
        var act = () => nonExistentFile.DeserializeObjectFromFile<TestData.TestObjects.SimpleRecord>();
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void DeserializeObjectFromFile_ShouldHandleEmptyFile()
    {
        // Arrange
        var tempFile = MockHelpers.CreateTempFile(string.Empty, ".json");

        try
        {
            // Act
            var result = tempFile.DeserializeObjectFromFile<TestData.TestObjects.SimpleRecord>();

            // Assert
            result.Should().BeNull();
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void DeserializeObjectFromFile_ShouldHandleMalformedJsonFile()
    {
        // Arrange
        var tempFile = MockHelpers.CreateTempFile(TestData.Strings.MalformedJson, ".json");

        try
        {
            // Act & Assert
            var act = () => tempFile.DeserializeObjectFromFile<TestData.TestObjects.SimpleRecord>();
            act.Should().Throw<Newtonsoft.Json.JsonSerializationException>();
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void DeserializeObjectFromFile_NonGeneric_ShouldReturnObjectOrNull()
    {
        var tempFile = MockHelpers.CreateTempFile("{\"Name\":\"Test\"}", ".json");
        var emptyFile = MockHelpers.CreateTempFile(string.Empty, ".json");
        var missingFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        try
        {
            tempFile.DeserializeObjectFromFile().Should().NotBeNull();
            emptyFile.DeserializeObjectFromFile().Should().BeNull();
            missingFile.DeserializeObjectFromFile().Should().BeNull();
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
            MockHelpers.CleanupTempFile(emptyFile);
        }
    }

    [Fact]
    public void DeserializeObjectFromFile_WithSettings_ShouldDeserializeUsingSettings()
    {
        var tempFile = MockHelpers.CreateTempFile("{\"When\":\"2024-01-02\"}", ".json");
        var settings = new JsonSerializerSettings { DateFormatString = "yyyy-MM-dd" };

        try
        {
            var result = tempFile.DeserializeObjectFromFile<SettingsRecord>(settings);

            result.When.Should().Be(new DateTime(2024, 1, 2));
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void DeserializeObjectFromFile_WithSettingsAndMissingFile_ShouldThrow()
    {
        var settings = new JsonSerializerSettings();
        var missingFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".json");

        Action act = () => missingFile.DeserializeObjectFromFile<SettingsRecord>(settings);

        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void DeserializeObjectFromFile_WithExecutableExtension_ShouldThrowArgumentException()
    {
        var executablePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".exe");

        Action act = () => executablePath.DeserializeObjectFromFile<SettingsRecord>();

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Serialization_ShouldBeReversible()
    {
        // Arrange
        var original = TestData.TestObjects.ComplexRecordInstance;

        // Act
        var json = original.SerializeObject();
        var deserialized = json.Deserialize<TestData.TestObjects.ComplexRecord>();

        // Assert
        deserialized.Should().BeEquivalentTo(original);
    }

    [Fact]
    public void Serialization_ShouldHandleSpecialCharacters()
    {
        // Arrange
        var obj = new TestData.TestObjects.SimpleRecord(TestData.Strings.Unicode, 42);

        // Act
        var json = obj.SerializeObject();
        var deserialized = json.Deserialize<TestData.TestObjects.SimpleRecord>();

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Name.Should().Be(TestData.Strings.Unicode);
    }

    [Fact]
    public void Serialization_ShouldHandleCollections()
    {
        // Arrange
        var collection = TestData.Collections.IntList;

        // Act
        var json = collection.SerializeObject();
        var deserialized = json.Deserialize<List<int>>();

        // Assert
        deserialized.Should().BeEquivalentTo(collection);
    }

    [Fact]
    public void Serialization_ShouldHandleDictionaries()
    {
        // Arrange
        var dictionary = TestData.Collections.StringIntDict;

        // Act
        var json = dictionary.SerializeObject();
        var deserialized = json.Deserialize<Dictionary<string, int>>();

        // Assert
        deserialized.Should().BeEquivalentTo(dictionary);
    }

    [Fact]
    public void Serialization_ShouldHandleBigInteger()
    {
        // Arrange
        var bigInt = TestData.Numbers.BigIntegerLarge;

        // Act
        var json = bigInt.SerializeObject();
        var deserialized = json.Deserialize<BigInteger>();

        // Assert
        deserialized.Should().Be(bigInt);
    }

    [Fact]
    public void Serialization_ShouldHandleDateTime()
    {
        // Arrange
        var dateTime = DateTime.Now;

        // Act
        var json = dateTime.SerializeObject();
        var deserialized = json.Deserialize<DateTime>();

        // Assert
        deserialized.Should().BeCloseTo(dateTime, TimeSpan.FromMilliseconds(1));
    }

    [Fact]
    public void Serialization_ShouldHandleNestedObjects()
    {
        // Arrange
        var nested = new
        {
            Level1 = new
            {
                Level2 = new
                {
                    Value = "deep nested value"
                }
            }
        };

        // Act
        var json = nested.SerializeObject();
        var deserialized = json.Deserialize<object>();

        // Assert
        deserialized.Should().NotBeNull();
        json.Should().Contain("deep nested value");
    }

    [Fact]
    public void FileOperations_ShouldHandleUnicodeContent()
    {
        // Arrange
        var obj = new TestData.TestObjects.SimpleRecord(TestData.Strings.Unicode, 42);
        var json = obj.SerializeObject();
        var tempFile = MockHelpers.CreateTempFile(json, ".json");

        try
        {
            // Act
            var result = tempFile.DeserializeObjectFromFile<TestData.TestObjects.SimpleRecord>();

            // Assert
            result.Should().NotBeNull();
            result!.Name.Should().Be(TestData.Strings.Unicode);
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    [Fact]
    public void FileOperations_ShouldHandleLargeFiles()
    {
        // Arrange
        var largeCollection = Enumerable.Range(1, 10000).ToList();
        var json = largeCollection.SerializeObject();
        var tempFile = MockHelpers.CreateTempFile(json, ".json");

        try
        {
            // Act
            var result = tempFile.DeserializeObjectFromFile<List<int>>();

            // Assert
            result.Should().NotBeNull();
            result!.Should().HaveCount(10000);
            result.Should().BeEquivalentTo(largeCollection);
        }
        finally
        {
            MockHelpers.CleanupTempFile(tempFile);
        }
    }

    private sealed class SettingsRecord
    {
        public DateTime When { get; set; }
    }
}
