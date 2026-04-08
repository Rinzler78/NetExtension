using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rinzler78.NetExtension.Strings;
using Xunit;

namespace Rinzler78.NetExtension.Tests.Strings;

[Trait("Category", "Unit")]
public class StringHelperTests
{
    [Theory]
    [InlineData("test@example.com", true)]
    [InlineData("user@domain.co.uk", true)]
    [InlineData("invalid.email", false)]
    [InlineData("@domain.com", false)]
    [InlineData("user@", false)]
    [InlineData("", false)]
    [InlineData(null, false)]
    public void IsValidEmail_ShouldReturnCorrectResult(string? email, bool expected)
    {
        // Act
        var result = email?.IsValidEmail() ?? false;

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "", 0)]
    [InlineData("abc", "", 3)]
    [InlineData("", "abc", 3)]
    [InlineData("abc", "abc", 0)]
    [InlineData("kitten", "sitting", 3)]
    [InlineData("saturday", "sunday", 3)]
    public void ComputeLevenshteinDistance_ShouldReturnCorrectDistance(string source, string target, int expected)
    {
        // Act
        var result = source.ComputeLevenshteinDistance(target);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "", 1.0)]
    [InlineData("abc", "abc", 1.0)]
    [InlineData("abc", "", 0.0)]
    [InlineData("", "abc", 0.0)]
    [InlineData("kitten", "sitting", 0.5714285714285714)]
    public void CalculateSimilarity_ShouldReturnCorrectSimilarity(string source, string target, double expected)
    {
        // Act
        var result = source.CalculateSimilarity(target);

        // Assert
        Assert.Equal(expected, result, 10); // 10 decimal places precision
    }

    [Theory]
    [InlineData("hello world test", new[] { "hello", "world" }, true)]
    [InlineData("hello world test", new[] { "hello", "missing" }, false)]
    [InlineData("hello world test", new[] { "test", "world", "hello" }, true)]
    [InlineData("hello world test", null, true)]
    [InlineData("hello world test", new string[0], true)]
    public void ContainsAll_ShouldReturnCorrectResult(string str, string[]? words, bool expected)
    {
        // Act
        var result = str.ContainsAll(words ?? new string[0]);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world test", new[] { "hello", "missing" }, true)]
    [InlineData("hello world test", new[] { "missing", "absent" }, false)]
    [InlineData("hello world test", new[] { "test" }, true)]
    [InlineData("hello world test", null, true)]
    [InlineData("hello world test", new string[0], true)]
    public void ContainsAny_ShouldReturnCorrectResult(string str, string[]? words, bool expected)
    {
        // Act
        var result = str.ContainsAny(words ?? new string[0]);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("Hello", "hello")]
    [InlineData("WORLD", "wORLD")]
    [InlineData("Test", "test")]
    [InlineData("a", "a")]
    public void BeginByLowerCase_ShouldReturnStringWithLowerCaseFirstChar(string input, string expected)
    {
        // Act
        var result = input.BeginByLowerCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", "Hello")]
    [InlineData("world", "World")]
    [InlineData("TEST", "TEST")]
    [InlineData("a", "A")]
    public void BeginByUpperCase_ShouldReturnStringWithUpperCaseFirstChar(string input, string expected)
    {
        // Act
        var result = input.BeginByUpperCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("HELLO WORLD", "Hello world")]
    [InlineData("test STRING", "Test string")]
    [InlineData("MiXeD cAsE", "Mixed case")]
    [InlineData("", "")]
    [InlineData("A", "A")]
    public void ToStartByUpperCase_ShouldReturnCorrectFormat(string input, string expected)
    {
        // Act
        var result = input.ToStartByUpperCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello_world", "HelloWorld")]
    [InlineData("test-case", "TestCase")]
    [InlineData("some text here", "SomeTextHere")]
    [InlineData("multiple___underscores", "MultipleUnderscores")]
    [InlineData("numbers123test", "Numbers123Test")]
    public void ToPascalCase_ShouldReturnCorrectPascalCase(string input, string expected)
    {
        // Act
        var result = input.ToPascalCase();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("alreadyPascal", "Alreadypascal")]
    [InlineData(".hidden-file", "HiddenFile")]
    [InlineData("user99name", "User99Name")]
    public void ToPascalCase_ShouldHandleEdgeCases(string input, string expected)
    {
        var result = input.ToPascalCase();

        Assert.Equal(expected, result);
    }

    [Fact]
    public void MakeAllCombinations_ShouldReturnAllCombinations()
    {
        // Arrange
        var strings = new[] { "a", "b" };

        // Act
        var result = strings.MakeAllCombinations().ToArray();

        // Assert
        Assert.Equal(4, result.Length);
        Assert.Contains("aa", result);
        Assert.Contains("ab", result);
        Assert.Contains("ba", result);
        Assert.Contains("bb", result);
    }

    [Fact]
    public void MakeAllCombinations_WithNullSource_ShouldThrowArgumentNullException()
    {
        IEnumerable<string>? values = null;

        Action act = () => values!.MakeAllCombinations().ToArray();

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData("", true)]
    [InlineData(null, true)]
    [InlineData("not empty", false)]
    [InlineData(" ", false)]
    public void IsNullOrEmpty_ShouldReturnCorrectResult(string? input, bool expected)
    {
        // Act
        // Pass input directly (including null) via the static call to avoid ?. null guard
        var result = StringHelper.IsNullOrEmpty(input!);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Join_WithCharSeparator_ShouldReturnJoinedString()
    {
        // Arrange
        var strings = new[] { "hello", "world", "test" };

        // Act
        var result = strings.Join(',');

        // Assert
        Assert.Equal("hello,world,test", result);
    }

    [Fact]
    public void Join_WithEmptyArray_ShouldReturnEmptyString()
    {
        var result = System.Array.Empty<string>().Join('|');

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public async Task GetStringContent_ShouldReturnCorrectStringContent()
    {
        // Arrange
        var obj = new { Name = "Test", Value = 123 };

        // Act
        var result = obj.GetStringContent();

        // Assert
        Assert.NotNull(result);
        Assert.Equal("application/json", result.Headers.ContentType?.MediaType);
        var content = await result.ReadAsStringAsync();
        Assert.Contains("Test", content);
        Assert.Contains("123", content);
    }

    [Theory]
    [InlineData("{\"name\":\"test\"}", "{\r\n  \"name\": \"test\"\r\n}")]
    [InlineData("[1,2,3]", "[\r\n  1,\r\n  2,\r\n  3\r\n]")]
    public void ToJsonFormattedString_ShouldReturnFormattedJson(string input, string expected)
    {
        // Act
        var result = input.ToJsonFormattedString();

        // Assert
        // Normalize line endings for cross-platform compatibility
        var normalizedResult = result.Replace("\r\n", "\n").Replace("\r", "\n");
        var normalizedExpected = expected.Replace("\r\n", "\n").Replace("\r", "\n");
        Assert.Equal(normalizedExpected, normalizedResult);
    }

    [Theory]
    [InlineData(0, "0 B")]
    [InlineData(1023, "1023 B")]
    [InlineData(1024, "1 KB")]
    [InlineData(1536, "1.5 KB")]
    [InlineData(1048576, "1 MB")]
    [InlineData(1073741824, "1 GB")]
    public void GetBytesReadable_Int_ShouldReturnCorrectFormat(int bytes, string expected)
    {
        // Act
        var result = bytes.GetBytesReadable();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(0L, "0 B")]
    [InlineData(1023L, "1023 B")]
    [InlineData(1024L, "1 KB")]
    [InlineData(1536L, "1.5 KB")]
    [InlineData(1048576L, "1 MB")]
    [InlineData(1073741824L, "1 GB")]
    [InlineData(1099511627776L, "1 TB")]
    [InlineData(-1024L, "-1 KB")]
    public void GetBytesReadable_Long_ShouldReturnCorrectFormat(long bytes, string expected)
    {
        // Act
        var result = bytes.GetBytesReadable();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(1125899906842624L, "1 PB")]
    [InlineData(1152921504606846976L, "1 EB")]
    [InlineData(-1152921504606846976L, "-1 EB")]
    public void GetBytesReadable_Long_ShouldHandleLargeUnits(long bytes, string expected)
    {
        var result = bytes.GetBytesReadable();

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello", new byte[] { 104, 101, 108, 108, 111 })]
    [InlineData("", new byte[0])]
    [InlineData("A", new byte[] { 65 })]
    public void GetBytes_ShouldReturnCorrectByteArray(string input, byte[] expected)
    {
        // Act
        var result = input.GetBytes();

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData(null, "")]
    [InlineData("", "")]
    [InlineData("file.txt", ".txt")]
    [InlineData("archive.tar.gz", ".gz")]
    [InlineData("/tmp/.hidden", "")]
    [InlineData("/tmp/folder.name/file", "")]
    [InlineData("C:/temp/report.json", ".json")]
    public void GetFileExtensionOptimized_ShouldReturnExpectedExtension(string? input, string expected)
    {
        var result = StringHelper.GetFileExtensionOptimized(input!);

        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("https://example.com", true)]
    [InlineData("HTTP://example.com", true)]
    [InlineData("ftp://example.com", false)]
    [InlineData("", false)]
    public void StartsWithAnyOptimized_ShouldReturnExpectedResult(string input, bool expected)
    {
        var result = input.StartsWithAnyOptimized(new[] { "http://", "https://" }, StringComparison.OrdinalIgnoreCase);

        Assert.Equal(expected, result);
    }

    [Fact]
    public void StartsWithAnyOptimized_WithNullPrefixes_ShouldReturnFalse()
    {
        var result = "https://example.com".StartsWithAnyOptimized(null!);

        Assert.False(result);
    }

    [Fact]
    public void TrimOptimized_WithNullInput_ShouldReturnEmptyString()
    {
        var result = StringHelper.TrimOptimized(null!);

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void TrimOptimized_WithEmptyInput_ShouldReturnEmptyString()
    {
        var result = string.Empty.TrimOptimized();

        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void TrimOptimized_WithWhitespaceOnly_ShouldTrimWhitespace()
    {
        var result = "  hello  ".TrimOptimized();

        Assert.Equal("hello", result);
    }

    [Fact]
    public void TrimOptimized_WithCustomTrimCharacters_ShouldTrimThemFromBothSides()
    {
        var result = "***hello***".TrimOptimized('*');

        Assert.Equal("hello", result);
    }

    [Fact]
    public void TrimOptimized_WhenNoTrimIsNeeded_ShouldReturnOriginalString()
    {
        var input = "hello";

        var result = input.TrimOptimized('*');

        Assert.Same(input, result);
    }

    // ─────────────────────────────────────────────────────────────────
    // Additional email edge cases
    // ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("user+tag@example.com", true)]
    [InlineData("user.name+tag@sub.domain.com", true)]
    [InlineData("user@123.123.123.123", true)]
    [InlineData("user@.domain.com", true)]  // EmailAddressAttribute accepts this
    [InlineData("user..name@domain.com", true)]  // EmailAddressAttribute accepts this
    [InlineData(" @domain.com", true)]  // EmailAddressAttribute accepts leading space
    [InlineData("plaintext", false)]
    [InlineData("a@b@c.com", false)]
    public void IsValidEmail_WithEdgeCases_ShouldValidateCorrectly(string email, bool expected)
    {
        var result = email.IsValidEmail();

        Assert.Equal(expected, result);
    }

    // ─────────────────────────────────────────────────────────────────
    // Levenshtein / Similarity additional edge cases
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ComputeLevenshteinDistance_WithNullSource_ShouldReturnTargetLength()
    {
        string? source = null;

        var result = source!.ComputeLevenshteinDistance("abc");

        Assert.Equal(3, result);
    }

    [Fact]
    public void ComputeLevenshteinDistance_WithBothNull_ShouldReturnZero()
    {
        string? source = null;
        string? target = null;

        var result = source!.ComputeLevenshteinDistance(target!);

        Assert.Equal(0, result);
    }

    [Fact]
    public void CalculateSimilarity_WithBothNull_ShouldReturnOne()
    {
        string? a = null;
        string? b = null;

        var result = a!.CalculateSimilarity(b!);

        Assert.Equal(1.0, result);
    }

    // ─────────────────────────────────────────────────────────────────
    // ContainsAll / ContainsAny with null str (extension method edge case)
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ContainsAll_WithNullString_ShouldThrowNullReferenceException()
    {
        string? str = null;

        // Extension method called on null: str.Contains() throws NRE.
        Action act = () => str!.ContainsAll(new[] { "word" });

        act.Should().Throw<NullReferenceException>();
    }

    [Fact]
    public void ContainsAny_WithNullString_ShouldThrowNullReferenceException()
    {
        string? str = null;

        Action act = () => str!.ContainsAny(new[] { "word" });

        act.Should().Throw<NullReferenceException>();
    }

    // ─────────────────────────────────────────────────────────────────
    // ToPascalCase additional edge cases
    // ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(null, null)]
    [InlineData("a", "A")]
    [InlineData("123abc", "123Abc")]
    [InlineData("with--dashes", "WithDashes")]
    [InlineData("___leading", "Leading")]
    public void ToPascalCase_WithMoreEdgeCases_ShouldConvertCorrectly(string? input, string? expected)
    {
        var result = input!.ToPascalCase();

        Assert.Equal(expected, result);
    }

    // ─────────────────────────────────────────────────────────────────
    // MakeAllCombinations edge cases
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void MakeAllCombinations_WithSingleElement_ShouldReturnSelfCombination()
    {
        var result = new[] { "X" }.MakeAllCombinations().ToArray();

        Assert.Single(result);
        Assert.Equal("XX", result[0]);
    }

    [Fact]
    public void MakeAllCombinations_WithEmptyCollection_ShouldReturnEmpty()
    {
        var result = System.Array.Empty<string>().MakeAllCombinations().ToArray();

        Assert.Empty(result);
    }

    // ─────────────────────────────────────────────────────────────────
    // GetStringContent edge case
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetStringContent_WithSimpleString_ShouldSerializeToJson()
    {
        var content = "hello".GetStringContent();
        var body = await content.ReadAsStringAsync();

        body.Should().Be("\"hello\"");
    }

    // ─────────────────────────────────────────────────────────────────
    // GetBytes — Unicode → ASCII (non-ASCII → 0x3F)
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void GetBytes_WithUnicodeCharacters_ShouldReplaceWithQuestionMark()
    {
        var result = "café".GetBytes();

        // 'é' (U+00E9) is outside ASCII range → replaced by 0x3F ('?')
        result.Should().Contain(0x3F);
        result.Length.Should().Be(4); // c, a, f, ?
    }

    // ─────────────────────────────────────────────────────────────────
    // BeginByLowerCase / BeginByUpperCase — null input
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void BeginByLowerCase_WithNull_ShouldReturnNull()
    {
        string? input = null;

        var result = input!.BeginByLowerCase();

        Assert.Null(result);
    }

    [Fact]
    public void BeginByUpperCase_WithNull_ShouldReturnNull()
    {
        string? input = null;

        var result = input!.BeginByUpperCase();

        Assert.Null(result);
    }

    // ─────────────────────────────────────────────────────────────────
    // ToJsonFormattedString — invalid JSON
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ToJsonFormattedString_WithInvalidJson_ShouldThrow()
    {
        Action act = () => "not json at all".ToJsonFormattedString();

        act.Should().Throw<Newtonsoft.Json.JsonReaderException>();
    }
}
