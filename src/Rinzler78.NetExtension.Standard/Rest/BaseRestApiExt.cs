using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Rinzler78.NetExtension.Rest;

public static class BaseRestApiExt
{
    public static string CreateUrlPath(this string baseUrl, IReadOnlyDictionary<string, object?>? args = null)
    {
        if (args?.Count > 0)
        {
            var argsString = string.Join("&", args.Where(a => a.Value is not null).Select(a =>
            {
                var key = WebUtility.UrlEncode(a.Key);
                var value = a.Value switch
                {
                    bool booleanValue => booleanValue ? "true" : "false",
                    IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                    _ => a.Value?.ToString() ?? string.Empty
                };

                return $"{key}={WebUtility.UrlEncode(value)}";
            }));

            if (argsString.Length > 0)
                baseUrl += $"?{argsString}";
        }

        return baseUrl;
    }
}
