using System.Collections.Generic;
using System.Linq;

namespace Rinzler78.NetExtension.Rest;

public static class BaseRestApiExt
{
    public static string CreateUrlPath(this string baseUrl, IReadOnlyDictionary<string, object?>? args = null)
    {
        if (args?.Count > 0)
        {
            var argsString = string.Join("&", args.Where(a => a.Value is not null).Select(a => $"{a.Key}={a.Value}"));

            if (argsString.Length > 0)
                baseUrl += argsString;
        }

        return baseUrl;
    }
}