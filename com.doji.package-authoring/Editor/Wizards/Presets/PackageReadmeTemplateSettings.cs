using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating package <c>README.md</c> files.
    /// </summary>
    [FilePath("ProjectSettings/PackageAuthoringPackageReadmeTemplate.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class PackageReadmeTemplateSettings : ProjectTemplateSettingsBase<PackageReadmeTemplateSettings> {
        protected override string DefaultContent => Templates.ReadmeTemplate.PackageReadmeDefaultContent;

        protected override string AssetPath => "ProjectSettings/PackageAuthoringPackageReadmeTemplate.asset";
    }
}
