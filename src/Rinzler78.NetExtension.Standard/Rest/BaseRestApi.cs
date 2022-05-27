using System;
using System.Linq;

namespace Rinzler78.NetExtension.Rest;

public abstract class BaseRestApi
{
    public readonly Uri _baseUri;

    protected BaseRestApi(string baseUri, string path) : this(new Uri(baseUri), path)
    {
    }

    protected BaseRestApi(Uri baseUri, string path)
    {
        if (!baseUri.AbsoluteUri.EndsWith("/"))
            baseUri = new Uri($"{baseUri.AbsoluteUri}/");

        if (!path.EndsWith("/"))
            path = $"{path}/";

        _baseUri = new Uri(baseUri.AbsoluteUri + path);
    }

    protected string CreateUrlPath(string baseUrl, object[] args, string[] argsNames)
    {
        if (args != null && argsNames != null && args.Length == argsNames.Length)
            if (args.Any(o => o != null))
            {
                baseUrl += "?";

                for (var i = 0; i < args.Length; ++i)
                {
                    var value = args[i];
                    var name = argsNames[i];

                    baseUrl += $"{name}={value}";

                    if (i + 1 < args.Length)
                        baseUrl += "&";
                }
            }

        return baseUrl;
    }
}