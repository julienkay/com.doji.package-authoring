using System;
using System.Collections.Generic;
using System.Linq;
using Doji.PackageAuthoring.Editor.Wizards.Models;

namespace Doji.PackageAuthoring.Editor.Wizards.Templates {
    /// <summary>
    /// Describes one supported token occurrence inside template text so editor UI can decorate the exact span.
    /// </summary>
    /// <remarks>
    /// This is intentionally lightweight because callers usually create a temporary collection every repaint while
    /// drawing IMGUI controls that need to colorize token substrings.
    /// </summary>
    internal readonly struct TemplateTokenMatch {
        /// <summary>
        /// Captures a supported token string and the zero-based character offset where it appears.
        /// </summary>
        public TemplateTokenMatch(string token, int startIndex) {
            Token = token;
            StartIndex = startIndex;
        }

        /// <summary>
        /// The exact supported token text found in the source string.
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Zero-based character offset of the token within the inspected source string.
        /// </summary>
        public int StartIndex { get; }

        /// <summary>
        /// Character length of <see cref="Token"/> so callers can derive the full matched span.
        /// </summary>
        public int Length => Token?.Length ?? 0;
    }

    /// <summary>
    /// Resolves shared placeholder tokens against the current package authoring context.
    /// </summary>
    internal static class TemplateTokenResolver {
        public const string SupportedTokensReferenceText = @"{{YEAR}}
{{COPYRIGHT_HOLDER}}
{{PACKAGE_NAME}}
{{PACKAGE_DISPLAY_NAME}}
{{PACKAGE_VERSION}}
{{PACKAGE_COMPANY}}
{{PACKAGE_DESCRIPTION}}
{{DOCUMENTATION_URL}}
{{PROJECT_NAME}}
{{PROJECT_COMPANY}}
{{NAMESPACE_NAME}}
{{NAMESPACE_NAME_REGEX}}
{{ASSEMBLY_NAME}}";

        public const string SupportedTokensHelpText =
            "Available tokens: {{YEAR}}, {{COPYRIGHT_HOLDER}}, {{PACKAGE_NAME}}, {{PACKAGE_DISPLAY_NAME}}, {{PACKAGE_VERSION}}, {{PACKAGE_COMPANY}}, {{PACKAGE_DESCRIPTION}}, {{DOCUMENTATION_URL}}, {{PROJECT_NAME}}, {{PROJECT_COMPANY}}, {{NAMESPACE_NAME}}, {{NAMESPACE_NAME_REGEX}}, {{ASSEMBLY_NAME}}";

        public const string SupportedTokensTooltipSuffix = "Supports standard package authoring placeholders.";

        private static readonly string[] SupportedTokens = {
            "{{YEAR}}",
            "{{COPYRIGHT_HOLDER}}",
            "{{PACKAGE_NAME}}",
            "{{PACKAGE_DISPLAY_NAME}}",
            "{{PACKAGE_VERSION}}",
            "{{PACKAGE_COMPANY}}",
            "{{PACKAGE_DESCRIPTION}}",
            "{{DOCUMENTATION_URL}}",
            "{{PROJECT_NAME}}",
            "{{PROJECT_COMPANY}}",
            "{{NAMESPACE_NAME}}",
            "{{NAMESPACE_NAME_REGEX}}",
            "{{ASSEMBLY_NAME}}"
        };

        /// <summary>
        /// Replaces supported placeholder tokens in the provided template content.
        /// </summary>
        /// <param name="template">Raw template text that may contain supported tokens.</param>
        /// <param name="ctx">Current scaffold context used to populate token values.</param>
        public static string Resolve(string template, PackageContext ctx) {
            return Resolve(template, ctx?.Project, ctx?.Package, ctx?.Repo);
        }

        /// <summary>
        /// Replaces supported placeholder tokens using the provided settings objects.
        /// </summary>
        /// <param name="template">Raw template text that may contain supported tokens.</param>
        /// <param name="project">Current project settings.</param>
        /// <param name="package">Current package settings when available.</param>
        /// <param name="repo">Current repository settings when available.</param>
        public static string Resolve(
            string template,
            ProjectSettings project,
            PackageSettings package = null,
            RepoSettings repo = null) {
            if (string.IsNullOrEmpty(template)) {
                return string.Empty;
            }

            Dictionary<string, string> tokenValues = GetTokenValues(project, package, repo);
            string resolved = template;
            foreach (KeyValuePair<string, string> tokenValue in tokenValues) {
                resolved = resolved.Replace(tokenValue.Key, tokenValue.Value ?? string.Empty);
            }

            return resolved;
        }

        /// <summary>
        /// Returns the distinct supported placeholder tokens detected in the provided template content.
        /// </summary>
        /// <remarks>
        /// Use this when only presence matters. For editor decoration where exact character ranges matter,
        /// <see cref="GetDetectedSupportedTokenMatches"/> is the authoritative API.
        /// </remarks>
        public static IReadOnlyList<string> GetDetectedSupportedTokens(string template) {
            if (string.IsNullOrEmpty(template)) {
                return Array.Empty<string>();
            }

            return GetDetectedSupportedTokenMatches(template)
                .Select(match => match.Token)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>
        /// Returns the concrete positions of supported placeholder tokens detected in the provided template content.
        /// </summary>
        /// <remarks>
        /// Matches are sorted by start index and then by descending token length so consumers can render or process
        /// the string from left to right without additional ordering work.
        /// </remarks>
        /// <param name="template">Template content to inspect for supported placeholder tokens.</param>
        /// <returns>Ordered token spans for every supported token occurrence.</returns>
        public static IReadOnlyList<TemplateTokenMatch> GetDetectedSupportedTokenMatches(string template) {
            if (string.IsNullOrEmpty(template)) {
                return Array.Empty<TemplateTokenMatch>();
            }

            List<TemplateTokenMatch> matches = new();
            foreach (string token in SupportedTokens) {
                int startIndex = 0;
                while (startIndex < template.Length) {
                    int matchIndex = template.IndexOf(token, startIndex, StringComparison.Ordinal);
                    if (matchIndex < 0) {
                        break;
                    }

                    matches.Add(new TemplateTokenMatch(token, matchIndex));
                    // Move past the current token to keep repeated occurrences deterministic and non-overlapping.
                    startIndex = matchIndex + token.Length;
                }
            }

            return matches
                .OrderBy(match => match.StartIndex)
                .ThenByDescending(match => match.Length)
                .ToArray();
        }

        private static Dictionary<string, string> GetTokenValues(
            ProjectSettings project,
            PackageSettings package,
            RepoSettings repo) {
            int currentYear = DateTime.Now.Year;

            Dictionary<string, string> tokenValues = new() {
                ["{{YEAR}}"] = currentYear.ToString(),
                ["{{COPYRIGHT_HOLDER}}"] = repo?.CopyrightHolder ?? string.Empty,
                ["{{PACKAGE_NAME}}"] = package?.PackageName ?? string.Empty,
                ["{{PACKAGE_DISPLAY_NAME}}"] = package?.PackageDisplayName ?? string.Empty,
                ["{{PACKAGE_VERSION}}"] = project?.Version ?? string.Empty,
                ["{{PACKAGE_COMPANY}}"] = package?.CompanyName ?? string.Empty,
                ["{{PACKAGE_DESCRIPTION}}"] = package?.Description ?? string.Empty,
                ["{{DOCUMENTATION_URL}}"] = string.Empty,
                ["{{PROJECT_NAME}}"] = project?.ProductName ?? string.Empty,
                ["{{PROJECT_COMPANY}}"] = project?.CompanyName ?? string.Empty,
                ["{{NAMESPACE_NAME}}"] = package?.NamespaceName ?? string.Empty,
                ["{{NAMESPACE_NAME_REGEX}}"] = (package?.NamespaceName ?? string.Empty).Replace(".", @"\."),
                ["{{ASSEMBLY_NAME}}"] = package?.AssemblyName ?? string.Empty
            };

            tokenValues["{{DOCUMENTATION_URL}}"] = ResolveDocumentationUrl(package?.DocumentationUrl, tokenValues);
            return tokenValues;
        }

        private static string ResolveDocumentationUrl(
            string documentationUrl,
            IReadOnlyDictionary<string, string> tokenValues) {
            if (string.IsNullOrWhiteSpace(documentationUrl)) {
                return "#";
            }

            string resolvedDocumentationUrl = documentationUrl;
            foreach (KeyValuePair<string, string> tokenValue in tokenValues) {
                if (string.Equals(tokenValue.Key, "{{DOCUMENTATION_URL}}", StringComparison.Ordinal)) {
                    continue;
                }

                resolvedDocumentationUrl = resolvedDocumentationUrl.Replace(tokenValue.Key, tokenValue.Value ?? string.Empty);
            }

            return resolvedDocumentationUrl;
        }
    }
}
