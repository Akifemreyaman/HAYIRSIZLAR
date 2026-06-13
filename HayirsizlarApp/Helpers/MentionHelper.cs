using System;
using System.Text.RegularExpressions;

namespace HayirsizlarApp.Helpers
{
    public static class MentionHelper
    {
        private static readonly Regex MentionRegex = new Regex(@"\B@([a-zA-Z0-9_]{3,20})\b", RegexOptions.Compiled);

        public static string HighlightMentions(string? content)
        {
            if (string.IsNullOrEmpty(content)) return string.Empty;

            // HTML encode first to prevent XSS
            var encoded = System.Net.WebUtility.HtmlEncode(content);
            
            // Highlight mentions
            var formatted = MentionRegex.Replace(encoded, match =>
            {
                return $"<span class=\"mention\">{match.Value}</span>";
            });

            // Convert newlines to <br />
            formatted = formatted.Replace("\n", "<br />");

            return formatted;
        }
    }
}
