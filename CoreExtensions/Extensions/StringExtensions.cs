using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace PenguinSoft.CoreExtensions.Extensions
{
    public static class StringExtensions
    {

        [DebuggerStepThrough]
        public static bool EqualsIgnoreCase(this string str, string value)
            => str.Equals(value, StringComparison.InvariantCultureIgnoreCase);

        [DebuggerStepThrough]
        public static bool EqualsIgnoreCaseAccent(this string str, string value)
            => str.ToNormalForm().EqualsIgnoreCase(value.ToNormalForm());

        [DebuggerStepThrough]
        public static bool ContainsIgnoreCase(this string str, string value)
            => str.ToLowerInvariant().Contains(value.ToLowerInvariant());

        [DebuggerStepThrough]
        public static bool StartsWithIgnoreCase(this string str, string value)
            => str.StartsWith(value, StringComparison.InvariantCultureIgnoreCase);

        [DebuggerStepThrough]
        public static bool EndsWithIgnoreCase(this string str, string value)
            => str.EndsWith(value, StringComparison.InvariantCultureIgnoreCase);


        [DebuggerStepThrough]
        public static int ToInt(this string str, int? defaultValue = null)
        {
            try
            {
                return int.Parse(str, NumberStyles.Integer);
            }
            catch
            {
                if (defaultValue.HasValue)
                    return defaultValue.Value;

                throw;
            }
        }

        [DebuggerStepThrough]
        public static double ToDouble(this string str, double? defaultValue)
        {
            try
            {
                return double.Parse(str);
            }
            catch
            {
                if (defaultValue.HasValue)
                    return defaultValue.Value;

                throw;
            }
        }

        [DebuggerStepThrough]
        public static double ToDouble(this string str)
        {
            return double.Parse(str);
        }

        [DebuggerStepThrough]
        public static long ToLong(this string str, long? defaultValue = null)
        {
            try
            {
                return long.Parse(str);
            }
            catch
            {
                if (defaultValue.HasValue)
                    return defaultValue.Value;

                throw;
            }
        }

        [DebuggerStepThrough]
        public static long ToDateTimeTicks(this string str)
        {
            return DateTime.Parse(str).Ticks;
        }

        [DebuggerStepThrough]
        public static DateTime ToDateTime(this string str)
        {
            return DateTime.Parse(str);
        }

        [DebuggerStepThrough]
        public static string NullSafe(this string value)
        {
            return (value ?? string.Empty).Trim();
        }

        [DebuggerStepThrough]
        public static string NullSafe(this string value, string defaultValue)
        {
            return string.IsNullOrEmpty(value.NullSafe()) ? defaultValue : value;
        }

        public static string ToBase64(this string text)
        {
            var data = Encoding.UTF8.GetBytes(text);
            return Convert.ToBase64String(data);
        }

        public static string FromBase64(this string data)
        {
            var base64EncodedBytes = Convert.FromBase64String(data);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

        [DebuggerStepThrough]
        public static string ExtractNumbers(this string value)
        {
            return value == null ? null : string.Join(null, Regex.Split(value, "[^\\d]"));
        }

        [DebuggerStepThrough]
        public static string ToSeparatedWords(this string value)
        {
            value = Regex.Replace(value, "([_]|[-])([A-Z]|[a-z])", " $2").Trim();
            value = Regex.Replace(value, "(?<=[^ ])([A-Z][a-z])", " $1").Trim();
            return value;
        }

        [DebuggerStepThrough]
        public static string ToPascalCase(this string value)
        {
            var matches = Regex.Matches(value, "([A-Z][a-z]{1,})|(?<=[a-z])[A-Z]{1,}");
            foreach (Match match in matches)
            {
                var pascal = match.Value;
                value = value.Replace(pascal, $"{pascal.Substring(0, 1).ToUpper()}{pascal.Substring(1).ToLower()}");
            }

            value = $"{value.Substring(0, 1).ToUpper()}{value.Substring(1)}";

            matches = Regex.Matches(value, "([_]|[-]|[ ])([a-z])*");
            foreach (Match match in matches)
            {
                var pascal = match.Value;
                if (pascal.Length > 1)
                    value = value.Replace(pascal, $"{pascal.Substring(1, 1).ToUpper()}{pascal.Substring(2).ToLower()}");
            }

            matches = Regex.Matches(value, "([_]|[-]|[ ])([A-Z])*");
            foreach (Match match in matches)
            {
                var pascal = match.Value;
                if (pascal.Length > 1)
                    value = value.Replace(pascal, $"{pascal.Substring(1, 1).ToUpper()}{pascal.Substring(2).ToLower()}");
            }

            value = $"{value.Substring(0, 1).ToUpper()}{value.Substring(1)}";

            if (value.IsAllUpper())
                value = $"{value.Substring(0, 1).ToUpper()}{value.Substring(1).ToLower()}";

            value = Regex.Replace(value, "[_]|[-]|[ ]", "");

            value = Regex.Replace(value, "(?<=^[A-Z])[A-Z]{1,}(?=[A-Z][a-z])", x => x.Value.ToLower());
            return value;
        }

        [DebuggerStepThrough]
        public static bool IsAllUpper(this string value)
        {
            return value == value.ToUpper();
        }

        [DebuggerStepThrough]
        public static bool IsAllLower(this string value)
        {
            return value == value.ToLower();
        }
        [DebuggerStepThrough]
        public static bool IsNullOrEmpty(this string str)
        {
            return string.IsNullOrEmpty(str);
        }

        [DebuggerStepThrough]
        public static bool IsEmpty(this string value)
        {
            return string.IsNullOrEmpty(value.NullSafe());
        }

        [DebuggerStepThrough]
        public static string ToNormalForm(this string value)
        {
            return new string(value.Normalize(NormalizationForm.FormD).ToCharArray()
                .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                .ToArray());
        }

        [DebuggerStepThrough]
        public static string ExtractLetters(this string value)
        {
            return value == null ? null : string.Join(null, Regex.Split(value, "[^a-zA-Z]"));
        }

        [DebuggerStepThrough]
        public static string ExtractLettersAndNumbers(this string value)
        {
            return value == null ? null : string.Join(null, Regex.Split(value, "[^a-zA-Z0-9]"));
        }

    }
}