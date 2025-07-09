using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.Tests.Strings;

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
    public void IsValidEmail_ShouldReturnCorrectResult(string email, bool expected)
    {
        // Act
        var result = email.IsValidEmail();

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
    public void ComputeLevenshteInDistance_ShouldReturnCorrectDistance(string source, string target, int expected)
    {
        // Act
        var result = source.ComputeLevenshteInDistance(target);

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
    public void ContainsAll_ShouldReturnCorrectResult(string str, string[] words, bool expected)
    {
        // Act
        var result = str.ContainsAll(words);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("hello world test", new[] { "hello", "missing" }, true)]
    [InlineData("hello world test", new[] { "missing", "absent" }, false)]
    [InlineData("hello world test", new[] { "test" }, true)]
    [InlineData("hello world test", null, true)]
    [InlineData("hello world test", new string[0], true)]
    public void ContainsAny_ShouldReturnCorrectResult(string str, string[] words, bool expected)
    {
        // Act
        var result = str.ContainsAny(words);

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

    [Theory]
    [InlineData("", true)]
    [InlineData(null, true)]
    [InlineData("not empty", false)]
    [InlineData(" ", false)]
    public void IsNullOrEmpty_ShouldReturnCorrectResult(string input, bool expected)
    {
        // Act
        var result = input.IsNullOrEmpty();

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
    public void ToJsonFormatedString_ShouldReturnFormattedJson(string input, string expected)
    {
        // Act
        var result = input.ToJsonFormatedString();

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

    // Note: HTTP methods are not tested here as they require external dependencies
    // These would be better suited for integration tests or mocked tests
}
