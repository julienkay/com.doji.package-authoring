namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
     /// Project-scoped editable template used when generating project <c>.gitignore</c> files.
     /// </summary>
    internal sealed class GitIgnoreTemplateSettings : ProjectTemplateSettingsBase<GitIgnoreTemplateSettings> {
        protected override string DefaultContent => Templates.GitIgnoreTemplate.DefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.PackageGitIgnore;
    }
}
