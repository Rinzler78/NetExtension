using System.Net;
using System.Net.Http;
using Moq;
using Moq.Protected;
using Rinzler78.NetExtension.Rest;
using Rinzler78.NetExtension.Tests.TestHelpers;

namespace Rinzler78.NetExtension.Tests.Rest;

[Trait("Category", "Unit")]
public class BaseRestApiTests
{
    // ─────────────────────────────────────────────────────────────────
    // Constructor / URI resolution
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithStringBaseUriAndPath_ShouldAppendPath()
    {
        var api = new TestRestApi("https://api.example.com", "v1/customers");

        api.BaseUri.AbsoluteUri.Should().Be("https://api.example.com/v1/customers");
    }

    [Fact]
    public void Constructor_WithUriBaseUriAndTrailingSlash_ShouldNotDuplicateSlash()
    {
        var api = new TestRestApi(new Uri("https://api.example.com/"), "v1/customers");

        api.BaseUri.AbsoluteUri.Should().Be("https://api.example.com/v1/customers");
    }

    [Fact]
    public void Constructor_WithNullPath_ShouldKeepOriginalUri()
    {
        var baseUri = new Uri("https://api.example.com/v1/");

        var api = new TestRestApi(baseUri, null);

        api.BaseUri.Should().BeSameAs(baseUri);
    }

    [Fact]
    public void Constructor_WithNullUri_ShouldThrowArgumentNullException()
    {
        Action act = () => new TestRestApi((Uri)null!, "v1/customers");

        act.Should().Throw<ArgumentNullException>().WithParameterName("baseUri");
    }

    // ─────────────────────────────────────────────────────────────────
    // HttpMessageHandler mock tests
    //
    // NOTE: BaseRestApi only manages URI construction — it does not expose
    // GetAsync / PostAsync directly.  The tests below use a concrete subclass
    // (HttpTestRestApi) that wires an HttpClient to the resolved BaseUri, which
    // is the expected usage pattern for inheritors.  This verifies that
    // BaseUri is configured correctly so that HTTP requests hit the right URL.
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public async Task GetAsync_ShouldSendRequestToResolvedBaseUri()
    {
        var handlerMock = MockHelpers.CreateHttpMessageHandlerMock(
            responseContent: "{\"ok\":true}",
            statusCode: HttpStatusCode.OK);

        var api = new HttpTestRestApi("https://api.example.com", "v1/orders", handlerMock.Object);

        var response = await api.GetAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify that exactly one GET request was sent to the correct URL.
        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri!.AbsoluteUri == "https://api.example.com/v1/orders"),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public async Task PostAsync_ShouldSendPostRequestWithBodyToResolvedBaseUri()
    {
        var handlerMock = MockHelpers.CreateHttpMessageHandlerMock(
            responseContent: "{\"created\":true}",
            statusCode: HttpStatusCode.Created);

        var api = new HttpTestRestApi("https://api.example.com", "v1/orders", handlerMock.Object);
        var payload = new StringContent("{\"item\":\"widget\"}", System.Text.Encoding.UTF8, "application/json");

        var response = await api.PostAsync(payload);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        handlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Post &&
                    req.RequestUri!.AbsoluteUri == "https://api.example.com/v1/orders"),
                ItExpr.IsAny<CancellationToken>());
    }

    [Fact]
    public void Constructor_WithLeadingSlashInPath_ProducesSameUriAsWithoutSlash()
    {
        var baseUri = new Uri("https://api.example.com/");
        var withSlash = new TestRestApi(baseUri, "/v1/resource");
        var withoutSlash = new TestRestApi(baseUri, "v1/resource");

        withSlash.BaseUri.Should().Be(withoutSlash.BaseUri);
    }

    [Fact]
    public void Constructor_ChainedConstruction_ShouldPreserveAllSegments()
    {
        // Reproduces the REST API hierarchy: root → cosmos → bank → v1beta1 → balances
        // Each level passes its BaseUri (no trailing slash) to the next level.
        // Before the fix, new Uri(root, segment) without trailing slash replaced the last segment.
        var root = new TestRestApi("https://api.example.com/", "cosmos");
        var l1 = new TestRestApi(root.BaseUri, "bank");
        var l2 = new TestRestApi(l1.BaseUri, "v1beta1");
        var l3 = new TestRestApi(l2.BaseUri, "balances");

        root.BaseUri.AbsoluteUri.Should().Be("https://api.example.com/cosmos");
        l1.BaseUri.AbsoluteUri.Should().Be("https://api.example.com/cosmos/bank");
        l2.BaseUri.AbsoluteUri.Should().Be("https://api.example.com/cosmos/bank/v1beta1");
        l3.BaseUri.AbsoluteUri.Should().Be("https://api.example.com/cosmos/bank/v1beta1/balances");
    }

    // ─────────────────────────────────────────────────────────────────
    // Test doubles
    // ─────────────────────────────────────────────────────────────────

    private sealed class TestRestApi : BaseRestApi
    {
        public TestRestApi(string baseUri, string? path = null) : base(baseUri, path) { }
        public TestRestApi(Uri baseUri, string? path = null) : base(baseUri, path) { }
    }

    /// <summary>
    /// Concrete subclass that adds GetAsync / PostAsync by wiring an HttpClient
    /// to the <see cref="BaseRestApi.BaseUri"/> resolved by the primary constructor.
    /// </summary>
    private sealed class HttpTestRestApi : BaseRestApi
    {
        private readonly HttpClient _client;

        public HttpTestRestApi(string baseUri, string path, HttpMessageHandler handler)
            : base(baseUri, path)
        {
            _client = new HttpClient(handler);
        }

        public Task<HttpResponseMessage> GetAsync()
            => _client.GetAsync(BaseUri);

        public Task<HttpResponseMessage> PostAsync(HttpContent content)
            => _client.PostAsync(BaseUri, content);
    }
}
