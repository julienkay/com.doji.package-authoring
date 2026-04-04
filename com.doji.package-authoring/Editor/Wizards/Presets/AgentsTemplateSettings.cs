using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating repository <c>AGENTS.md</c> files.
    /// </summary>
    [FilePath("ProjectSettings/PackageAuthoringAgentsTemplate.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class AgentsTemplateSettings : ProjectTemplateSettingsBase<AgentsTemplateSettings> {
        protected override string DefaultContent => Templates.AgentsTemplate.DefaultContent;

        protected override string AssetPath => "ProjectSettings/PackageAuthoringAgentsTemplate.asset";
    }
}
