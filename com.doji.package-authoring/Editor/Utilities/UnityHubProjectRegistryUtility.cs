using System;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace Doji.PackageAuthoring.Utilities {
    /// <summary>
    /// Best-effort helper that adds generated projects to the local Unity Hub project registry.
    /// </summary>
    internal static class UnityHubProjectRegistryUtility {
        internal static string HubDataDirectoryOverride { get; set; }

        /// <summary>
        /// Attempts to register a Unity project in the local Unity Hub project list.
        /// </summary>
        /// <param name="projectPath">Absolute path to the Unity project that should be registered.</param>
        /// <returns><see langword="true"/> when the registry file was updated successfully.</returns>
        internal static bool TryRegisterProject(string projectPath) {
            if (string.IsNullOrWhiteSpace(projectPath) || !Directory.Exists(projectPath)) {
                Debug.LogWarning($"Cannot add Unity project to Unity Hub because path is invalid: {projectPath}");
                return false;
            }

            string registryPath = GetProjectsRegistryPath();
            if (string.IsNullOrWhiteSpace(registryPath)) {
                Debug.Log("Unity Hub project registry path could not be resolved. Skipping Unity Hub registration.");
                return false;
            }

            try {
                Directory.CreateDirectory(Path.GetDirectoryName(registryPath) ?? string.Empty);

                JObject root = LoadRegistry(registryPath);
                JObject data = root["data"] as JObject ?? new JObject();
                data[projectPath] = BuildProjectEntry(projectPath);
                root["schema_version"] = root["schema_version"] ?? "v1";
                root["data"] = data;

                File.WriteAllText(registryPath, root.ToString(Formatting.None));
                Debug.Log($"Added project to Unity Hub registry: {projectPath}");
                return true;
            }
            catch (Exception exception) {
                Debug.LogWarning(
                    $"Failed to add generated project to Unity Hub registry at '{registryPath}'. {exception.GetType().Name}: {exception.Message}");
                return false;
            }
        }

        private static JObject LoadRegistry(string registryPath) {
            if (!File.Exists(registryPath)) {
                return new JObject {
                    ["schema_version"] = "v1",
                    ["data"] = new JObject()
                };
            }

            string json = File.ReadAllText(registryPath);
            if (string.IsNullOrWhiteSpace(json)) {
                return new JObject {
                    ["schema_version"] = "v1",
                    ["data"] = new JObject()
                };
            }

            JObject root = JObject.Parse(json);
            root["data"] ??= new JObject();
            return root;
        }

        private static JObject BuildProjectEntry(string projectPath) {
            string projectName =
                Path.GetFileName(projectPath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
            string containingFolderPath = Path.GetDirectoryName(projectPath) ?? string.Empty;
            DateTimeOffset lastModifiedUtc = Directory.GetLastWriteTimeUtc(projectPath);
            string editorVersion = TryReadProjectVersion(projectPath, "m_EditorVersion");
            string changeset = TryReadProjectVersion(projectPath, "m_EditorVersionWithRevision");

            JObject entry = new() {
                ["title"] = projectName,
                ["lastModified"] = lastModifiedUtc == DateTimeOffset.MinValue
                    ? null
                    : lastModifiedUtc.ToUnixTimeMilliseconds(),
                ["isCustomEditor"] = false,
                ["path"] = projectPath,
                ["containingFolderPath"] = containingFolderPath,
                ["version"] = editorVersion,
                ["architecture"] = GetEditorArchitecture(),
                ["isFavorite"] = false
            };

            if (!string.IsNullOrWhiteSpace(changeset)) {
                entry["changeset"] = changeset;
            }

            return entry;
        }

        private static string GetProjectsRegistryPath() {
            string hubDataDirectory = GetHubDataDirectory();
            return string.IsNullOrWhiteSpace(hubDataDirectory)
                ? string.Empty
                : Path.Combine(hubDataDirectory, "projects-v1.json");
        }

        private static string GetHubDataDirectory() {
            if (!string.IsNullOrWhiteSpace(HubDataDirectoryOverride)) {
                return HubDataDirectoryOverride;
            }

            if (Application.platform == RuntimePlatform.WindowsEditor) {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, "UnityHub");
            }

            if (Application.platform == RuntimePlatform.OSXEditor) {
                string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Path.Combine(homeDirectory, "Library", "Application Support", "UnityHub");
            }

            if (Application.platform == RuntimePlatform.LinuxEditor) {
                string xdgConfigHome = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");
                if (!string.IsNullOrWhiteSpace(xdgConfigHome)) {
                    return Path.Combine(xdgConfigHome, "UnityHub");
                }

                string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
                return Path.Combine(homeDirectory, ".config", "UnityHub");
            }

            return string.Empty;
        }

        private static string TryReadProjectVersion(string projectPath, string key) {
            string projectVersionPath = Path.Combine(projectPath, "ProjectSettings", "ProjectVersion.txt");
            if (!File.Exists(projectVersionPath)) {
                return string.Empty;
            }

            foreach (string line in File.ReadLines(projectVersionPath)) {
                if (!line.StartsWith($"{key}: ", StringComparison.Ordinal)) {
                    continue;
                }

                string value = line.Substring(key.Length + 2).Trim();
                if (key == "m_EditorVersionWithRevision") {
                    int revisionStart = value.IndexOf('(');
                    int revisionEnd = value.IndexOf(')');
                    if (revisionStart >= 0 && revisionEnd > revisionStart) {
                        return value.Substring(revisionStart + 1, revisionEnd - revisionStart - 1);
                    }
                }

                return value;
            }

            return string.Empty;
        }

        private static string GetEditorArchitecture() {
            return RuntimeInformation.ProcessArchitecture switch {
                Architecture.Arm64 => "arm64",
                Architecture.X64 => "x86_64",
                Architecture.X86 => "x86",
                _ => RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant()
            };
        }
    }
}
