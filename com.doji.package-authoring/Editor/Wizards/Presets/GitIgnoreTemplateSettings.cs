using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating project <c>.gitignore</c> files.
    /// </summary>
    [FilePath("ProjectSettings/PackageAuthoringGitIgnoreTemplate.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class GitIgnoreTemplateSettings : ProjectTemplateSettingsBase<GitIgnoreTemplateSettings> {
        protected override string DefaultContent => Templates.GitIgnoreTemplate.DefaultContent;
        
        protected override string AssetPath => "ProjectSettings/PackageAuthoringGitIgnoreTemplate.asset";
    }
}
