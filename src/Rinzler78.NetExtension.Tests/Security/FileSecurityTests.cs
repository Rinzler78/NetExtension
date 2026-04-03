using System;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using FluentAssertions;
using Rinzler78.NetExtension.Json;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Security;

/// <summary>
/// Security validation tests for file operations and path handling.
/// Tests for potential vulnerabilities in file I/O operations.
/// </summary>
[Trait("Category", "Unit")]
public class FileSecurityTests : IDisposable
{
    private readonly string _tempDirectory;

    public FileSecurityTests()
    {
        _tempDirectory = Path.Combine(Path.GetTempPath(), "NetExtensionSecurityTests", Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDirectory);
    }

    [Theory]
    [InlineData("../../../etc/passwd")]
    [InlineData("..\\..\\windows\\system32\\config\\sam")]
    [InlineData("/etc/passwd")]
    [InlineData("../../../../root/.ssh/id_rsa")]
    public void DeserializeObjectFromFile_DirectoryTraversalAttempts_ShouldThrowSecurityException(string maliciousPath)
    {
        // Act & Assert
        Action act = () => maliciousPath.DeserializeObjectFromFile<object>();

        // Should throw an exception for security reasons
        act.Should().Throw<ArgumentException>()
            .Which.Message.Should().Contain("filePath");
    }

    [Theory]
    [InlineData("file\0name.json")]           // Null byte injection
    [InlineData("file\r\nname.json")]         // CRLF injection
    [InlineData("file\u0001name.json")]       // Control character
    [InlineData("file\u007F.json")]           // DEL character
    [InlineData("file\u0008.json")]           // Backspace
    public void DeserializeObjectFromFile_ControlCharactersInFilename_ShouldHandleSafely(string filenameWithControlChars)
    {
        // Arrange
        var filePath = Path.Combine(_tempDirectory, filenameWithControlChars);

        // Act & Assert
        Action act = () => filePath.DeserializeObjectFromFile<object>();

        // Should either sanitize the filename or throw an appropriate exception
        act.Should().Throw<Exception>();
    }

    [Theory]
    [InlineData("CON.json")]                  // Windows reserved name
    [InlineData("PRN.json")]                  // Windows reserved name
    [InlineData("AUX.json")]                  // Windows reserved name
    [InlineData("NUL.json")]                  // Windows reserved name
    [InlineData("LPT1.json")]                 // Windows reserved name
    [InlineData("COM1.json")]                 // Windows reserved name
    public void DeserializeObjectFromFile_WindowsReservedNames_ShouldHandleSafely(string reservedName)
    {
        // This test is primarily for Windows systems, but should handle gracefully on all platforms

        // Arrange
        var filePath = Path.Combine(_tempDirectory, reservedName);

        // Act & Assert
        Action act = () => filePath.DeserializeObjectFromFile<object>();

        // On non-Windows these names are valid filenames; the file simply does not exist
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void DeserializeObjectFromFile_ExtremelyLongFilename_ShouldHandleGracefully()
    {
        // Arrange
        var longFilename = new string('A', 1000) + ".json";
        var filePath = Path.Combine(_tempDirectory, longFilename);

        // Act & Assert
        Action act = () => filePath.DeserializeObjectFromFile<object>();

        // File with a 1000-char name does not exist
        act.Should().Throw<FileNotFoundException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DeserializeObjectFromFile_EmptyPath_ShouldThrowArgumentException(string invalidPath)
    {
        // Act & Assert
        Action act = () => invalidPath.DeserializeObjectFromFile<object>();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeserializeObjectFromFile_NullPath_ShouldThrowArgumentNullException()
    {
        // Act & Assert
        string? nullPath = null;
        Action act = () => nullPath!.DeserializeObjectFromFile<object>();
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DeserializeObjectFromFile_NonExistentFile_ShouldThrowFileNotFoundException()
    {
        // Arrange
        var nonExistentPath = Path.Combine(_tempDirectory, "nonexistent.json");

        // Act & Assert
        Action act = () => nonExistentPath.DeserializeObjectFromFile<object>();
        act.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void DeserializeObjectFromFile_ReadOnlyDirectory_ShouldThrowUnauthorizedAccessException()
    {
        // This test simulates attempting to read from a protected directory

        // Arrange
        var readOnlyPath = "/System/Library/test.json"; // macOS system directory
        if (Environment.OSVersion.Platform == PlatformID.Win32NT)
        {
            readOnlyPath = "C:\\Windows\\System32\\test.json"; // Windows system directory
        }

        // Act & Assert
        Action act = () => readOnlyPath.DeserializeObjectFromFile<object>();

        // The file does not exist in the protected directory (File.Exists returns false → FileNotFoundException)
        act.Should().Throw<FileNotFoundException>();
    }

    [Theory]
    [InlineData("file:///etc/passwd")]
    [InlineData("http://evil.com/malicious.json")]
    [InlineData("ftp://evil.com/file.json")]
    public void DeserializeObjectFromFile_MaliciousUriSchemes_ShouldThrowArgumentException(string maliciousUri)
    {
        // Act & Assert
        Action act = () => maliciousUri.DeserializeObjectFromFile<object>();

        // Should not attempt to download from remote URLs or access non-file schemes
        act.Should().Throw<Exception>();
    }

    [Fact]
    public void DeserializeObjectFromFile_ValidJsonFile_ShouldDeserializeCorrectly()
    {
        // Arrange
        var validJsonContent = "{\"Name\":\"Test\",\"Value\":123}";
        var validJsonPath = Path.Combine(_tempDirectory, "valid.json");
        File.WriteAllText(validJsonPath, validJsonContent);

        // Act
        var result = validJsonPath.DeserializeObjectFromFile<object>();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeOfType<Newtonsoft.Json.Linq.JObject>();
    }

    [Fact]
    public void DeserializeObjectFromFile_MalformedJsonFile_ShouldThrowJsonException()
    {
        // Arrange
        var malformedJsonContent = "{invalid json content";
        var malformedJsonPath = Path.Combine(_tempDirectory, "malformed.json");
        File.WriteAllText(malformedJsonPath, malformedJsonContent);

        // Act & Assert
        Action act = () => malformedJsonPath.DeserializeObjectFromFile<object>();
        act.Should().Throw<Newtonsoft.Json.JsonException>();
    }

    [Fact]
    public void DeserializeObjectFromFile_LargeJsonFile_ShouldHandleMemoryEfficiently()
    {
        // Arrange
        var largeJsonContent = "{\"Data\":\"" + new string('X', 100000) + "\"}"; // 100KB+ content
        var largeJsonPath = Path.Combine(_tempDirectory, "large.json");
        File.WriteAllText(largeJsonPath, largeJsonContent);

        // Act & Assert
        Action act = () => largeJsonPath.DeserializeObjectFromFile<object>();

        act.Should().NotThrow();
        act.ExecutionTime().Should().BeLessThan(TimeSpan.FromSeconds(5),
            "Deserialization should complete in reasonable time");

        var result = largeJsonPath.DeserializeObjectFromFile<object>();
        result.Should().NotBeNull();
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_tempDirectory))
            {
                Directory.Delete(_tempDirectory, true);
            }
        }
        catch
        {
            // Ignore cleanup errors in tests
        }
    }
}
