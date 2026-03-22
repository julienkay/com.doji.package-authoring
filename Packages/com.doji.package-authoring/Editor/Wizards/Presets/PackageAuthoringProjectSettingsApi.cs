namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Exposes package authoring project-settings operations for editor scripts that want to drive the tooling programmatically.
    /// </summary>
    public static class PackageAuthoringProjectSettingsApi {
        /// <summary>
        /// Saves all package authoring project settings assets back into <c>ProjectSettings</c>.
        /// </summary>
        public static void SaveAllProjectSettings() {
            PackageAuthoringProjectSettings.Instance.SaveSettings();
            SaveTemplateSettings(
                GitIgnoreTemplateSettings.Instance,
                CustomLicenseTemplateSettings.Instance,
                RepositoryReadmeTemplateSettings.Instance);
            SaveDocumentationTemplateSettings();
        }

        /// <summary>
        /// Saves all documentation template and branding settings back into <c>ProjectSettings</c>.
        /// </summary>
        public static void SaveDocumentationTemplateSettings() {
            SaveTemplateSettings(
                DocsGitIgnoreTemplateSettings.Instance,
                DocsApiGitIgnoreTemplateSettings.Instance,
                DocsApiIndexTemplateSettings.Instance,
                DocsDocfxJsonTemplateSettings.Instance,
                DocsDocfxPdfJsonTemplateSettings.Instance,
                DocsFilterConfigTemplateSettings.Instance,
                DocsIndexTemplateSettings.Instance,
                DocsRootTocTemplateSettings.Instance,
                DocsManualTocTemplateSettings.Instance,
                DocsPdfTocTemplateSettings.Instance);
            DocsBrandingImageSettings.Instance.SaveSettings();
        }

        /// <summary>
        /// Restores all documentation templates to the package defaults and saves the updated project settings assets.
        /// </summary>
        public static void ReapplyDocumentationTemplateDefaults() {
            RestoreDefaultContents(
                DocsGitIgnoreTemplateSettings.Instance,
                DocsApiGitIgnoreTemplateSettings.Instance,
                DocsApiIndexTemplateSettings.Instance,
                DocsDocfxJsonTemplateSettings.Instance,
                DocsDocfxPdfJsonTemplateSettings.Instance,
                DocsFilterConfigTemplateSettings.Instance,
                DocsIndexTemplateSettings.Instance,
                DocsRootTocTemplateSettings.Instance,
                DocsManualTocTemplateSettings.Instance,
                DocsPdfTocTemplateSettings.Instance);
            SaveDocumentationTemplateSettings();
        }

        private static void RestoreDefaultContents(params ProjectTemplateAsset[] templateSettings) {
            foreach (ProjectTemplateAsset templateSetting in templateSettings) {
                templateSetting.RestoreDefaultContent();
            }
        }

        private static void SaveTemplateSettings(params ProjectTemplateSettingsAsset[] templateSettings) {
            foreach (ProjectTemplateSettingsAsset templateSetting in templateSettings) {
                templateSetting.SaveSettings();
            }
        }
    }
}
