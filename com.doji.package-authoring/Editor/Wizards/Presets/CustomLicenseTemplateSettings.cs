using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating repository <c>LICENSE</c> files for custom licenses.
    /// </summary>
    [FilePath("ProjectSettings/PackageAuthoringCustomLicenseTemplate.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class CustomLicenseTemplateSettings : ProjectTemplateSettingsBase<CustomLicenseTemplateSettings> {
        protected override string DefaultContent => Templates.CustomLicenseTemplate.DefaultContent;
        
        protected override string AssetPath => "ProjectSettings/PackageAuthoringCustomLicenseTemplate.asset";
    }
}
