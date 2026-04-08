using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.Tests.Strings;

[Trait("Category", "Unit")]
public class StringOptimizationTests
{
    [Fact]
    public void GetFileExtensionOptimized_ShouldReturnCorrectExtension()
    {
        // Arrange & Act & Assert
        Assert.Equal(".pdf", "document.pdf".GetFileExtensionOptimized());
        Assert.Equal(".txt", "file.txt".GetFileExtensionOptimized());
        Assert.Equal(".gz", "archive.tar.gz".GetFileExtensionOptimized());
        Assert.Equal("", "filename".GetFileExtensionOptimized());
        Assert.Equal("", "".GetFileExtensionOptimized());
        Assert.Equal("", ((string)null!).GetFileExtensionOptimized());
        Assert.Equal(".exe", "C:\\Windows\\System32\\file.exe".GetFileExtensionOptimized());
        Assert.Equal(".txt", "/path/to/file.txt".GetFileExtensionOptimized());
        Assert.Equal("", "file.".GetFileExtensionOptimized()); // Dot at end, no extension
        Assert.Equal("", ".hidden".GetFileExtensionOptimized()); // Hidden file without extension
    }

    [Fact]
    public void StartsWithAnyOptimized_ShouldReturnCorrectResult()
    {
        // Arrange
        var prefixes = new[] { "http://", "https://", "ftp://" };

        // Act & Assert
        Assert.True("https://example.com".StartsWithAnyOptimized(prefixes));
        Assert.True("http://test.com".StartsWithAnyOptimized(prefixes));
        Assert.True("ftp://files.server.com".StartsWithAnyOptimized(prefixes));
        Assert.False("ws://socket.example.com".StartsWithAnyOptimized(prefixes));
        Assert.False("".StartsWithAnyOptimized(prefixes));
        Assert.False(((string)null!).StartsWithAnyOptimized(prefixes));
        Assert.False("https://example.com".StartsWithAnyOptimized(null!));
        Assert.False("https://example.com".StartsWithAnyOptimized(new string[0]));
    }

    [Fact]
    public void TrimOptimized_ShouldTrimCorrectly()
    {
        // Arrange & Act & Assert
        Assert.Equal("Hello World", "  Hello World  ".TrimOptimized());
        Assert.Equal("Hello World", "!!!Hello World!!!".TrimOptimized('!'));
        Assert.Equal("Hello World", "  !!!Hello World!!!  ".TrimOptimized('!'));
        Assert.Equal("Hello", "\t\nHello\r\n".TrimOptimized());
        Assert.Equal("", "".TrimOptimized());
        Assert.Equal("", ((string)null!).TrimOptimized());
        Assert.Equal("Hello", "Hello".TrimOptimized()); // Should return same instance if no trimming needed
    }

    [Theory]
    [InlineData("Hello World", "hello World")]
    [InlineData("XML", "xML")]
    [InlineData("a", "a")]
    [InlineData("ABC", "aBC")]
    [InlineData("123", "123")]
    [InlineData("", "")]
    public void BeginByLowerCase_OptimizedVersions_ShouldMatchOriginal(string input, string expected)
    {
        var result = input.BeginByLowerCase();
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData("hello world", "Hello world")]
    [InlineData("xml", "Xml")]
    [InlineData("a", "A")]
    [InlineData("abc", "Abc")]
    [InlineData("123", "123")]
    [InlineData("", "")]
    public void BeginByUpperCase_OptimizedVersions_ShouldMatchOriginal(string input, string expected)
    {
        var result = input.BeginByUpperCase();
        result.Should().Be(expected);
    }

    [Fact]
    public void MakeAllCombinations_OptimizedVersion_ShouldMatchOriginal()
    {
        // Arrange
        var input = new[] { "A", "B", "C" };
        var expected = new[] { "AA", "AB", "AC", "BA", "BB", "BC", "CA", "CB", "CC" };

        // Act
        var result = input.MakeAllCombinations().ToArray();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user.name@domain.co.uk", true)]
    [InlineData("another@test.org", true)]
    [InlineData("invalid-email", false)]
    [InlineData("@example.com", false)]
    [InlineData("test@", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmail_OptimizedVersion_ShouldMatchOriginal(string? email, bool expectedResult)
    {
        var result = email?.IsValidEmail() ?? false;
        result.Should().Be(expectedResult, $"Email '{email}' should be {(expectedResult ? "valid" : "invalid")}");
    }

    // ─────────────────────────────────────────────────────────────────
    // Additional optimization edge cases
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void GetFileExtensionOptimized_WithHiddenFileAndExtension_ShouldReturnExtension()
    {
        // Hidden file WITH an extension (e.g., .gitignore.bak)
        var result = "/tmp/.hidden.bak".GetFileExtensionOptimized();

        result.Should().Be(".bak");
    }

    [Fact]
    public void GetFileExtensionOptimized_WithDotAtEnd_ShouldReturnEmpty()
    {
        // Trailing dot — no characters after dot = no extension
        var result = "file.".GetFileExtensionOptimized();

        result.Should().BeEmpty();
    }

    [Fact]
    public void StartsWithAnyOptimized_WithEmptyPrefixArray_ShouldReturnFalse()
    {
        var result = "https://example.com".StartsWithAnyOptimized(System.Array.Empty<string>());

        result.Should().BeFalse();
    }

    [Fact]
    public void TrimOptimized_WithNoMatchingChars_ShouldReturnSameInstance()
    {
        var input = "hello";

        var result = input.TrimOptimized('*', '#');

        // No trimming needed — should return the exact same string instance
        result.Should().BeSameAs(input);
    }

    [Fact]
    public void TrimOptimized_WithOnlyWhitespace_ShouldReturnEmpty()
    {
        var result = "   \t\n\r   ".TrimOptimized();

        result.Should().BeEmpty();
    }
}
