using System;
using System.Linq;

namespace Rinlzer78.NetExtension.String
{
    public static class StringHelper
    {
        public static string BeginByLowerCase(this string str)
        {
            if (str?.Length > 0)
                return $"{char.ToLower(str[0])}{(str.Length > 1 ? str.Substring(1) : "")}";

            return null;
        }

        public static string ToPascalCase(this string str)
        {
            string sample = string.Join("", str?.Select(c => Char.IsLetterOrDigit(c) ? c.ToString().ToLower() : "_").ToArray());

            return string.Join("", sample?
                .Split(new[] { '_' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => $"{s.Substring(0, 1).ToUpper()}{s.Substring(1)}"));
        }
    }
}
