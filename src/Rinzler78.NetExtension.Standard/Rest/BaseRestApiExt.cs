using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;

namespace Rinzler78.NetExtension.Rest;

/// <summary>
/// Provides extension methods for building REST API URL paths with query parameters.
/// </summary>
public static class BaseRestApiExt
{
    /// <summary>
    /// Appends URL-encoded query parameters to the base URL string.
    /// </summary>
    /// <param name="baseUrl">The base URL to append parameters to.</param>
    /// <param name="args">Optional dictionary of query parameter key-value pairs.</param>
    /// <returns>The base URL with appended query string, or the original URL if no parameters are provided.</returns>
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
