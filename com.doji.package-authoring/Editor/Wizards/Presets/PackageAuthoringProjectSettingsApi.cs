using System.Collections.Generic;

namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Exposes package authoring project-settings operations for editor scripts that want to drive the tooling programmatically.
    /// </summary>
    public static class PackageAuthoringProjectSettingsApi {
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
