#if DEBUG
//#define SHOW_HTTP_TRACE
#endif

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rinzler78.NetExtension.Strings;

public static class StringHelper
{
    //        public static async Task<string> HttpGetString(this string url)
    //        {
    //#if SHOW_HTTP_TRACE
    //            var strb = new StringBuilder();
    //            strb.AppendLine($"Http Get ({url}) :");
    //#endif
    //            //return new WebClient().DownloadString(url);
    //            var result = await new HttpClient().GetStringAsync(url).ConfigureAwait(false);

    //#if SHOW_HTTP_TRACE
    //            strb.AppendLine($"Answer ({url}) :");
    //            strb.AppendLine($"{result}");
    //            Console.WriteLine(strb.ToString());
    //#endif
    //            return result;
    //        }

    private static readonly HttpClient HttpClient = new();

    public static bool IsValidEmail(this string str)
    {
        var emailAddressAttribute = new EmailAddressAttribute();
        return emailAddressAttribute.IsValid(str);
    }

    public static int ComputeLevenshteInDistance(this string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return string.IsNullOrEmpty(target) ? 0 : target.Length;

        if (string.IsNullOrEmpty(target))
            return string.IsNullOrEmpty(source) ? 0 : source.Length;

        var sourceLength = source.Length;
        var targetLength = target.Length;

        var distance = new int[sourceLength + 1, targetLength + 1];

        // Step 1
        for (var i = 0; i <= sourceLength; distance[i, 0] = i++)
        {
        }

        for (var j = 0; j <= targetLength; distance[0, j] = j++)
        {
        }

        for (var i = 1; i <= sourceLength; i++)
        for (var j = 1; j <= targetLength; j++)
        {
            // Step 2
            var cost = target[j - 1] == source[i - 1] ? 0 : 1;

            // Step 3
            distance[i, j] = System.Math.Min(
                System.Math.Min(distance[i - 1, j] + 1, distance[i, j - 1] + 1),
                distance[i - 1, j - 1] + cost);
        }

        return distance[sourceLength, targetLength];
    }

    public static double CalculateSimilarity(this string source, string target)
    {
        if (string.IsNullOrEmpty(source))
            return string.IsNullOrEmpty(target) ? 1 : 0;

        if (string.IsNullOrEmpty(target))
            return string.IsNullOrEmpty(source) ? 1 : 0;

        double stepsToSame = ComputeLevenshteInDistance(source, target);
        return 1.0 - stepsToSame / System.Math.Max(source.Length, target.Length);
    }

    public static bool ContainsAll(this string str, string[] words)
    {
        return words?.All(arg => str.Contains(arg, StringComparison.Ordinal)) ?? true;
    }

    public static bool ContainsAny(this string str, string[] words)
    {
        return words?.Any(arg => str.Contains(arg, StringComparison.Ordinal)) ?? true;
    }

    public static string BeginByLowerCase(this string str)
    {
        if (str?.Length > 0)
            return $"{char.ToLower(str[0])}{(str.Length > 1 ? str.Substring(1) : "")}";

        return null;
    }

    public static string BeginByUpperCase(this string str)
    {
        if (str?.Length > 0)
            return $"{char.ToUpper(str[0])}{(str.Length > 1 ? str.Substring(1) : "")}";

        return null;
    }

    public static string ToStartByUpperCase(this string str)
    {
        return str.ToLower().BeginByUpperCase();
    }

    public static string ToPascalCase(this string str)
    {
        var sample = string.Join("",
            str?.Select(c => char.IsLetterOrDigit(c) ? c.ToString().ToLower() : "_").ToArray());

        return string.Join("", sample?
            .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => $"{s.Substring(0, 1).ToUpper()}{s.Substring(1)}"));
    }

    public static IEnumerable<string> MakeAllCombinatiions(this IEnumerable<string> allStrings)
    {
        var list = new List<string>();

        if ((allStrings?.Count() ?? 0) > 0)
            foreach (var leftStr in allStrings)
            foreach (var rightStr in allStrings)
                list.Add($"{leftStr}{rightStr}");

        return list;
    }

    public static bool IsNullOrEmpty(this string str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static string Join(this string[] strs, char separator)
    {
        return strs?.Length > 0 ? string.Join(separator, strs) : null;
    }

    public static Task<Stream> HttpGetStreamAsync(this string url)
    {
        return HttpClient.GetStreamAsync(url);
    }

    public static Task<string> HttpGetStringAsync(this string url)
    {
        return HttpClient.GetStringAsync(url);
    }

    public static StringContent GetStringContent(this object obj)
    {
        var jsonContent = JsonConvert.SerializeObject(obj);
        var contentString = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        //contentString.Headers.ContentType = new MediaTypeHeaderValue("application/json");

        return contentString;
    }

    public static async Task<object> HttpGetAsync(this string url)
    {
        await using (var s = await url.HttpGetStreamAsync().ConfigureAwait(false))
        using (var sr = new StreamReader(s))
        using (JsonReader reader = new JsonTextReader(sr))
        {
            var serializer = JsonSerializer.Create();
            var obj = serializer.Deserialize(reader);
            return obj;
        }
    }

    public static async Task<object> HttpGetAsync(this string url, JsonSerializerSettings settings)
    {
        await using (var s = await url.HttpGetStreamAsync().ConfigureAwait(false))
        using (var sr = new StreamReader(s))
        using (JsonReader reader = new JsonTextReader(sr))
        {
            var serializer = JsonSerializer.Create(settings);
            var obj = serializer.Deserialize(reader);
            return obj;
        }
    }

    public static async Task<ReturnType> HttpGetAsync<ReturnType>(this string url)
    {
        await using (var s = await url.HttpGetStreamAsync().ConfigureAwait(false))
        using (var sr = new StreamReader(s))
        using (JsonReader reader = new JsonTextReader(sr))
        {
            var serializer = JsonSerializer.Create();
            var obj = serializer.Deserialize<ReturnType>(reader);
            return obj;
        }
    }

    public static async Task<ReturnType> HttpGetAsync<ReturnType>(this string url, JsonSerializerSettings settings)
    {
        await using (var s = await url.HttpGetStreamAsync().ConfigureAwait(false))
        using (var sr = new StreamReader(s))
        using (JsonReader reader = new JsonTextReader(sr))
        {
            var serializer = JsonSerializer.Create(settings);
            var obj = serializer.Deserialize<ReturnType>(reader);
            return obj;
        }
    }

    public static Task<string> HttpPostString<RequestType>(this string url, RequestType obj)
    {
        return Task.Run(async () =>
        {
#if SHOW_HTTP_TRACE
            var strb = new StringBuilder();
            strb.AppendLine($"Http Post Request ({url}):");

            strb.AppendLine($"Payload :");
            strb.AppendLine($"{JsonConvert.SerializeObject(obj)}");
#endif
            var httpResponse = await HttpClient.PostAsync(url, obj.GetStringContent()).ConfigureAwait(false);
            var result = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
#if SHOW_HTTP_TRACE
            strb.AppendLine($"Answer ({url}) :");
            strb.AppendLine($"{result}");
            Console.WriteLine(strb.ToString());
#endif
            return result;
        });
    }

    public static async Task<ReturnType> HttpPostString<ReturnType>(this string url, string obj)
    {
        return JsonConvert.DeserializeObject<ReturnType>(await url.HttpPostString(obj).ConfigureAwait(false));
    }

    public static async Task<object> HttpPost<RequestType>(this string url, RequestType obj)
    {
        return JsonConvert.DeserializeObject(await url.HttpPostString(obj).ConfigureAwait(false));
    }

    //public static Task<ReturnType> HttpPost<RequestType, ReturnType>(this string url, RequestType obj)
    //    => Task.Run(async () =>
    //    {
    //        var str = await url.HttpPostString(obj).ConfigureAwait(false);
    //        return JsonConvert.DeserializeObject<ReturnType>(str);
    //    });

    public static string ToJsonFormatedString(this string jsonString)
    {
        var jt = JToken.Parse(jsonString);
        return jt.ToString(Formatting.Indented);
    }

    public static string GetBytesReadable(this int i)
    {
        return ((long)i).GetBytesReadable();
    }

    public static string GetBytesReadable(this long i)
    {
        // Get absolute value
        var absoluteI = i < 0 ? -i : i;
        // Determine the suffix and readable value
        string suffix;
        double readable;
        if (absoluteI >= 0x1000000000000000) // Exabyte
        {
            suffix = "EB";
            readable = i >> 50;
        }
        else if (absoluteI >= 0x4000000000000) // Petabyte
        {
            suffix = "PB";
            readable = i >> 40;
        }
        else if (absoluteI >= 0x10000000000) // Terabyte
        {
            suffix = "TB";
            readable = i >> 30;
        }
        else if (absoluteI >= 0x40000000) // Gigabyte
        {
            suffix = "GB";
            readable = i >> 20;
        }
        else if (absoluteI >= 0x100000) // Megabyte
        {
            suffix = "MB";
            readable = i >> 10;
        }
        else if (absoluteI >= 0x400) // Kilobyte
        {
            suffix = "KB";
            readable = i;
        }
        else
        {
            return i.ToString("0 B"); // Byte
        }

        // Divide by 1024 to get fractional value
        readable /= 1024;
        // Return formatted number with suffix
        return readable.ToString("0.### ") + suffix;
    }

    public static byte[] GetBytes(this string str)
    {
        return Encoding.ASCII.GetBytes(str);
    }
}