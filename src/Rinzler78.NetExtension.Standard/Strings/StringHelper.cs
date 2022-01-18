using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rinzler78.NetExtension.Strings
{
    public static class StringHelper
    {
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
            string sample = string.Join("", str?.Select(c => Char.IsLetterOrDigit(c) ? c.ToString().ToLower() : "_").ToArray());

            return string.Join("", sample?
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => $"{s.Substring(0, 1).ToUpper()}{s.Substring(1)}"));
        }

        public static IEnumerable<string> MakeAllCombinatiions(this IEnumerable<string> allStrings)
        {
            var list = new List<String>();

            if ((allStrings?.Count() ?? 0) > 0)
                foreach (var leftStr in allStrings)
                    foreach (var rightStr in allStrings)
                        list.Add($"{leftStr}{rightStr}");

            return list;
        }

        public static bool IsNullOrEmpty(this string str) => string.IsNullOrEmpty(str);
        public static string Join(this string[] strs, char separator) => strs?.Length > 0 ? string.Join(separator, strs) : null;

        public static string HttpGetString(this string url)
        {
#if DEBUG
            var strb = new StringBuilder();
            strb.AppendLine($"Http Get ({url}) :");
#endif
            //return new WebClient().DownloadString(url);
            var result = new HttpClient().GetStringAsync(url).Result;

#if DEBUG
            strb.AppendLine($"Answer ({url}) :");
            strb.AppendLine($"{result}");
            Console.WriteLine(strb.ToString());
#endif
            return result;
        }

        public static StringContent GetStringContent(this object obj)
        {
            var jsonContent = JsonConvert.SerializeObject(obj);
            var contentString = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            //contentString.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return contentString;
        }

        public static object HttpGet(this string url)
            => JsonConvert.DeserializeObject(url.HttpGetString());

        public static ReturnType HttpGet<ReturnType>(this string url)
            => JsonConvert.DeserializeObject<ReturnType>(url.HttpGetString());

        public static string HttpPostString<RequestType>(this string url, RequestType obj)
        {
#if DEBUG
            var strb = new StringBuilder();
            strb.AppendLine($"Http Post Request ({url}):");

            strb.AppendLine($"Payload :");
            strb.AppendLine($"{JsonConvert.SerializeObject(obj)}");
#endif
            var httpResponse = new HttpClient().PostAsync(url, obj.GetStringContent()).Result;
            var result = httpResponse.Content.ReadAsStringAsync().Result;
#if DEBUG
            strb.AppendLine($"Answer ({url}) :");
            strb.AppendLine($"{result}");
            Console.WriteLine(strb.ToString());
#endif
            return result;
        }

        public static ReturnType HttpPostString<ReturnType>(this string url, string obj)
            => JsonConvert.DeserializeObject<ReturnType>(url.HttpPostString(obj));

        public static object HttpPost<RequestType>(this string url, RequestType obj)
            => JsonConvert.DeserializeObject(url.HttpPostString(obj));

        public static ReturnType HttpPost<RequestType, ReturnType>(this string url, RequestType obj)
        {
            var str = url.HttpPostString(obj);
            return JsonConvert.DeserializeObject<ReturnType>(str);
        }

        public static string ToJsonFormatedString(this string jsonString)
        {
            JToken jt = JToken.Parse(jsonString);
            return jt.ToString(Formatting.Indented);
        }

        public static string GetBytesReadable(this int i)
            => ((long)i).GetBytesReadable();

        public static string GetBytesReadable(this long i)
        {
            // Get absolute value
            long absolute_i = (i < 0 ? -i : i);
            // Determine the suffix and readable value
            string suffix;
            double readable;
            if (absolute_i >= 0x1000000000000000) // Exabyte
            {
                suffix = "EB";
                readable = (i >> 50);
            }
            else if (absolute_i >= 0x4000000000000) // Petabyte
            {
                suffix = "PB";
                readable = (i >> 40);
            }
            else if (absolute_i >= 0x10000000000) // Terabyte
            {
                suffix = "TB";
                readable = (i >> 30);
            }
            else if (absolute_i >= 0x40000000) // Gigabyte
            {
                suffix = "GB";
                readable = (i >> 20);
            }
            else if (absolute_i >= 0x100000) // Megabyte
            {
                suffix = "MB";
                readable = (i >> 10);
            }
            else if (absolute_i >= 0x400) // Kilobyte
            {
                suffix = "KB";
                readable = i;
            }
            else
            {
                return i.ToString("0 B"); // Byte
            }
            // Divide by 1024 to get fractional value
            readable = (readable / 1024);
            // Return formatted number with suffix
            return readable.ToString("0.### ") + suffix;
        }
    }
}
