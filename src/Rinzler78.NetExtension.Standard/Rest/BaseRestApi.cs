using System;

namespace Rinzler78.NetExtension.Rest;

public abstract class BaseRestApi
{
    public Uri BaseUri { get; }

    protected BaseRestApi(string baseUri, string? path = null)
        : this(new Uri(baseUri), path)
    {
    }

    protected BaseRestApi(Uri baseUri, string? path = null)
    {
        ArgumentNullException.ThrowIfNull(baseUri);
        BaseUri = path is null
            ? baseUri
            : new Uri(baseUri, path.TrimStart('/'));
    }
}
