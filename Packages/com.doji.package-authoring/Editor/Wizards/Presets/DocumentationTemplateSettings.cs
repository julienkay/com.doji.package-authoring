using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Stores the project settings asset paths used by generated documentation scaffold templates.
    /// </summary>
    internal static class DocumentationTemplateAssetPaths {
        public const string DocsGitIgnore = "ProjectSettings/PackageAuthoringDocsGitIgnoreTemplate.asset";
        public const string DocsApiGitIgnore = "ProjectSettings/PackageAuthoringDocsApiGitIgnoreTemplate.asset";
        public const string DocsApiIndex = "ProjectSettings/PackageAuthoringDocsApiIndexTemplate.asset";
        public const string DocsDocfxJson = "ProjectSettings/PackageAuthoringDocsDocfxJsonTemplate.asset";
        public const string DocsDocfxPdfJson = "ProjectSettings/PackageAuthoringDocsDocfxPdfJsonTemplate.asset";
        public const string DocsFilterConfig = "ProjectSettings/PackageAuthoringDocsFilterConfigTemplate.asset";
        public const string DocsIndex = "ProjectSettings/PackageAuthoringDocsIndexTemplate.asset";
        public const string DocsRootToc = "ProjectSettings/PackageAuthoringDocsRootTocTemplate.asset";
        public const string DocsManualToc = "ProjectSettings/PackageAuthoringDocsManualTocTemplate.asset";
        public const string DocsPdfToc = "ProjectSettings/PackageAuthoringDocsPdfTocTemplate.asset";
    }

    /// <summary>
    /// Implements the shared persistence flow for documentation template settings assets.
    /// </summary>
    internal abstract class DocumentationTemplateSettingsBase<T> : ProjectTemplateAsset
        where T : DocumentationTemplateSettingsBase<T> {
        private static T _instance;

        protected abstract string AssetPath { get; }

        /// <summary>
        /// Shared project settings asset instance for this documentation template.
        /// </summary>
        public static T Instance => _instance ??= GetOrCreateSettings();

        /// <summary>
        /// Saves the current settings instance back into the project settings asset.
        /// </summary>
        public void SaveSettings() {
            SaveSettingsAsset(AssetPath);
        }

        private void OnDisable() {
            if (_instance == this) {
                _instance = null;
            }
        }

        private static T GetOrCreateSettings() {
            T settings = LoadOrCreateSettings<T>(GetAssetPath());
            settings.EnsureDefaultContent();
            return settings;
        }

        private static string GetAssetPath() {
            T templateSettings = CreateInstance<T>();
            string assetPath = templateSettings.AssetPath;
            Object.DestroyImmediate(templateSettings);
            return assetPath;
        }
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/.gitignore</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsGitIgnore, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsGitIgnoreTemplateSettings : DocumentationTemplateSettingsBase<DocsGitIgnoreTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.DocsGitIgnoreDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsGitIgnore;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/api/.gitignore</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsApiGitIgnore, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsApiGitIgnoreTemplateSettings : DocumentationTemplateSettingsBase<DocsApiGitIgnoreTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.DocsApiGitIgnoreDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsApiGitIgnore;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/api/index.md</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsApiIndex, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsApiIndexTemplateSettings : DocumentationTemplateSettingsBase<DocsApiIndexTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.DocsApiIndexDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsApiIndex;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/docfx.json</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsDocfxJson, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsDocfxJsonTemplateSettings : DocumentationTemplateSettingsBase<DocsDocfxJsonTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.DocfxJsonDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsDocfxJson;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/docfx-pdf.json</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsDocfxPdfJson, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsDocfxPdfJsonTemplateSettings : DocumentationTemplateSettingsBase<DocsDocfxPdfJsonTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.DocfxPdfJsonDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsDocfxPdfJson;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/filterConfig.yml</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsFilterConfig, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsFilterConfigTemplateSettings : DocumentationTemplateSettingsBase<DocsFilterConfigTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.FilterConfigDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsFilterConfig;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/index.md</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsIndex, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsIndexTemplateSettings : DocumentationTemplateSettingsBase<DocsIndexTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.IndexDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsIndex;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/toc.yml</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsRootToc, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsRootTocTemplateSettings : DocumentationTemplateSettingsBase<DocsRootTocTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.RootTocDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsRootToc;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/manual/toc.yml</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsManualToc, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsManualTocTemplateSettings : DocumentationTemplateSettingsBase<DocsManualTocTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.ManualTocDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsManualToc;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/pdf/toc.yml</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsPdfToc, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsPdfTocTemplateSettings : DocumentationTemplateSettingsBase<DocsPdfTocTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.PdfTocDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsPdfToc;
    }
}
