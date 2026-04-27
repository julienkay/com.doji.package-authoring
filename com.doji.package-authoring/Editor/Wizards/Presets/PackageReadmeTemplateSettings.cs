namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating package <c>README.md</c> files.
    /// </summary>
    internal sealed class PackageReadmeTemplateSettings : ProjectTemplateSettingsBase<PackageReadmeTemplateSettings> {
        protected override string DefaultContent => Templates.ReadmeTemplate.PackageReadmeDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.PackageReadme;
    }
}
