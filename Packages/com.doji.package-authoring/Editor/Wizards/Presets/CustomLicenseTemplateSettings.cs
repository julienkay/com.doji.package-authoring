using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating repository <c>LICENSE</c> files for custom licenses.
    /// </summary>
    [FilePath("ProjectSettings/PackageAuthoringCustomLicenseTemplate.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class CustomLicenseTemplateSettings : ProjectTemplateAsset {
        private static CustomLicenseTemplateSettings _instance;

        protected override string DefaultContent => Templates.CustomLicenseTemplate.DefaultContent;

        /// <summary>
        /// Shared project settings asset for the generated custom repository license template.
        /// </summary>
        public static CustomLicenseTemplateSettings Instance => _instance ??= GetOrCreateSettings();

        /// <summary>
        /// Saves the current settings instance back into the project settings asset.
        /// </summary>
        public void SaveSettings() {
            Save(true);
        }

        private void OnDisable() {
            if (_instance == this) {
                _instance = null;
            }
        }

        private static CustomLicenseTemplateSettings GetOrCreateSettings() {
            CustomLicenseTemplateSettings settings = LoadOrCreate();
            settings.EnsureDefaultContent();
            return settings;
        }

        private static CustomLicenseTemplateSettings LoadOrCreate() {
            Object[] settings =
                InternalEditorUtility.LoadSerializedFileAndForget(
                    "ProjectSettings/PackageAuthoringCustomLicenseTemplate.asset");
            if (settings.Length > 0 && settings[0] is CustomLicenseTemplateSettings customLicenseTemplateSettings) {
                return customLicenseTemplateSettings;
            }

            CustomLicenseTemplateSettings created = CreateInstance<CustomLicenseTemplateSettings>();
            created.hideFlags = HideFlags.HideAndDontSave;
            return created;
        }

        private void Save(bool saveAsText) {
            EnsureDefaultContent();
            InternalEditorUtility.SaveToSerializedFileAndForget(
                new Object[] {
                    this
                },
                "ProjectSettings/PackageAuthoringCustomLicenseTemplate.asset",
                saveAsText);
        }
    }
}
