using System.Collections.Generic;
using System.Text;
using Doji.PackageAuthoring.Editor.Wizards.Templates;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.UI {
    /// <summary>
    /// Converts template text into rich-text markup that highlights supported token spans.
    /// </summary>
    internal static class TokenHighlightRichTextFormatter {
        private static readonly Color TokenColor = new(0.22f, 0.58f, 0.28f);

        /// <summary>
        /// Builds a rich-text string for overlay rendering so supported tokens appear colorized inline.
        /// </summary>
        /// <example>
        /// <c>https://docs.example.com/{{PACKAGE_NAME}}/api</c> becomes
        /// <c>https://docs.example.com/&lt;color=#388F47&gt;{{PACKAGE_NAME}}&lt;/color&gt;/api</c>
        /// before Unity renders it through the overlay label.
        /// </example>
        /// <param name="value">The full field content to display.</param>
        /// <returns>Rich-text markup ready for an overlay label.</returns>
        public static string Build(string value) {
            if (string.IsNullOrEmpty(value)) {
                return string.Empty;
            }

            IReadOnlyList<TemplateTokenMatch> tokenMatches =
                TemplateTokenResolver.GetDetectedSupportedTokenMatches(value);
            if (tokenMatches.Count == 0) {
                return value;
            }

            StringBuilder builder = new();
            int currentIndex = 0;
            string tokenColor = ColorUtility.ToHtmlStringRGB(TokenColor);

            foreach (TemplateTokenMatch match in tokenMatches) {
                if (match.StartIndex < currentIndex || match.StartIndex >= value.Length) {
                    continue;
                }

                int plainTextLength = match.StartIndex - currentIndex;
                if (plainTextLength > 0) {
                    builder.Append(value.Substring(currentIndex, plainTextLength));
                }

                int tokenLength = Mathf.Min(match.Length, value.Length - match.StartIndex);
                if (tokenLength <= 0) {
                    continue;
                }

                builder.Append("<color=#");
                builder.Append(tokenColor);
                builder.Append('>');
                builder.Append(value.Substring(match.StartIndex, tokenLength));
                builder.Append("</color>");
                currentIndex = match.StartIndex + tokenLength;
            }

            if (currentIndex < value.Length) {
                builder.Append(value.Substring(currentIndex));
            }

            return builder.ToString();
        }
    }
}
