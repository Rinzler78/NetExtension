using System;

namespace Rinzler78.NetExtension;

public static class UriHelper
{
    public static string ToFullEndpoint(this Uri uri)
    {
        var fullEndpoint = $"{uri.Scheme}://{uri.Host}:{uri.Port}{uri.AbsolutePath}";

        return fullEndpoint.EndsWith("/", StringComparison.OrdinalIgnoreCase) ? fullEndpoint.Substring(0, fullEndpoint.Length - 1) : fullEndpoint;
    }
}
