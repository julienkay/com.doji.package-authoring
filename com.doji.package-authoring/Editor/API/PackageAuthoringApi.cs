using System.Collections.Generic;
using Doji.PackageAuthoring.Wizards;
using Doji.PackageAuthoring.Wizards.Presets;
using Doji.PackageAuthoring.Models;

namespace Doji.PackageAuthoring {
    /// <summary>
    /// Exposes package authoring settings and generation operations for editor scripts that drive the tooling programmatically.
    /// </summary>
    public static class PackageAuthoringApi {
        /// <summary>
        /// Generates a standalone Unity project from the provided settings.
        /// </summary>
        /// <param name="projectSettings">Project-facing metadata and output location.</param>
        /// <param name="openProjectAfterCreation">Whether to open the generated project in the current Unity editor.</param>
        /// <returns>The generated project directory.</returns>
        public static string GenerateProject(ProjectSettings projectSettings, bool openProjectAfterCreation = false) {
            return PackageAuthoringGenerationUtility.GenerateProject(projectSettings, openProjectAfterCreation);
        }

        /// <summary>
        /// Generates a package repository and its companion Unity project from the provided settings.
        /// </summary>
        /// <param name="projectSettings">Companion project metadata and repository output root.</param>
        /// <param name="packageSettings">Package manifest and folder layout settings.</param>
        /// <param name="repoSettings">Repository-level files and git behavior.</param>
        /// <param name="openProjectAfterCreation">Whether to open the generated companion project in the current Unity editor.</param>
        /// <returns>The generated repository root directory.</returns>
        public static string GeneratePackage(
            ProjectSettings projectSettings,
            PackageSettings packageSettings,
            RepoSettings repoSettings,
            bool openProjectAfterCreation = false) {
            return PackageAuthoringGenerationUtility.GeneratePackage(
                projectSettings,
                packageSettings,
                repoSettings,
                openProjectAfterCreation);
        }

        /// <summary>
        /// Saves all package authoring project settings assets back into <c>ProjectSettings</c>.
        /// </summary>
        public static void SaveAllProjectSettings() {
            SaveSettingsAssets(PackageAuthoringSettingsRegistry.AllPersistedSettingsAssets);
        }

        /// <summary>
        /// Restores all editable templates to the package defaults and saves the updated project settings assets.
        /// </summary>
        public static void ReapplyAllTemplateDefaults() {
            RestoreDefaultContents(PackageAuthoringSettingsRegistry.AllResettableTemplateSettingsAssets);
            SaveAllProjectSettings();
        }

        private static void RestoreDefaultContents(IEnumerable<IPackageAuthoringTemplateSettingsAsset> templateSettings) {
            foreach (IPackageAuthoringTemplateSettingsAsset templateSetting in templateSettings) {
                templateSetting.RestoreDefaultContent();
            }
        }

        private static void SaveSettingsAssets(IEnumerable<IPackageAuthoringProjectSettingsAsset> settingsAssets) {
            foreach (IPackageAuthoringProjectSettingsAsset settingsAsset in settingsAssets) {
                settingsAsset.SaveSettings();
            }
        }
    }
}
