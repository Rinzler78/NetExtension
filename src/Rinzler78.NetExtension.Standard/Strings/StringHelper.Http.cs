using System;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Rinzler78.NetExtension.Strings;

public static partial class StringHelper
{
    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(DefaultHttpTimeoutSeconds)
    };

    public static async Task<Stream> HttpGetStreamAsync(this string url, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ValidateUrl(url);

        if (timeout.HasValue)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout.Value);
            return await HttpClient.GetStreamAsync(url, cts.Token).ConfigureAwait(false);
        }

        return await HttpClient.GetStreamAsync(url, cancellationToken).ConfigureAwait(false);
    }

    public static async Task<string> HttpGetStringAsync(this string url, TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        ValidateUrl(url);

        if (timeout.HasValue)
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(timeout.Value);
            return await HttpClient.GetStringAsync(url, cts.Token).ConfigureAwait(false);
        }

        return await HttpClient.GetStringAsync(url, cancellationToken).ConfigureAwait(false);
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
            httpResponse = await HttpClient.PostAsync(url, obj?.GetStringContent(), cts.Token)
                .ConfigureAwait(false);
        }
        else
        {
            httpResponse = await HttpClient.PostAsync(url, obj?.GetStringContent(), cancellationToken)
                .ConfigureAwait(false);
        }

        var result = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);

#if SHOW_HTTP_TRACE
        strb.AppendLine($"Answer ({url}) :");
        strb.AppendLine(result);
        Console.WriteLine(strb.ToString());
#endif
        return result;
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
        if (string.Equals(host, LocalhostName, StringComparison.Ordinal) ||
            string.Equals(host, LocalhostIpv4, StringComparison.Ordinal) ||
            string.Equals(host, LocalhostIpv6, StringComparison.Ordinal))
        {
            throw new ArgumentException("Access to localhost is not allowed.", nameof(url));
        }

        if (System.Net.IPAddress.TryParse(host, out var ipAddress))
        {
            if (ipAddress.AddressFamily == AddressFamily.InterNetworkV6)
            {
                if (ipAddress.Equals(System.Net.IPAddress.IPv6Loopback) ||
                    ipAddress.IsIPv6LinkLocal ||
                    ipAddress.IsIPv6SiteLocal ||
                    IsIPv6UniqueLocal(ipAddress))
                    throw new ArgumentException("Access to private network ranges is not allowed.", nameof(url));
            }
            else
            {
                var bytes = ipAddress.GetAddressBytes();
                if (bytes.Length == 4)
                {
                    if (bytes[0] == 0)
                        throw new ArgumentException("Access to private network ranges is not allowed.", nameof(url));

                    if (bytes[0] == ClassAPrivateFirstOctet)
                        throw new ArgumentException("Access to private network ranges is not allowed.", nameof(url));

                    if (bytes[0] == ClassBPrivateFirstOctet &&
                        bytes[1] >= ClassBPrivateSecondOctetMin &&
                        bytes[1] <= ClassBPrivateSecondOctetMax)
                        throw new ArgumentException("Access to private network ranges is not allowed.", nameof(url));

                    if (bytes[0] == ClassCPrivateFirstOctet &&
                        bytes[1] == ClassCPrivateSecondOctet)
                        throw new ArgumentException("Access to private network ranges is not allowed.", nameof(url));

                    if (bytes[0] == 169 && bytes[1] == 254)
                        throw new ArgumentException("Access to link-local addresses is not allowed.", nameof(url));

                    if (bytes[0] == 100 && bytes[1] >= 64 && bytes[1] <= 127)
                        throw new ArgumentException("Access to CGNAT address space is not allowed.", nameof(url));
                }
            }
        }
    }

    private static bool IsIPv6UniqueLocal(System.Net.IPAddress ipAddress)
    {
        var bytes = ipAddress.GetAddressBytes();
        return bytes.Length == 16 && (bytes[0] & 0xFE) == 0xFC;
    }
}
