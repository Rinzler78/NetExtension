using Rinzler78.NetExtension.Rest;

namespace Rinzler78.NetExtension.Tests.Rest;

[Trait("Category", "Unit")]
public class BaseRestApiExtTests
{
    // ─────────────────────────────────────────────────────────────────
    // Existing tests (unchanged)
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CreateUrlPath_WithNullArguments_ShouldReturnOriginalUrl()
    {
        var result = "/customers".CreateUrlPath();

        result.Should().Be("/customers");
    }

    [Fact]
    public void CreateUrlPath_WithEmptyArguments_ShouldReturnOriginalUrl()
    {
        var result = "/customers".CreateUrlPath(new Dictionary<string, object?>());

        result.Should().Be("/customers");
    }

    [Fact]
    public void CreateUrlPath_WithArguments_ShouldAppendQueryStringAndSkipNullValues()
    {
        var result = "/customers".CreateUrlPath(new Dictionary<string, object?>
        {
            ["page"] = 2,
            ["includeInactive"] = false,
            ["ignored"] = null
        });

        result.Should().Be("/customers?page=2&includeInactive=false");
    }

    // ─────────────────────────────────────────────────────────────────
    // Null / empty root URL
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CreateUrlPath_WithNullBaseUrl_ShouldReturnNullUnchanged()
    {
        // CreateUrlPath is a string extension method.  When called on null the
        // C# compiler routes it to the static method with a null first argument.
        // The implementation performs no null-guard — it returns whatever baseUrl
        // is, which is null.  This test documents that contract.
        string? baseUrl = null;

        var result = baseUrl!.CreateUrlPath();

        result.Should().BeNull(
            "CreateUrlPath has no null-guard; a null baseUrl is returned as-is");
    }

    [Fact]
    public void CreateUrlPath_WithEmptyBaseUrl_ShouldReturnEmptyString()
    {
        var result = string.Empty.CreateUrlPath();

        result.Should().BeEmpty();
    }

    // ─────────────────────────────────────────────────────────────────
    // Trailing slash behaviour
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CreateUrlPath_BaseUrlWithTrailingSlash_ShouldAppendQueryStringAfterSlash()
    {
        var result = "https://api.example.com/v1/".CreateUrlPath(
            new Dictionary<string, object?> { ["limit"] = 10 });

        result.Should().Be("https://api.example.com/v1/?limit=10");
    }

    [Fact]
    public void CreateUrlPath_BaseUrlWithoutTrailingSlash_ShouldAppendQueryStringDirectly()
    {
        var result = "https://api.example.com/v1".CreateUrlPath(
            new Dictionary<string, object?> { ["limit"] = 10 });

        result.Should().Be("https://api.example.com/v1?limit=10");
    }

    // ─────────────────────────────────────────────────────────────────
    // Special characters in path segments / argument values
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CreateUrlPath_WithSpecialCharactersInArgumentValues_ShouldUrlEncodeThem()
    {
        var result = "/search".CreateUrlPath(new Dictionary<string, object?>
        {
            ["q"] = "hello world",
            ["tag"] = "a&b=c"
        });

        result.Should().Be("/search?q=hello+world&tag=a%26b%3Dc",
            "query-string values must be URL-encoded so special characters " +
            "do not corrupt downstream requests");
    }

    [Fact]
    public void CreateUrlPath_WithAllNullArgumentValues_ShouldReturnOriginalUrl()
    {
        // All values are null → filtered out → no query string appended.
        var result = "/items".CreateUrlPath(new Dictionary<string, object?>
        {
            ["a"] = null,
            ["b"] = null
        });

        result.Should().Be("/items");
    }

    // ─────────────────────────────────────────────────────────────────
    // Gap tests — bool, all-null full URL, special char key
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CreateUrlPath_WithBoolTrueValue_SerializesAsTrue()
    {
        var result = "https://api.example.com/resource"
            .CreateUrlPath(new Dictionary<string, object?> { ["active"] = (object)true });
        result.Should().Contain("active=true");
    }

    [Fact]
    public void CreateUrlPath_WithAllNullValues_ReturnsUnchangedUrl()
    {
        const string baseUrl = "https://api.example.com/resource";
        var result = baseUrl.CreateUrlPath(
            new Dictionary<string, object?> { ["a"] = null, ["b"] = null });
        result.Should().Be(baseUrl);
    }

    [Fact]
    public void CreateUrlPath_WithSpecialCharacterInKey_EncodesKey()
    {
        var result = "https://api.example.com/resource"
            .CreateUrlPath(new Dictionary<string, object?> { ["my key"] = 1 });
        // WebUtility.UrlEncode encodes space as '+'
        result.Should().MatchRegex(@"my(\+|%20)key=1");
    }
}
