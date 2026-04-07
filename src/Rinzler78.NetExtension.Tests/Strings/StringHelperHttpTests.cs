using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Rinzler78.NetExtension.Strings;
using Rinzler78.NetExtension.Tests.TestHelpers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using Xunit;
using FluentAssertions;

namespace Rinzler78.NetExtension.Tests.Strings;

[Collection(nameof(StringHelperHttpTestCollection))]
public class StringHelperHttpTests
{
    private readonly WireMockServerFixture _fixture;

    public StringHelperHttpTests(WireMockServerFixture fixture)
    {
        _fixture = fixture;
        // Reset any stubs from previous tests
        _fixture.Server.Reset();
    }

    // ── HttpGetStreamAsync ────────────────────────────────────────────

    [Fact]
    public async Task HttpGetStreamAsync_HappyPath_ReturnsStream()
    {
        _fixture.Server
            .Given(Request.Create().WithPath("/stream").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("hello stream"));

        var url = $"{_fixture.BaseUrl}/stream";
        await using var stream = await url.HttpGetStreamAsync();
        using var reader = new StreamReader(stream);
        var content = await reader.ReadToEndAsync();
        content.Should().Contain("hello stream");
    }

    [Fact]
    public async Task HttpGetStreamAsync_SsrfBlockedUrl_ThrowsArgumentException()
    {
        var act = async () => await "http://192.168.1.1/resource".HttpGetStreamAsync();
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HttpGetStreamAsync_TimeoutExpired_ThrowsOperationCanceledException()
    {
        _fixture.Server
            .Given(Request.Create().WithPath("/slow").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("late")
                .WithDelay(TimeSpan.FromSeconds(2)));

        var url = $"{_fixture.BaseUrl}/slow";
        var act = async () => await url.HttpGetStreamAsync(timeout: TimeSpan.FromMilliseconds(100));
        await act.Should().ThrowAsync<Exception>()
            .Where(e => e is OperationCanceledException || e is TaskCanceledException);
    }

    [Fact]
    public async Task HttpGetStreamAsync_PreCancelledToken_ThrowsOperationCanceledException()
    {
        _fixture.Server
            .Given(Request.Create().WithPath("/any").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("ok"));

        var url = $"{_fixture.BaseUrl}/any";
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var act = async () => await url.HttpGetStreamAsync(cancellationToken: cts.Token);
        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // ── HttpGetStringAsync (happy path + missing cases) ───────────────

    [Fact]
    public async Task HttpGetStringAsync_HappyPath_ReturnsBody()
    {
        _fixture.Server
            .Given(Request.Create().WithPath("/text").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("hello world"));

        var result = await $"{_fixture.BaseUrl}/text".HttpGetStringAsync();
        result.Should().Be("hello world");
    }

    [Fact]
    public async Task HttpGetStringAsync_LoopbackRangeAddress_ThrowsArgumentException()
    {
        // 127.x.x.x other than 127.0.0.1 — still blocked
        var act = async () => await "http://127.0.0.2/resource".HttpGetStringAsync();
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HttpGetStringAsync_DnsRebinding_ThrowsArgumentException()
    {
        // Override the resolver so rebind.example.com resolves to a private IP at connect time.
        using var rebindOverride = StringHelper.OverrideHostAddressResolverForTesting(
            host => host == "rebind.example.com"
                ? new[] { IPAddress.Parse("10.0.0.1") }        // private → blocked
                : new[] { IPAddress.Parse("93.184.216.34") });  // everything else is public

        var act = async () => await "http://rebind.example.com/resource".HttpGetStringAsync();
        await act.Should().ThrowAsync<ArgumentException>();
    }

    // ── HttpGetAsync<T> ───────────────────────────────────────────────

    private record SampleDto(string Name, int Value);

    [Fact]
    public async Task HttpGetAsync_HappyPath_ReturnsDeserializedObject()
    {
        var json = JsonConvert.SerializeObject(new SampleDto("test", 42));
        _fixture.Server
            .Given(Request.Create().WithPath("/dto").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(json)
                .WithHeader("Content-Type", "application/json"));

        var result = await $"{_fixture.BaseUrl}/dto".HttpGetAsync<SampleDto>();
        result.Name.Should().Be("test");
        result.Value.Should().Be(42);
    }

    [Fact]
    public async Task HttpGetAsync_NonDeserializableResponse_ThrowsInvalidOperationException()
    {
        // Return a JSON null — deserializer returns null, method should throw
        _fixture.Server
            .Given(Request.Create().WithPath("/null-dto").UsingGet())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("null")
                .WithHeader("Content-Type", "application/json"));

        var act = async () => await $"{_fixture.BaseUrl}/null-dto".HttpGetAsync<SampleDto>();
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    // ── HttpPostString<TReq> ─────────────────────────────────────────

    [Fact]
    public async Task HttpPostString_HappyPath_ReturnsResponseBody()
    {
        _fixture.Server
            .Given(Request.Create().WithPath("/post").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("posted ok"));

        var result = await $"{_fixture.BaseUrl}/post".HttpPostString(new { Key = "value" });
        result.Should().Be("posted ok");
    }

    [Fact]
    public async Task HttpPostString_SsrfBlockedUrl_ThrowsArgumentException()
    {
        var act = async () => await "http://10.0.0.1/post".HttpPostString(new { });
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task HttpPostString_NullPayload_SendsEmptyBodyAndReturnsResponse()
    {
        _fixture.Server
            .Given(Request.Create().WithPath("/post-null").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("empty ok"));

        string? payload = null;
        var result = await $"{_fixture.BaseUrl}/post-null".HttpPostString(payload);
        result.Should().Be("empty ok");
    }

    // ── HttpPost<TReq, TImpl, TReturn> ───────────────────────────────

    private interface IResult { string Status { get; } }
    private record ConcreteResult(string Status) : IResult;

    [Fact]
    public async Task HttpPost_HappyPath_ReturnsTypedResponse()
    {
        var json = JsonConvert.SerializeObject(new ConcreteResult("ok"));
        _fixture.Server
            .Given(Request.Create().WithPath("/typed-post").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody(json)
                .WithHeader("Content-Type", "application/json"));

        var result = await $"{_fixture.BaseUrl}/typed-post"
            .HttpPost<object, ConcreteResult, IResult>(new { });
        result.Status.Should().Be("ok");
    }

    [Fact]
    public async Task HttpPost_NonDeserializableResponse_ThrowsInvalidOperationException()
    {
        _fixture.Server
            .Given(Request.Create().WithPath("/bad-post").UsingPost())
            .RespondWith(Response.Create().WithStatusCode(200).WithBody("null")
                .WithHeader("Content-Type", "application/json"));

        var act = async () => await $"{_fixture.BaseUrl}/bad-post"
            .HttpPost<object, ConcreteResult, IResult>(new { });
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
