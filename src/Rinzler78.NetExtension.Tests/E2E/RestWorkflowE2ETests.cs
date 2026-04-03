using Newtonsoft.Json;
using Rinzler78.NetExtension;
using Rinzler78.NetExtension.Json;
using Rinzler78.NetExtension.Rest;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.E2E;

[Trait("Category", "E2E")]
public class RestWorkflowE2ETests
{
    [Fact]
    public void EndToEnd_RestEndpointManifest_ShouldRoundTripWithNormalizedRoutes()
    {
        var filePath = MockHelpers.CreateTempFile(string.Empty, ".json");

        try
        {
            var api = new CustomerApi("https://api.example.com", "v2/customers");
            var tags = new List<string> { "draft", "legacy" };
            tags.Set(new[] { "production", "rest", "coverage" });

            var manifest = new EndpointManifest
            {
                Endpoint = api.BaseUri.ToFullEndpoint(),
                RequestUrl = api.BaseUri.AbsoluteUri.CreateUrlPath(new Dictionary<string, object?>
                {
                    ["page"] = 3,
                    ["size"] = 25,
                    ["draft"] = null
                }),
                Tags = tags.OrderBy(tag => tag).ToArray()
            };

            File.WriteAllText(filePath, manifest.SerializeObject(Formatting.Indented));

            var restored = filePath.DeserializeObjectFromFile<EndpointManifest>();

            restored.Endpoint.Should().Be("https://api.example.com:443/v2/customers");
            restored.RequestUrl.Should().Be("https://api.example.com/v2/customers?page=3&size=25");
            restored.Tags.Should().BeEquivalentTo(new[] { "coverage", "production", "rest" });
        }
        finally
        {
            MockHelpers.CleanupTempFile(filePath);
        }
    }

    private sealed class CustomerApi : BaseRestApi
    {
        public CustomerApi(string baseUri, string path)
            : base(baseUri, path)
        {
        }
    }

    private sealed class EndpointManifest
    {
        public string Endpoint { get; set; } = string.Empty;

        public string RequestUrl { get; set; } = string.Empty;

        public string[] Tags { get; set; } = System.Array.Empty<string>();
    }
}
