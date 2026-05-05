using Doji.PackageAuthoring.Wizards.Templates;

namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating repository <c>AGENTS.md</c> files.
    /// </summary>
    internal sealed class AgentsTemplateSettings : ProjectTemplateSettingsBase<AgentsTemplateSettings> {
        protected override string DefaultContent => AgentsTemplate.DefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.RepositoryAgents;
    }
}
