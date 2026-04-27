namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating repository <c>LICENSE</c> files for custom licenses.
    /// </summary>
    internal sealed class CustomLicenseTemplateSettings : ProjectTemplateSettingsBase<CustomLicenseTemplateSettings> {
        protected override string DefaultContent => Templates.CustomLicenseTemplate.DefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.RepositoryCustomLicense;
    }
}
