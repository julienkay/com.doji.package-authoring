using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Doji.PackageAuthoring.Editor.Wizards.Models;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.Templates {
    /// <summary>
    /// Builds generated Unity project manifests from the baseline project manifest plus scaffold-specific overrides.
    /// </summary>
    public static class ProjectManifestTemplate {
        private static readonly string[] IdePackageIds = {
            "com.unity.ide.visualstudio",
            "com.unity.ide.vscode",
            "com.unity.ide.rider"
        };

        /// <summary>
        /// Builds a generated project manifest from the baseline project manifest plus project-specific overrides.
        /// </summary>
        public static string GetProjectManifest(ProjectSettings projectSettings) {
            JObject json = LoadBaselineManifest();
            json["dependencies"] = GetProjectDependencies(projectSettings, json["dependencies"] as JObject);
            json.Remove("testables");
            return json.ToString(Formatting.Indented);
        }

        public static string GetProjectManifest(PackageContext ctx) {
            JObject json = LoadBaselineManifest();
            json["dependencies"] = GetProjectDependencies(ctx.Project, json["dependencies"] as JObject);

            if (ctx.Package.CreateTestsFolder) {
                json["testables"] = new JArray(ctx.Package.PackageName);
            } else {
                json.Remove("testables");
            }

            ((JObject)json["dependencies"])[ctx.Package.PackageName] = $"file:../../../{ctx.Package.PackageName}";
            return json.ToString(Formatting.Indented);
        }

        private static JObject GetProjectDependencies(ProjectSettings projectSettings, JObject baselineDependencies) {
            JObject deps = baselineDependencies != null
                ? new JObject(baselineDependencies)
                : new JObject();

            AddIncludedPackages(deps, projectSettings?.IncludedPackages);
            RemoveIdeDependencies(deps);
            AddPreferredEditorDependency(deps, projectSettings?.PreferredEditor ?? PreferredEditor.None);
            return deps;
        }

        private static JObject LoadBaselineManifest() {
            string manifestPath = Path.Combine("Packages", "manifest.json");
            if (!File.Exists(manifestPath)) {
                Debug.LogWarning($"Baseline manifest not found at {manifestPath}. Falling back to an empty manifest.");
                return new JObject();
            }

            return JObject.Parse(File.ReadAllText(manifestPath));
        }

        private static void RemoveIdeDependencies(JObject deps) {
            foreach (string packageId in IdePackageIds) {
                deps.Remove(packageId);
            }
        }

        private static void AddIncludedPackages(JObject deps, PackageDependencyList includedPackages) {
            if (includedPackages?.Items == null) {
                return;
            }

            foreach (PackageDependencyEntry package in includedPackages.Items) {
                string packageName = package?.PackageName?.Trim();
                string version = package?.Version?.Trim();
                if (string.IsNullOrWhiteSpace(packageName) || string.IsNullOrWhiteSpace(version)) {
                    continue;
                }

                deps[packageName] = version;
            }
        }

        private static void AddPreferredEditorDependency(JObject deps, PreferredEditor preferredEditor) {
            switch (preferredEditor) {
                case PreferredEditor.VisualStudio:
                    deps["com.unity.ide.visualstudio"] = "2.0.27";
                    break;
                case PreferredEditor.VisualStudioCode:
                    deps["com.unity.ide.vscode"] = "1.2.5";
                    break;
                case PreferredEditor.Rider:
                    deps["com.unity.ide.rider"] = "3.0.39";
                    break;
            }
        }
    }
}
