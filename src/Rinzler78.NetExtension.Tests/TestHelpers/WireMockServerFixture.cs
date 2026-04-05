using System;
using System.Net;
using WireMock.Server;
using Rinzler78.NetExtension.Strings;

namespace Rinzler78.NetExtension.Tests.TestHelpers;

/// <summary>
/// Shared xUnit fixture that starts an embedded WireMock HTTP server on a random port
/// and registers the server's endpoint in the SSRF whitelist so that integration tests
/// can reach it without triggering the ValidateUrl guard.
/// </summary>
public sealed class WireMockServerFixture : IDisposable
{
    private readonly IDisposable _resolverOverride;

    public WireMockServer Server { get; }

    /// <summary>Base URL of the embedded server, e.g. http://localhost:54321.</summary>
    public string BaseUrl => Server.Urls[0];

    public WireMockServerFixture()
    {
        Server = WireMockServer.Start();

        // Whitelist the WireMock endpoint so ValidateUrl lets it through and
        // ConnectAsync routes the socket directly to 127.0.0.1 instead of
        // going through SSRF-blocked DNS resolution.
        var serverUri = new Uri(Server.Urls[0]);
        _resolverOverride = StringHelper.AllowLocalhostEndpointForTesting(
            serverUri.Host, serverUri.Port);
    }

    public void Dispose()
    {
        Server.Stop();
        _resolverOverride.Dispose();
    }
}
