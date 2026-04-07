using System;

namespace Rinzler78.NetExtension;

/// <summary>
/// Provides extension methods for <see cref="Uri"/> manipulation.
/// </summary>
public static class UriHelper
{
    /// <summary>
    /// Converts a URI to a full endpoint string in the format scheme://host:port/path, stripping any trailing slash.
    /// </summary>
    /// <param name="uri">The URI to convert.</param>
    /// <returns>The full endpoint string without a trailing slash.</returns>
    public static string ToFullEndpoint(this Uri uri)
    {
        var fullEndpoint = $"{uri.Scheme}://{uri.Host}:{uri.Port}{uri.AbsolutePath}";

        return fullEndpoint.EndsWith("/", StringComparison.Ordinal) ? fullEndpoint[..^1] : fullEndpoint;
    }
}
