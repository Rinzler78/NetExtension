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
    // Empty / whitespace path
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithEmptyStringPath_ShouldAppendTrailingSlash()
    {
        // "" is not null, so the concat branch executes: base + "/" + ""
        var api = new TestRestApi("https://api.example.com", "");

        api.BaseUri.AbsoluteUri.Should().Be("https://api.example.com/");
    }

    // ─────────────────────────────────────────────────────────────────
    // Slash pattern normalization
    // ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("https://api.example.com", "///path", "https://api.example.com/path")]
    [InlineData("https://api.example.com", "v1/customers/", "https://api.example.com/v1/customers/")]
    [InlineData("https://api.example.com/", "///path", "https://api.example.com/path")]
    [InlineData("https://api.example.com", "customers", "https://api.example.com/customers")]
    public void Constructor_WithVariousSlashPatterns_ShouldNormalize(
        string baseUri, string path, string expected)
    {
        var api = new TestRestApi(baseUri, path);

        api.BaseUri.AbsoluteUri.Should().Be(expected);
    }

    // ─────────────────────────────────────────────────────────────────
    // Port preservation
    // ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("https://api.example.com:8443", "v1/data", "https://api.example.com:8443/v1/data")]
    [InlineData("http://localhost:5000", "api/health", "http://localhost:5000/api/health")]
    [InlineData("http://localhost:5000/", "api/health", "http://localhost:5000/api/health")]
    public void Constructor_WithPortInBaseUri_ShouldPreservePort(
        string baseUri, string path, string expected)
    {
        var api = new TestRestApi(baseUri, path);

        api.BaseUri.AbsoluteUri.Should().Be(expected);
    }

    // ─────────────────────────────────────────────────────────────────
    // Encoded / special characters in path
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithPreEncodedPath_ShouldPreserveEncoding()
    {
        var api = new TestRestApi("https://api.example.com", "v1/my%20resource");

        api.BaseUri.AbsoluteUri.Should().Contain("my%20resource");
    }

    [Fact]
    public void Constructor_WithSpaceInPath_ShouldEncodeSpace()
    {
        var api = new TestRestApi("https://api.example.com", "v1/file name");

        // Uri constructor percent-encodes spaces
        api.BaseUri.AbsoluteUri.Should().Contain("file%20name");
    }

    // ─────────────────────────────────────────────────────────────────
    // Deep chaining (6 levels)
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_DeeplyChainedConstruction_ShouldPreserveAllSegments()
    {
        var root = new TestRestApi("https://api.example.com/", "cosmos");
        var l1 = new TestRestApi(root.BaseUri, "bank");
        var l2 = new TestRestApi(l1.BaseUri, "v1beta1");
        var l3 = new TestRestApi(l2.BaseUri, "balances");
        var l4 = new TestRestApi(l3.BaseUri, "address123");
        var l5 = new TestRestApi(l4.BaseUri, "spendable");

        l5.BaseUri.AbsoluteUri.Should().Be(
            "https://api.example.com/cosmos/bank/v1beta1/balances/address123/spendable");
    }

    [Fact]
    public void Constructor_ChainedWithTrailingSlashInBase_ShouldPreserveAllSegments()
    {
        var root = new TestRestApi("https://api.example.com/", "api");
        var l1 = new TestRestApi(new Uri(root.BaseUri.AbsoluteUri + "/"), "v2");
        var l2 = new TestRestApi(l1.BaseUri, "users");

        l2.BaseUri.AbsoluteUri.Should().Be("https://api.example.com/api/v2/users");
    }

    // ─────────────────────────────────────────────────────────────────
    // Constructor overload equivalence
    // ─────────────────────────────────────────────────────────────────

    [Theory]
    [InlineData("https://api.example.com", "v1/customers")]
    [InlineData("https://api.example.com/", "v1/customers")]
    [InlineData("https://api.example.com:8443", "v1/data")]
    public void Constructor_StringAndUriOverloads_ShouldProduceEquivalentUri(
        string baseUri, string path)
    {
        var fromString = new TestRestApi(baseUri, path);
        var fromUri = new TestRestApi(new Uri(baseUri), path);

        fromString.BaseUri.AbsoluteUri.Should().Be(fromUri.BaseUri.AbsoluteUri);
    }

    // ─────────────────────────────────────────────────────────────────
    // Invalid / malformed base URI
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithRelativeBaseUriString_ShouldThrowUriFormatException()
    {
        Action act = () => new TestRestApi("not-a-uri", "v1");

        act.Should().Throw<UriFormatException>();
    }

    [Fact]
    public void Constructor_WithEmptyBaseUriString_ShouldThrowUriFormatException()
    {
        Action act = () => new TestRestApi("", "v1");

        act.Should().Throw<UriFormatException>();
    }

    // ─────────────────────────────────────────────────────────────────
    // Known limitations — query string / fragment in base URI
    // ─────────────────────────────────────────────────────────────────

    [Fact]
    public void Constructor_WithQueryStringInBaseUri_ShouldAppendPathAfterQuery()
    {
        // Known limitation: path is appended via string concat after the query string.
        var api = new TestRestApi("https://api.example.com/base?v=2", "v1/data");

        // The resulting URI is technically malformed, but this documents current behavior.
        api.BaseUri.AbsoluteUri.Should().Contain("v1/data");
    }

    [Fact]
    public void Constructor_WithFragmentInBaseUri_ShouldAppendPathAfterFragment()
    {
        // Known limitation: fragments are included in AbsoluteUri, path appends after them.
        var api = new TestRestApi("https://api.example.com/docs#section", "v1/data");

        api.BaseUri.AbsoluteUri.Should().Contain("v1/data");
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
