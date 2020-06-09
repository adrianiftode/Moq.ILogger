using System;
using System.Text.RegularExpressions;

namespace Moq.Internal
{
    internal static class WildcardComparison
    {
        internal static bool IsWildcardMatch(this string source, string wildcard)
        {
            if (source == null)
            {
                throw new ArgumentException("Source cannot be null or an empty string.", nameof(source));
            }

            if (wildcard == null)
            {
                throw new ArgumentException("Wildcard cannot be null or an empty string.", nameof(wildcard));
            }

            var pattern = WildcardToRegular(wildcard);
            return Regex.IsMatch(source, pattern, RegexOptions.IgnoreCase);

            static string WildcardToRegular(string value) 
                => "^" + Regex.Escape(value).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        }
    }
}
