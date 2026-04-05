using System;
using System.Collections.Generic;
using System.Runtime.ExceptionServices;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Strings;

public static partial class StringHelper
{
    private static readonly object HostAddressResolverSync = new();
    private static Func<string, IPAddress[]> _hostAddressResolver = Dns.GetHostAddresses;

    private static readonly HttpClient HttpClient = CreateHttpClient();

    public static async Task<Stream> HttpGetStreamAsync(this string url, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ValidateUrl(url);

        if (timeout.HasValue)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout.Value);
            return await ExecuteHttpCall(() => HttpClient.GetStreamAsync(url, cts.Token)).ConfigureAwait(false);
        }

        return await ExecuteHttpCall(() => HttpClient.GetStreamAsync(url, cancellationToken)).ConfigureAwait(false);
    }

    public static async Task<string> HttpGetStringAsync(this string url, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ValidateUrl(url);

        if (timeout.HasValue)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout.Value);
            return await ExecuteHttpCall(() => HttpClient.GetStringAsync(url, cts.Token)).ConfigureAwait(false);
        }

        return await ExecuteHttpCall(() => HttpClient.GetStringAsync(url, cancellationToken)).ConfigureAwait(false);
    }

    public static async Task<ReturnType> HttpGetAsync<ReturnType>(this string url,
        JsonSerializerSettings? settings = null, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        var stream = await url.HttpGetStreamAsync(timeout, cancellationToken).ConfigureAwait(false);
        await using (stream.ConfigureAwait(false))
        using (var streamReader = new StreamReader(stream))
        using (JsonReader reader = new JsonTextReader(streamReader))
        {
            var serializer = JsonSerializer.Create(settings);
            return serializer.Deserialize<ReturnType>(reader)
                   ?? throw new InvalidOperationException(
                       "Failed to deserialize response to the specified type.");
        }
    }

    public static async Task<ReturnType> HttpGetAsync<ImplementationType, ReturnType>(this string url,
        JsonSerializerSettings? settings = null, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
        where ImplementationType : ReturnType
    {
        return await url.HttpGetAsync<ImplementationType>(settings, timeout, cancellationToken)
            .ConfigureAwait(false);
    }

    public static async Task<string> HttpPostString<RequestType>(this string url, RequestType? obj,
        TimeSpan? timeout = null, CancellationToken cancellationToken = default)
    {
        ValidateUrl(url);

#if SHOW_HTTP_TRACE
        var strb = new StringBuilder();
        strb.AppendLine($"Http Post Request ({url}):");
        strb.AppendLine("Payload :");
        strb.AppendLine(JsonConvert.SerializeObject(obj));
#endif

        HttpResponseMessage httpResponse;
        if (timeout.HasValue)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout.Value);
            httpResponse = await ExecuteHttpCall(() => HttpClient.PostAsync(url, obj?.GetStringContent(), cts.Token))
                .ConfigureAwait(false);
        }
        else
        {
            httpResponse = await ExecuteHttpCall(() => HttpClient.PostAsync(url, obj?.GetStringContent(), cancellationToken))
                .ConfigureAwait(false);
        }

        using (httpResponse)
        {
            var result = await httpResponse.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

#if SHOW_HTTP_TRACE
            strb.AppendLine($"Answer ({url}) :");
            strb.AppendLine(result);
            Console.WriteLine(strb.ToString());
#endif
            return result;
        }
    }

    public static async Task<ReturnType> HttpPost<RequestType, ImplementationType, ReturnType>(
        this string url, RequestType obj, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
        where ImplementationType : ReturnType
        where ReturnType : class
    {
        var response = await url.HttpPostString(obj, timeout, cancellationToken).ConfigureAwait(false);
        return JsonConvert.DeserializeObject<ImplementationType>(response) as ReturnType
               ?? throw new InvalidOperationException(
                   "Failed to deserialize response to the specified type.");
    }

    // ── Test-only whitelist ───────────────────────────────────────────────────────────────────
    // Allows integration tests to route requests to a local WireMock server while the SSRF
    // guard remains active for all other callers.  Register via AllowLocalhostEndpointForTesting.

    private static readonly HashSet<string> AllowedLocalhostEndpoints =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly object AllowedLocalhostSync = new();

    /// <summary>
    /// Adds <paramref name="host"/>:<paramref name="port"/> to the SSRF bypass whitelist so
    /// that calls to that endpoint skip URL validation and connect directly via 127.0.0.1.
    /// Returns a disposable that removes the entry when disposed.  Test use only.
    /// </summary>
    internal static IDisposable AllowLocalhostEndpointForTesting(string host, int port)
    {
        var key = MakeEndpointKey(host, port);
        lock (AllowedLocalhostSync)
            AllowedLocalhostEndpoints.Add(key);
        return new LocalhostEndpointOverride(host, port);
    }

    private static string MakeEndpointKey(string host, int port) =>
        $"{host.ToLowerInvariant()}:{port}";

    private static bool IsAllowedLocalhostEndpoint(string host, int port)
    {
        lock (AllowedLocalhostSync)
            return AllowedLocalhostEndpoints.Contains(MakeEndpointKey(host, port));
    }

    private sealed class LocalhostEndpointOverride : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private bool _disposed;

        public LocalhostEndpointOverride(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public void Dispose()
        {
            if (_disposed) return;
            lock (AllowedLocalhostSync)
                AllowedLocalhostEndpoints.Remove(MakeEndpointKey(_host, _port));
            _disposed = true;
        }
    }

    private static void ValidateUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("URL cannot be null or empty.", nameof(url));

        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
            throw new ArgumentException("Invalid URL format.", nameof(url));

        if (!string.Equals(uri.Scheme, Uri.UriSchemeHttp, StringComparison.Ordinal) &&
            !string.Equals(uri.Scheme, Uri.UriSchemeHttps, StringComparison.Ordinal))
        {
            throw new ArgumentException("Only HTTP and HTTPS schemes are allowed.", nameof(url));
        }

        var host = uri.Host.ToLowerInvariant();
        // Test whitelist: explicitly allowed localhost endpoints bypass all SSRF checks.
        if (IsAllowedLocalhostEndpoint(host, uri.Port)) return;

        if (string.Equals(host, LocalhostName, StringComparison.Ordinal) ||
            string.Equals(host, LocalhostIpv4, StringComparison.Ordinal) ||
            string.Equals(host, LocalhostIpv6, StringComparison.Ordinal))
        {
            throw new ArgumentException("Access to localhost is not allowed.", nameof(url));
        }

        if (IPAddress.TryParse(host, out var ipAddress))
        {
            ThrowIfBlockedAddress(ipAddress, nameof(url));
        }
    }

    internal static IDisposable OverrideHostAddressResolverForTesting(Func<string, IPAddress[]> resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);

        lock (HostAddressResolverSync)
        {
            var previousResolver = _hostAddressResolver;
            _hostAddressResolver = resolver;
            return new HostAddressResolverOverride(previousResolver);
        }
    }

    private static void ThrowIfBlockedAddress(IPAddress ipAddress, string paramName)
    {
        if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
        {
            if (ipAddress.Equals(IPAddress.IPv6Loopback) ||
                ipAddress.IsIPv6LinkLocal ||
                ipAddress.IsIPv6SiteLocal ||
                IsIPv6UniqueLocal(ipAddress))
            {
                throw new ArgumentException("Access to private network ranges is not allowed.", paramName);
            }

            return;
        }

        var bytes = ipAddress.GetAddressBytes();
        if (bytes.Length != 4)
            return;

        if (bytes[0] == 0)
            throw new ArgumentException("Access to private network ranges is not allowed.", paramName);

        if (bytes[0] == 127)
            throw new ArgumentException("Access to localhost is not allowed.", paramName);

        if (bytes[0] == ClassAPrivateFirstOctet)
            throw new ArgumentException("Access to private network ranges is not allowed.", paramName);

        if (bytes[0] == ClassBPrivateFirstOctet &&
            bytes[1] >= ClassBPrivateSecondOctetMin &&
            bytes[1] <= ClassBPrivateSecondOctetMax)
        {
            throw new ArgumentException("Access to private network ranges is not allowed.", paramName);
        }

        if (bytes[0] == ClassCPrivateFirstOctet &&
            bytes[1] == ClassCPrivateSecondOctet)
        {
            throw new ArgumentException("Access to private network ranges is not allowed.", paramName);
        }

        if (bytes[0] == 169 && bytes[1] == 254)
            throw new ArgumentException("Access to link-local addresses is not allowed.", paramName);

        if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127)
            throw new ArgumentException("Access to CGNAT address space is not allowed.", paramName);
    }

    private static bool IsIPv6UniqueLocal(System.Net.IPAddress ipAddress)
    {
        var bytes = ipAddress.GetAddressBytes();
        return bytes.Length == 16 && (bytes[0] & 0xFE) == 0xFC;
    }

    private static HttpClient CreateHttpClient()
    {
        var handler = new SocketsHttpHandler
        {
            ConnectCallback = ConnectAsync
        };

        return new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(DefaultHttpTimeoutSeconds)
        };
    }

    private static async ValueTask<Stream> ConnectAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        var dnsHost = context.DnsEndPoint.Host;
        var dnsPort = context.DnsEndPoint.Port;

        // Test whitelist: connect directly to loopback, bypassing SSRF DNS checks.
        if (IsAllowedLocalhostEndpoint(dnsHost, dnsPort))
        {
            var loopbackSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                await loopbackSocket.ConnectAsync(new IPEndPoint(IPAddress.Loopback, dnsPort), cancellationToken)
                    .ConfigureAwait(false);
                return new NetworkStream(loopbackSocket, ownsSocket: true);
            }
            catch
            {
                loopbackSocket.Dispose();
                throw;
            }
        }

        var addresses = ResolveHostAddresses(dnsHost);
        SocketException? lastSocketException = null;

        foreach (var address in addresses)
        {
            ThrowIfBlockedAddress(address, nameof(context));

            var socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                await socket.ConnectAsync(new IPEndPoint(address, context.DnsEndPoint.Port), cancellationToken)
                    .ConfigureAwait(false);
                return new NetworkStream(socket, ownsSocket: true);
            }
            catch (SocketException ex)
            {
                lastSocketException = ex;
                socket.Dispose();
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }

        throw lastSocketException ?? new SocketException((int)SocketError.HostNotFound);
    }

    private static async Task<T> ExecuteHttpCall<T>(Func<Task<T>> httpCall)
    {
        try
        {
            return await httpCall().ConfigureAwait(false);
        }
        catch (HttpRequestException ex) when (ex.InnerException is ArgumentException argumentException)
        {
            ExceptionDispatchInfo.Capture(argumentException).Throw();
            throw;
        }
    }

    private static IPAddress[] ResolveHostAddresses(string host)
    {
        Func<string, IPAddress[]> resolver;
        lock (HostAddressResolverSync)
            resolver = _hostAddressResolver;

        return resolver(host);
    }

    private sealed class HostAddressResolverOverride : IDisposable
    {
        private readonly Func<string, IPAddress[]> _previousResolver;
        private bool _disposed;

        public HostAddressResolverOverride(Func<string, IPAddress[]> previousResolver)
        {
            _previousResolver = previousResolver;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            lock (HostAddressResolverSync)
                _hostAddressResolver = _previousResolver;

            _disposed = true;
        }
    }
}
