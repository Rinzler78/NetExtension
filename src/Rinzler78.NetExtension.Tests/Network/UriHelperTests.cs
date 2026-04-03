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
}
