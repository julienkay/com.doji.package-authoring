using System.Collections.Generic;

namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Defines the common persistence contract for package authoring project settings assets.
    /// </summary>
    internal interface IPackageAuthoringProjectSettingsAsset {
        /// <summary>
        /// Persists the current in-memory settings back into <c>ProjectSettings</c>.
        /// </summary>
        void SaveSettings();
    }

    /// <summary>
    /// Defines the shared default-reset contract for editable template settings assets.
    /// </summary>
    internal interface IPackageAuthoringTemplateSettingsAsset : IPackageAuthoringProjectSettingsAsset {
        /// <summary>
        /// Replaces the current customized content with the built-in template default.
        /// </summary>
        void RestoreDefaultContent();
    }

    /// <summary>
    /// Centralizes membership for package authoring project settings assets so save and reset operations stay in sync.
    /// </summary>
    internal static class PackageAuthoringSettingsRegistry {
        /// <summary>
        /// Package authoring settings assets that are persisted but do not expose template default restoration.
        /// </summary>
        internal static IReadOnlyList<IPackageAuthoringProjectSettingsAsset> NonTemplatePersistedSettingsAssets { get; } = new IPackageAuthoringProjectSettingsAsset[] {
            PackageAuthoringProjectSettings.Instance,
            DocsBrandingImageSettings.Instance
        };

        /// <summary>
        /// Editable template settings assets that can both persist their state and restore package-provided default content.
        /// </summary>
        /// <remarks>
        /// This collection is a behavioral subset of <see cref="AllPersistedSettingsAssets"/>. Every entry here also
        /// participates in the persisted settings list because template settings must be saved after edits or resets.
        /// </remarks>
        internal static IReadOnlyList<IPackageAuthoringTemplateSettingsAsset> AllResettableTemplateSettingsAssets { get; } = new IPackageAuthoringTemplateSettingsAsset[] {
            PackageReadmeTemplateSettings.Instance,
            GitIgnoreTemplateSettings.Instance,
            CustomLicenseTemplateSettings.Instance,
            RepositoryReadmeTemplateSettings.Instance,
            DocsGitIgnoreTemplateSettings.Instance,
            DocsApiGitIgnoreTemplateSettings.Instance,
            DocsApiIndexTemplateSettings.Instance,
            DocsDocfxJsonTemplateSettings.Instance,
            DocsDocfxPdfJsonTemplateSettings.Instance,
            DocsFilterConfigTemplateSettings.Instance,
            DocsIndexTemplateSettings.Instance,
            DocsRootTocTemplateSettings.Instance,
            DocsManualTocTemplateSettings.Instance,
            DocsPdfTocTemplateSettings.Instance
        };

        /// <summary>
        /// All package authoring settings assets that should be persisted together, whether or not they are editable templates.
        /// </summary>
        /// <remarks>
        /// This combines <see cref="NonTemplatePersistedSettingsAssets"/> with
        /// <see cref="AllResettableTemplateSettingsAssets"/> so save operations can treat package authoring settings as one set.
        /// </remarks>
        internal static IEnumerable<IPackageAuthoringProjectSettingsAsset> AllPersistedSettingsAssets {
            get {
                foreach (IPackageAuthoringProjectSettingsAsset settingsAsset in NonTemplatePersistedSettingsAssets) {
                    yield return settingsAsset;
                }

                foreach (IPackageAuthoringTemplateSettingsAsset templateSettingsAsset in AllResettableTemplateSettingsAssets) {
                    yield return templateSettingsAsset;
                }
            }
        }
    }
}
