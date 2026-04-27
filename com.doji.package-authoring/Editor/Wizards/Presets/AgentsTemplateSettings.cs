namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating repository <c>AGENTS.md</c> files.
    /// </summary>
    internal sealed class AgentsTemplateSettings : ProjectTemplateSettingsBase<AgentsTemplateSettings> {
        protected override string DefaultContent => Templates.AgentsTemplate.DefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.RepositoryAgents;
    }
}
