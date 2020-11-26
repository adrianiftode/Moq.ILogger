using System;
using System.Text.RegularExpressions;

// ReSharper disable once CheckNamespace
namespace Moq
{
    internal static class WildcardComparison
    {
        internal static bool IsWildcardMatch(this string source, string wildcard)
        {
            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentException("Source cannot be null or an empty string.", nameof(source));
            }

            if (string.IsNullOrEmpty(wildcard))
            {
                throw new ArgumentException("Wildcard cannot be null or an empty string.", nameof(wildcard));
            }

            var pattern = WildcardToRegular(wildcard);
            return Regex.IsMatch(source, pattern, RegexOptions.IgnoreCase | RegexOptions.Multiline);

            static string WildcardToRegular(string value)
                => "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }

        internal static bool EqualsIgnoreCase(this string source, string other)
        {
            if (string.IsNullOrEmpty(source) && string.IsNullOrEmpty(other))
            {
                return true;
            }

            if (string.IsNullOrEmpty(source) || string.IsNullOrEmpty(other))
            {
                return false;
            }

            return string.Equals(source, other, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsWildcard(this string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return false;
            }

            if (source[0] == '*')
            {
                return true;
            }

            for (var i = 1; i < source.Length; i++)
            {
                if (source[i] == '*' && source[i - 1] != '\\')
                {
                    return true;
                }
            }

            return false;
        }
    }
}
