using System;

namespace Rinzler78.NetExtension.Rest;

public abstract class BaseRestApi
{
    public readonly Uri BaseUri;

    protected BaseRestApi(string baseUri, string? path = null)
        : this(new Uri(baseUri), path)
    {
    }

    protected BaseRestApi(Uri baseUri, string? path = null)
    {
        if (baseUri is null)
        {
            throw new ArgumentNullException(nameof(baseUri));
        }

        BaseUri = path is null ? baseUri : new Uri(baseUri.AbsoluteUri + (baseUri.AbsoluteUri.EndsWith('/') ? "" : "/") + path);
    }
}
