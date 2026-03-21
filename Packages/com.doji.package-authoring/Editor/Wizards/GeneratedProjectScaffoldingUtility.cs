using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using Doji.PackageAuthoring.Editor.Wizards.Models;

namespace Doji.PackageAuthoring.Editor.Wizards {
    /// <summary>
    /// Owns the shared baseline-copy and temporary Unity-project mutation flow used while generating projects.
    /// Temporary project settings are applied, the template baseline is copied with generated overrides,
    /// then the editor project's settings are restored.
    /// </summary>
    internal static class GeneratedProjectScaffoldingUtility {
        private static readonly IReadOnlyList<NamedBuildTarget> NamedTargets = new[] {
            NamedBuildTarget.Standalone,
            NamedBuildTarget.Android
        };

        /// <summary>
        /// Applies temporary project settings so copied template files inherit the generated project's metadata.
        /// </summary>
        public static IDisposable ApplyTemporaryProjectSettings(
            ProjectSettings projectSettings,
            Func<NamedBuildTarget, string> applicationIdentifierResolver,
            string rootNamespace) {
            return new TemporaryProjectSettingsScope(projectSettings, applicationIdentifierResolver, rootNamespace);
        }

        /// <summary>
        /// Copies the template project's baseline folders and optional generated overrides into a new project.
        /// </summary>
        public static void CopyTemplateProjectBaseline(
            string projectDirectory,
            string gitIgnoreContent = null,
            string manifestContent = null) {
            Directory.CreateDirectory(projectDirectory);
            Directory.CreateDirectory(Path.Combine(projectDirectory, "Assets"));
            CopyDirectory("Packages", Path.Combine(projectDirectory, "Packages"));
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
        /// Recursively copies a directory into the generated output, preserving existing destination files.
        /// </summary>
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
        public static void CopyFile(string sourceFileName, string destinationFileName) {
            if (!File.Exists(destinationFileName)) {
                File.Copy(sourceFileName, destinationFileName, overwrite: false);
            }
        }

        /// <summary>
        /// Writes scaffolded content to disk and ensures the target directory exists first.
        /// </summary>
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
        /// Captures the current Unity project settings, applies generated values, and restores the originals on disposal.
        /// </summary>
        private sealed class TemporaryProjectSettingsScope : IDisposable {
            private readonly string _originalCompanyName;
            private readonly string _originalProductName;
            private readonly string _originalVersion;
            private readonly List<string> _originalIdentifiers = new();
            private readonly string _originalRootNamespace;
            private bool _disposed;

            public TemporaryProjectSettingsScope(
                ProjectSettings projectSettings,
                Func<NamedBuildTarget, string> applicationIdentifierResolver,
                string rootNamespace) {
                if (projectSettings == null) {
                    throw new ArgumentNullException(nameof(projectSettings));
                }

                _originalCompanyName = PlayerSettings.companyName;
                _originalProductName = PlayerSettings.productName;
                _originalVersion = PlayerSettings.bundleVersion;
                foreach (NamedBuildTarget target in NamedTargets) {
                    _originalIdentifiers.Add(PlayerSettings.GetApplicationIdentifier(target));
                }

                _originalRootNamespace = EditorSettings.projectGenerationRootNamespace;

                PlayerSettings.companyName = projectSettings.CompanyName;
                PlayerSettings.productName = projectSettings.ProductName;
                PlayerSettings.bundleVersion = projectSettings.Version;
                foreach (NamedBuildTarget target in NamedTargets) {
                    string applicationIdentifier = applicationIdentifierResolver?.Invoke(target);
                    if (!string.IsNullOrWhiteSpace(applicationIdentifier)) {
                        PlayerSettings.SetApplicationIdentifier(target, applicationIdentifier);
                    }
                }

                EditorSettings.projectGenerationRootNamespace = rootNamespace;

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            public void Dispose() {
                if (_disposed) {
                    return;
                }

                PlayerSettings.companyName = _originalCompanyName;
                PlayerSettings.productName = _originalProductName;
                PlayerSettings.bundleVersion = _originalVersion;
                for (int i = 0; i < NamedTargets.Count; i++) {
                    PlayerSettings.SetApplicationIdentifier(NamedTargets[i], _originalIdentifiers[i]);
                }

                EditorSettings.projectGenerationRootNamespace = _originalRootNamespace;

                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                _disposed = true;
            }
        }
    }
}
