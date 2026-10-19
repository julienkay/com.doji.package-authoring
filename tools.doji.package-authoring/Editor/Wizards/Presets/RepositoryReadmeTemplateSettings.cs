using Doji.PackageAuthoring.Wizards.Templates;

namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating repository <c>README.md</c> files.
    /// </summary>
    internal sealed class
        RepositoryReadmeTemplateSettings : ProjectTemplateSettingsBase<RepositoryReadmeTemplateSettings> {
        protected override string DefaultContent => ReadmeTemplate.RepositoryReadmeDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.RepositoryReadme;
    }
}
