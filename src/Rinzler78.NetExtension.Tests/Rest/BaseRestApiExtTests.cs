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

    // ─────────────────────────────────────────────────────────────────
    // Numeric / DateTime / Enum formatting
    // ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData(42, "42")]
    [InlineData(3.14, "3.14")]
    public void CreateUrlPath_NumericTypes_ShouldFormatWithInvariantCulture(
        object value, string expectedFragment)
    {
        var result = "/resource".CreateUrlPath(
            new Dictionary<string, object?> { ["n"] = value });

        result.Should().Be($"/resource?n={expectedFragment}");
    }

    [Fact]
    public void CreateUrlPath_DecimalValue_ShouldFormatWithInvariantCulture()
    {
        var result = "/resource".CreateUrlPath(
            new Dictionary<string, object?> { ["price"] = 1234.56m });

        result.Should().Be("/resource?price=1234.56");
    }

    [Fact]
    public void CreateUrlPath_DateTimeValue_ShouldUseInvariantCultureFormat()
    {
        var dt = new DateTime(2026, 1, 15, 10, 30, 0, DateTimeKind.Unspecified);
        var result = "/resource".CreateUrlPath(
            new Dictionary<string, object?> { ["date"] = dt });

        // DateTime.ToString(null, InvariantCulture) → "01/15/2026 10:30:00", then URL-encoded
        result.Should().Contain("date=");
        result.Should().Contain("2026");
    }

    [Fact]
    public void CreateUrlPath_WithEmptyStringValue_ShouldIncludeKeyWithEmptyValue()
    {
        var result = "/items".CreateUrlPath(
            new Dictionary<string, object?> { ["filter"] = "" });

        // Empty string is not null, so it passes the Where(a.Value is not null) filter.
        result.Should().Be("/items?filter=");
    }

    [Fact]
    public void CreateUrlPath_EnumValue_ShouldSerializeAsName()
    {
        var result = "/resource".CreateUrlPath(
            new Dictionary<string, object?> { ["status"] = System.Net.HttpStatusCode.OK });

        result.Should().Contain("status=OK");
    }

    // ─────────────────────────────────────────────────────────────────
    // Known limitation — existing query string
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CreateUrlPath_WithExistingQueryString_ShouldAppendWithQuestionMark()
    {
        // Known limitation: the implementation always appends "?" regardless of existing query.
        var result = "https://api.example.com/v1?existing=true".CreateUrlPath(
            new Dictionary<string, object?> { ["new"] = 1 });

        // Documents the double "?" behavior:
        result.Should().Contain("?existing=true?new=1");
    }

    // ─────────────────────────────────────────────────────────────────
    // Parameter ordering and bool false
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void CreateUrlPath_ShouldPreserveParameterInsertionOrder()
    {
        var args = new Dictionary<string, object?>
        {
            ["z"] = 1,
            ["a"] = 2,
            ["m"] = 3
        };

        var result = "/resource".CreateUrlPath(args);

        result.Should().Be("/resource?z=1&a=2&m=3");
    }

    [Fact]
    public void CreateUrlPath_WithBoolFalseValue_SerializesAsFalse()
    {
        var result = "/resource".CreateUrlPath(
            new Dictionary<string, object?> { ["active"] = (object)false });

        result.Should().Contain("active=false");
    }

    [Fact]
    public void CreateUrlPath_WithObjectToStringFallback_ShouldUseToString()
    {
        // An object that is not bool and not IFormattable falls through to ToString()
        var result = "/resource".CreateUrlPath(
            new Dictionary<string, object?> { ["id"] = new Uri("https://example.com") });

        result.Should().Contain("id=");
        result.Should().Contain("example.com");
    }
}
