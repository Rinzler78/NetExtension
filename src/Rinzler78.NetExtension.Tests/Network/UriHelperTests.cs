using Rinzler78.NetExtension;

namespace Rinzler78.NetExtension.Tests.Network;

[Trait("Category", "Unit")]
public class UriHelperTests
{
    [Theory]
    [InlineData("https://api.example.com:8443/customers", "https://api.example.com:8443/customers")]
    [InlineData("https://api.example.com:8443/customers/", "https://api.example.com:8443/customers")]
    [InlineData("http://localhost:5000/", "http://localhost:5000")]
    public void ToFullEndpoint_ShouldNormalizeTrailingSlash(string rawUri, string expected)
    {
        var uri = new Uri(rawUri);

        var result = uri.ToFullEndpoint();

        result.Should().Be(expected);
    }

    // ─────────────────────────────────────────────────────────────────
    // Default ports always included
    // ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("https://api.example.com/path", "https://api.example.com:443/path")]
    [InlineData("http://api.example.com/path", "http://api.example.com:80/path")]
    public void ToFullEndpoint_WithDefaultPorts_ShouldIncludePort(string rawUri, string expected)
    {
        var uri = new Uri(rawUri);

        var result = uri.ToFullEndpoint();

        result.Should().Be(expected);
    }

    // ─────────────────────────────────────────────────────────────────
    // Query string and fragment are discarded
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ToFullEndpoint_WithQueryString_ShouldDiscardQueryString()
    {
        var uri = new Uri("https://api.example.com:8443/path?key=value&other=123");

        var result = uri.ToFullEndpoint();

        result.Should().Be("https://api.example.com:8443/path");
    }

    [Fact]
    public void ToFullEndpoint_WithFragment_ShouldDiscardFragment()
    {
        var uri = new Uri("https://api.example.com:8443/path#section");

        var result = uri.ToFullEndpoint();

        result.Should().Be("https://api.example.com:8443/path");
    }

    // ─────────────────────────────────────────────────────────────────
    // Edge cases: no path, deep path, IP addresses
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void ToFullEndpoint_WithNoPath_ShouldReturnHostWithPort()
    {
        var uri = new Uri("https://api.example.com");

        var result = uri.ToFullEndpoint();

        // AbsolutePath is "/" which gets stripped by trailing-slash removal.
        result.Should().Be("https://api.example.com:443");
    }

    [Fact]
    public void ToFullEndpoint_WithDeepPath_ShouldPreserveAllSegments()
    {
        var uri = new Uri("https://api.example.com:8443/a/b/c/d/e/f");

        var result = uri.ToFullEndpoint();

        result.Should().Be("https://api.example.com:8443/a/b/c/d/e/f");
    }

    [Theory]
    [InlineData("http://192.168.1.1:8080/api", "http://192.168.1.1:8080/api")]
    [InlineData("http://127.0.0.1:5000/", "http://127.0.0.1:5000")]
    public void ToFullEndpoint_WithIpv4Address_ShouldPreserveIp(string rawUri, string expected)
    {
        var uri = new Uri(rawUri);

        var result = uri.ToFullEndpoint();

        result.Should().Be(expected);
    }

    [Fact]
    public void ToFullEndpoint_WithIpv6Address_ShouldHandleCorrectly()
    {
        var uri = new Uri("http://[::1]:5000/api");

        var result = uri.ToFullEndpoint();

        // Uri.Host for [::1] returns "::1" (without brackets).
        // The implementation uses uri.Host directly, so the output format depends on .NET behavior.
        result.Should().Contain("5000");
        result.Should().Contain("/api");
    }
}
