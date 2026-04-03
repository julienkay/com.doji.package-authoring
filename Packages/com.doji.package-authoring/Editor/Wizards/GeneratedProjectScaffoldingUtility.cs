using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Doji.PackageAuthoring.Editor.Wizards.Models;

namespace Doji.PackageAuthoring.Editor.Wizards {
    /// <summary>
    /// Owns the shared baseline-copy and generated-project settings patching flow used while scaffolding projects.
    /// </summary>
    internal static class GeneratedProjectScaffoldingUtility {
        /// <summary>
        /// Keeps local package inheritance disabled until the behavior is intentionally exposed again.
        /// </summary>
        internal const bool IncludeLocalPackagesByDefault = false;

        /// <summary>
        /// Builds the default reverse-DNS application identifier used by generated Unity projects.
        /// </summary>
        /// <param name="companyName">Company name used for the middle reverse-DNS segment.</param>
        /// <param name="productName">Project name used for the final reverse-DNS segment.</param>
        /// <returns>A sanitized identifier like <c>com.company.product</c>.</returns>
        public static string BuildDefaultApplicationIdentifier(string companyName, string productName) {
            string companySegment = SanitizeApplicationIdentifierSegment(companyName);
            string productSegment = SanitizeApplicationIdentifierSegment(productName);
            return $"com.{companySegment}.{productSegment}";
        }

        /// <summary>
        /// Produces a valid C# root namespace from free-form user input.
        /// </summary>
        /// <param name="value">The user-facing name or namespace seed.</param>
        /// <returns>A namespace-safe PascalCase identifier.</returns>
        public static string SanitizeRootNamespace(string value) {
            StringBuilder builder = new();
            bool capitalizeNext = true;

            foreach (char character in value ?? string.Empty) {
                if (!char.IsLetterOrDigit(character)) {
                    capitalizeNext = true;
                    continue;
                }

                if (builder.Length == 0 && char.IsDigit(character)) {
                    builder.Append("Project");
                }

                builder.Append(capitalizeNext
                    ? char.ToUpperInvariant(character)
                    : character);
                capitalizeNext = false;
            }

            if (builder.Length == 0) {
                builder.Append("Project");
            }

            return builder.ToString();
        }

        /// <summary>
        /// Applies generated metadata directly to the copied project's settings assets.
        /// </summary>
        /// <param name="projectDirectory">Root directory of the generated Unity project.</param>
        /// <param name="projectSettings">Project-facing metadata to write into Unity settings files.</param>
        /// <param name="applicationIdentifier">Bundle/application identifier written to each existing target entry.</param>
        /// <param name="rootNamespace">Root namespace written into <c>EditorSettings.asset</c>.</param>
        public static void ApplyGeneratedProjectSettings(
            string projectDirectory,
            ProjectSettings projectSettings,
            string applicationIdentifier,
            string rootNamespace) {
            if (string.IsNullOrWhiteSpace(projectDirectory)) {
                throw new ArgumentException("Project directory is required.", nameof(projectDirectory));
            }

            if (projectSettings == null) {
                throw new ArgumentNullException(nameof(projectSettings));
            }

            string projectSettingsPath = Path.Combine(projectDirectory, "ProjectSettings", "ProjectSettings.asset");
            string editorSettingsPath = Path.Combine(projectDirectory, "ProjectSettings", "EditorSettings.asset");

            PatchProjectSettingsAsset(projectSettingsPath, projectSettings, applicationIdentifier);
            PatchEditorSettingsAsset(editorSettingsPath, rootNamespace);
        }

        /// <summary>
        /// Copies the template project's baseline folders and optional generated overrides into a new project.
        /// </summary>
        /// <param name="projectDirectory">Output directory for the generated Unity project.</param>
        /// <param name="gitIgnoreContent">Optional generated <c>.gitignore</c> content.</param>
        /// <param name="manifestContent">Optional generated <c>Packages/manifest.json</c> content.</param>
        public static void CopyTemplateProjectBaseline(
            string projectDirectory,
            string gitIgnoreContent = null,
            string manifestContent = null) {
            Directory.CreateDirectory(projectDirectory);
            Directory.CreateDirectory(Path.Combine(projectDirectory, "Assets"));
            CopyPackagesBaseline(Path.Combine(projectDirectory, "Packages"));
            CopyDirectory("ProjectSettings", Path.Combine(projectDirectory, "ProjectSettings"));

            if (!string.IsNullOrWhiteSpace(gitIgnoreContent)) {
                CreateFile(Path.Combine(projectDirectory, ".gitignore"), gitIgnoreContent);
            }

            if (!string.IsNullOrWhiteSpace(manifestContent)) {
                CreateFile(
                    Path.Combine(projectDirectory, "Packages", "manifest.json"),
                    manifestContent,
                    overwrite: true);
            }
        }

        /// <summary>
        /// Copies the package-manager baseline and optionally preserves the template project's local package lock file.
        /// </summary>
        /// <param name="destinationDir">Destination <c>Packages</c> directory in the generated project.</param>
        public static void CopyPackagesBaseline(string destinationDir) {
            const string sourceDir = "Packages";
            if (!Directory.Exists(sourceDir)) {
                UnityEngine.Debug.LogWarning($"Source directory {sourceDir} does not exist. Skipping copy.");
                return;
            }

            Directory.CreateDirectory(destinationDir);

            foreach (string file in Directory.GetFiles(sourceDir)) {
                string fileName = Path.GetFileName(file);
                if (!IncludeLocalPackagesByDefault
                    && string.Equals(fileName, "packages-lock.json", StringComparison.OrdinalIgnoreCase)) {
                    continue;
                }

                string destinationFile = Path.Combine(destinationDir, fileName);
                CopyFile(file, destinationFile);
            }
        }

        /// <summary>
        /// Recursively copies a directory into the generated output, preserving existing destination files.
        /// </summary>
        /// <param name="sourceDir">Source directory in the template project.</param>
        /// <param name="destinationDir">Destination directory in the generated project.</param>
        public static void CopyDirectory(string sourceDir, string destinationDir) {
            if (!Directory.Exists(sourceDir)) {
                UnityEngine.Debug.LogWarning($"Source directory {sourceDir} does not exist. Skipping copy.");
                return;
            }

            Directory.CreateDirectory(destinationDir);

            foreach (string file in Directory.GetFiles(sourceDir)) {
                string destinationFile = Path.Combine(destinationDir, Path.GetFileName(file));
                CopyFile(file, destinationFile);
            }

            foreach (string subDirectory in Directory.GetDirectories(sourceDir)) {
                string destinationSubDirectory = Path.Combine(destinationDir, Path.GetFileName(subDirectory));
                CopyDirectory(subDirectory, destinationSubDirectory);
            }
        }

        /// <summary>
        /// Copies a single file if the destination does not already exist.
        /// </summary>
        /// <param name="sourceFileName">Source file path.</param>
        /// <param name="destinationFileName">Destination file path.</param>
        public static void CopyFile(string sourceFileName, string destinationFileName) {
            if (!File.Exists(destinationFileName)) {
                File.Copy(sourceFileName, destinationFileName, overwrite: false);
            }
        }

        /// <summary>
        /// Writes scaffolded content to disk and ensures the target directory exists first.
        /// </summary>
        /// <param name="path">File path to create.</param>
        /// <param name="content">File content to write.</param>
        /// <param name="overwrite">Whether an existing file should be replaced.</param>
        public static void CreateFile(string path, string content, bool overwrite = false) {
            if (File.Exists(path) && !overwrite) {
                return;
            }

            string directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory)) {
                Directory.CreateDirectory(directory);
            }

            File.WriteAllText(path, content);
        }

        /// <summary>
        /// Updates the generated <c>ProjectSettings.asset</c> file after the template baseline has been copied.
        /// This avoids mutating the currently open Unity project just to make the copied YAML inherit new values.
        /// </summary>
        /// <param name="assetPath">Path to the generated <c>ProjectSettings.asset</c>.</param>
        /// <param name="projectSettings">Project metadata to write into the asset.</param>
        /// <param name="applicationIdentifier">Application identifier to write into all existing target entries.</param>
        private static void PatchProjectSettingsAsset(
            string assetPath,
            ProjectSettings projectSettings,
            string applicationIdentifier) {
            if (!File.Exists(assetPath)) {
                throw new FileNotFoundException("Generated ProjectSettings.asset was not found.", assetPath);
            }

            string content = File.ReadAllText(assetPath);
            content = ReplaceYamlScalar(content, "companyName", projectSettings.CompanyName);
            content = ReplaceYamlScalar(content, "productName", projectSettings.ProductName);
            content = ReplaceYamlScalar(content, "bundleVersion", projectSettings.Version);

            if (!string.IsNullOrWhiteSpace(applicationIdentifier)) {
                content = ReplaceApplicationIdentifiers(content, applicationIdentifier);
                content = ReplaceOrInsertOverrideDefaultApplicationIdentifier(content, "1");
            }

            File.WriteAllText(assetPath, content);
        }

        /// <summary>
        /// Updates the generated <c>EditorSettings.asset</c> file with the resolved root namespace used for IDE project generation.
        /// </summary>
        /// <param name="assetPath">Path to the generated <c>EditorSettings.asset</c>.</param>
        /// <param name="rootNamespace">Namespace value to write.</param>
        private static void PatchEditorSettingsAsset(string assetPath, string rootNamespace) {
            if (!File.Exists(assetPath)) {
                throw new FileNotFoundException("Generated EditorSettings.asset was not found.", assetPath);
            }

            string content = File.ReadAllText(assetPath);
            content = ReplaceYamlScalar(content, "m_ProjectGenerationRootNamespace", rootNamespace);
            File.WriteAllText(assetPath, content);
        }

        /// <summary>
        /// Rewrites every existing application-identifier entry under Unity's YAML map to the same generated identifier.
        /// The method intentionally preserves the target list already present in the template asset instead of hardcoding targets in code.
        /// </summary>
        /// <param name="content">Full YAML content of <c>ProjectSettings.asset</c>.</param>
        /// <param name="applicationIdentifier">Identifier value to assign to each target entry.</param>
        /// <returns>The patched YAML content.</returns>
        private static string ReplaceApplicationIdentifiers(string content, string applicationIdentifier) {
            Match match = Regex.Match(
                content,
                @"(^  applicationIdentifier:\r?\n)(?<entries>(?:^    [^:\r\n]+:.*\r?\n)+)",
                RegexOptions.Multiline);

            if (!match.Success) {
                throw new InvalidOperationException("Could not find applicationIdentifier entries to patch.");
            }

            string replacement = Regex.Replace(
                match.Value,
                @"(^  applicationIdentifier:\r?\n)(?<entries>(?:^    [^:\r\n]+:.*\r?\n)+)",
                innerMatch => {
                    StringBuilder builder = new();
                    builder.Append(innerMatch.Groups[1].Value);
                    string entries = innerMatch.Groups["entries"].Value;
                    foreach (string line in entries.Split(new[] {
                                 "\r\n",
                                 "\n"
                             }, StringSplitOptions.None)) {
                        if (string.IsNullOrWhiteSpace(line)) {
                            continue;
                        }

                        int separatorIndex = line.IndexOf(':');
                        if (separatorIndex < 0) {
                            builder.AppendLine(line);
                            continue;
                        }

                        string key = line.Substring(0, separatorIndex);
                        builder.Append(key);
                        builder.Append(": ");
                        builder.AppendLine(applicationIdentifier);
                    }

                    return builder.ToString();
                },
                RegexOptions.Multiline);

            return content.Substring(0, match.Index) +
                   replacement +
                   content.Substring(match.Index + match.Length);
        }

        /// <summary>
        /// Replaces Unity's application-identifier override flag when present, or inserts it after the
        /// application-identifier map for Unity versions that omit the scalar until it is explicitly set.
        /// </summary>
        /// <param name="content">Full YAML content of <c>ProjectSettings.asset</c>.</param>
        /// <param name="value">Override flag value to write.</param>
        /// <returns>The patched YAML content.</returns>
        private static string ReplaceOrInsertOverrideDefaultApplicationIdentifier(string content, string value) {
            const string Key = "overrideDefaultApplicationIdentifier";
            string scalarPattern = $@"^(?<indent>\s*){Key}:.*$";
            if (Regex.IsMatch(content, scalarPattern, RegexOptions.Multiline)) {
                return ReplaceYamlScalar(content, Key, value);
            }

            string inserted = Regex.Replace(
                content,
                @"(?<map>^  applicationIdentifier:\r?\n(?:^    [^:\r\n]+:.*\r?\n)+)",
                match => $"{match.Groups["map"].Value}  {Key}: {value}{Environment.NewLine}",
                RegexOptions.Multiline);

            if (inserted == content) {
                throw new InvalidOperationException(
                    $"Could not find a location to insert YAML key '{Key}'.");
            }

            return inserted;
        }

        /// <summary>
        /// Replaces a single-line YAML scalar while preserving its current indentation.
        /// This is used for Unity settings assets whose layout is stable enough to patch without a full YAML serializer.
        /// </summary>
        /// <param name="content">Full YAML file content.</param>
        /// <param name="key">Scalar key to replace.</param>
        /// <param name="value">Replacement value.</param>
        /// <returns>The patched YAML content.</returns>
        private static string ReplaceYamlScalar(string content, string key, string value) {
            string safeValue = value ?? string.Empty;
            string pattern = $@"^(?<indent>\s*){Regex.Escape(key)}:.*$";
            Match match = Regex.Match(content, pattern, RegexOptions.Multiline);
            if (!match.Success) {
                throw new InvalidOperationException($"Could not find YAML key '{key}' to patch.");
            }

            return Regex.Replace(
                content,
                pattern,
                scalarMatch => $"{scalarMatch.Groups["indent"].Value}{key}: {safeValue}",
                RegexOptions.Multiline);
        }

        /// <summary>
        /// Normalizes one reverse-DNS identifier segment so it only contains lowercase ASCII letters and digits.
        /// Empty or digit-prefixed results are given a stable <c>app</c> prefix to keep the final identifier valid.
        /// </summary>
        /// <param name="value">The source segment text.</param>
        /// <returns>A sanitized identifier segment.</returns>
        private static string SanitizeApplicationIdentifierSegment(string value) {
            StringBuilder builder = new();
            foreach (char character in value ?? string.Empty) {
                if (char.IsLetterOrDigit(character)) {
                    builder.Append(char.ToLowerInvariant(character));
                }
            }

            string sanitized = builder.ToString();
            if (string.IsNullOrEmpty(sanitized)) {
                sanitized = "app";
            }

            if (!char.IsLetter(sanitized[0])) {
                sanitized = $"app{sanitized}";
            }

            return sanitized;
        }
    }
}
