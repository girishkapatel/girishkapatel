using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VJLiabraries
{

    public static class ExtensionMethods
    {
        public static string ToDelimitedString<T>(this IEnumerable<T> source)
        {
            return source.ToDelimitedString(x => x.ToString(),
                CultureInfo.CurrentCulture.TextInfo.ListSeparator);
        }

        public static string ToDelimitedString<T>(
            this IEnumerable<T> source, Func<T, string> converter)
        {
            return source.ToDelimitedString(converter,
                CultureInfo.CurrentCulture.TextInfo.ListSeparator);
        }

        public static string ToDelimitedString<T>(
            this IEnumerable<T> source, string separator)
        {
            return source.ToDelimitedString(x => x.ToString(), separator);
        }

        public static string ToDelimitedString<T>(this IEnumerable<T> source,
            Func<T, string> converter, string separator)
        {
            return string.Join(separator, source.Select(converter).ToArray());
        }

        public static string RemoveWhitespace(this string source)
        {
            if (source == null) return source;
            return new string(source.Where(c => !Char.IsWhiteSpace(c)).ToArray());
        }

        public static async Task<string> ReadAsStringAsync(this IFormFile file)
        {
            var result = new StringBuilder();
            using (var reader = new StreamReader(file.OpenReadStream()))
            {
                while (reader.Peek() >= 0)
                    result.AppendLine(await reader.ReadLineAsync());
            }
            return result.ToString();
        }

        public static int GetQuarter(this DateTime date)
        {
            if (date.Month >= 4 && date.Month <= 6)
                return 1;
            else if (date.Month >= 7 && date.Month <= 9)
                return 2;
            else if (date.Month >= 10 && date.Month <= 12)
                return 3;
            else
                return 4;
        }
    }
}
