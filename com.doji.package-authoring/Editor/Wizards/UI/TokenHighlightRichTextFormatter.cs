using System.Collections.Generic;
using System.Text;
using Doji.PackageAuthoring.Wizards.Templates;
using UnityEngine;

namespace Doji.PackageAuthoring.Wizards.UI {
    /// <summary>
    /// Converts preview text into rich-text markup that can colorize supported tokens and optional hover-linked values.
    /// </summary>
    internal static class TokenHighlightRichTextFormatter {
        private static readonly Color TokenColor = new(0.22f, 0.58f, 0.28f);
        private static readonly Color HoverHighlightColor = new(0.82f, 0.53f, 0.10f);

        /// <summary>
        /// Builds a rich-text string for overlay rendering with token highlighting and optional hover accents.
        /// </summary>
        /// <example>
        /// <c>https://docs.example.com/{{PACKAGE_NAME}}/api</c> becomes
        /// <c>https://docs.example.com/&lt;color=#388F47&gt;{{PACKAGE_NAME}}&lt;/color&gt;/api</c>
        /// before Unity renders it through the overlay label.
        /// </example>
        /// <param name="value">The full field content to display.</param>
        /// <param name="hoverHighlights">Resolved literal substrings that should receive hover-driven accent coloring.</param>
        /// <returns>Rich-text markup ready for an overlay label.</returns>
        public static string Build(string value, IReadOnlyList<string> hoverHighlights = null) {
            if (string.IsNullOrEmpty(value)) {
                return string.Empty;
            }

            List<RichTextMatch> matches = new();
            AppendTokenMatches(matches, value);
            AppendLiteralMatches(matches, value, hoverHighlights);
            matches.Sort((left, right) => {
                int startComparison = left.StartIndex.CompareTo(right.StartIndex);
                return startComparison != 0 ? startComparison : right.Length.CompareTo(left.Length);
            });
            
            StringBuilder builder = new();
            int currentIndex = 0;

            foreach (RichTextMatch match in matches) {
                if (match.StartIndex < currentIndex || match.StartIndex >= value.Length) {
                    continue;
                }

                int plainTextLength = match.StartIndex - currentIndex;
                if (plainTextLength > 0) {
                    builder.Append(value.Substring(currentIndex, plainTextLength));
                }

                int matchLength = Mathf.Min(match.Length, value.Length - match.StartIndex);
                if (matchLength <= 0) {
                    continue;
                }

                builder.Append("<color=#");
                builder.Append(ColorUtility.ToHtmlStringRGB(match.Color));
                builder.Append('>');
                builder.Append(value.Substring(match.StartIndex, matchLength));
                builder.Append("</color>");
                currentIndex = match.StartIndex + matchLength;
            }

            if (currentIndex < value.Length) {
                builder.Append(value.Substring(currentIndex));
            }

            return builder.ToString();
        }

        private static void AppendTokenMatches(List<RichTextMatch> matches, string value) {
            IReadOnlyList<TemplateTokenMatch> tokenMatches = TemplateTokenResolver.GetDetectedSupportedTokenMatches(value);
            foreach (TemplateTokenMatch match in tokenMatches) {
                matches.Add(new RichTextMatch(match.StartIndex, match.Length, TokenColor));
            }
        }

        private static void AppendLiteralMatches(
            List<RichTextMatch> matches,
            string value,
            IReadOnlyList<string> hoverHighlights) {
            if (hoverHighlights == null || hoverHighlights.Count == 0) {
                return;
            }

            HashSet<string> uniqueHighlights = new(System.StringComparer.Ordinal);
            foreach (string highlight in hoverHighlights) {
                if (!string.IsNullOrWhiteSpace(highlight)) {
                    uniqueHighlights.Add(highlight);
                }
            }

            foreach (string highlight in uniqueHighlights) {
                int startIndex = 0;
                while (startIndex < value.Length) {
                    int matchIndex = value.IndexOf(highlight, startIndex, System.StringComparison.Ordinal);
                    if (matchIndex < 0) {
                        break;
                    }

                    matches.Add(new RichTextMatch(matchIndex, highlight.Length, HoverHighlightColor));
                    startIndex = matchIndex + highlight.Length;
                }
            }
        }

        private readonly struct RichTextMatch {
            public RichTextMatch(int startIndex, int length, Color color) {
                StartIndex = startIndex;
                Length = length;
                Color = color;
            }

            public int StartIndex { get; }

            public int Length { get; }

            public Color Color { get; }
        }
    }
}
