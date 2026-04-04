using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating repository <c>README.md</c> files.
    /// </summary>
    [FilePath("ProjectSettings/PackageAuthoringRepositoryReadmeTemplate.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class RepositoryReadmeTemplateSettings : ProjectTemplateSettingsBase<RepositoryReadmeTemplateSettings> {
        protected override string DefaultContent => Templates.ReadmeTemplate.RepositoryReadmeDefaultContent;

        protected override string AssetPath => "ProjectSettings/PackageAuthoringRepositoryReadmeTemplate.asset";
    }
}
