using System;

namespace Rinzler78.NetExtension.Rest;

public abstract class BaseRestApi
{
    public readonly Uri _baseUri;

    protected BaseRestApi(string baseUri, string path = null)
        : this(new Uri(baseUri), path)
    {
    }

    protected BaseRestApi(Uri baseUri, string path = null)
    {
        if (!baseUri.AbsoluteUri.EndsWith("/", StringComparison.Ordinal))
            baseUri = new Uri($"{baseUri.AbsoluteUri}/");

        if (path != null)
        {
            if (!path.EndsWith("/", StringComparison.Ordinal))
                path = $"{path}/";

            _baseUri = new Uri(baseUri.AbsoluteUri + path);
        }
        else
        {
            _baseUri = new Uri(baseUri.AbsoluteUri);
        }
    }
}