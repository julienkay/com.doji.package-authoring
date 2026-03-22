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
            GitIgnoreTemplateSettings.Instance.SaveSettings();
            CustomLicenseTemplateSettings.Instance.SaveSettings();
            SaveDocumentationTemplateSettings();
        }

        /// <summary>
        /// Saves all documentation template and branding settings back into <c>ProjectSettings</c>.
        /// </summary>
        public static void SaveDocumentationTemplateSettings() {
            DocsGitIgnoreTemplateSettings.Instance.SaveSettings();
            DocsApiGitIgnoreTemplateSettings.Instance.SaveSettings();
            DocsApiIndexTemplateSettings.Instance.SaveSettings();
            DocsDocfxJsonTemplateSettings.Instance.SaveSettings();
            DocsDocfxPdfJsonTemplateSettings.Instance.SaveSettings();
            DocsFilterConfigTemplateSettings.Instance.SaveSettings();
            DocsIndexTemplateSettings.Instance.SaveSettings();
            DocsRootTocTemplateSettings.Instance.SaveSettings();
            DocsManualTocTemplateSettings.Instance.SaveSettings();
            DocsPdfTocTemplateSettings.Instance.SaveSettings();
            DocsBrandingImageSettings.Instance.SaveSettings();
        }

        /// <summary>
        /// Restores all documentation templates to the package defaults and saves the updated project settings assets.
        /// </summary>
        public static void ReapplyDocumentationTemplateDefaults() {
            DocsGitIgnoreTemplateSettings.Instance.RestoreDefaultContent();
            DocsApiGitIgnoreTemplateSettings.Instance.RestoreDefaultContent();
            DocsApiIndexTemplateSettings.Instance.RestoreDefaultContent();
            DocsDocfxJsonTemplateSettings.Instance.RestoreDefaultContent();
            DocsDocfxPdfJsonTemplateSettings.Instance.RestoreDefaultContent();
            DocsFilterConfigTemplateSettings.Instance.RestoreDefaultContent();
            DocsIndexTemplateSettings.Instance.RestoreDefaultContent();
            DocsRootTocTemplateSettings.Instance.RestoreDefaultContent();
            DocsManualTocTemplateSettings.Instance.RestoreDefaultContent();
            DocsPdfTocTemplateSettings.Instance.RestoreDefaultContent();
            SaveDocumentationTemplateSettings();
        }
    }
}
