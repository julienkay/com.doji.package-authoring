using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Stores the project settings asset paths used by generated documentation scaffold templates.
    /// </summary>
    internal static class DocumentationTemplateAssetPaths {
        public const string DefaultLogoTextureAsset = "Packages/com.doji.package-authoring/Editor/Defaults/Documentation/logo-template.png";
        public const string DefaultFaviconTextureAsset = "Packages/com.doji.package-authoring/Editor/Defaults/Documentation/favicon-template.png";
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
        public const string DocsBrandingImage = "ProjectSettings/PackageAuthoringDocsBrandingImage.asset";
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/.gitignore</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsGitIgnore, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsGitIgnoreTemplateSettings : ProjectTemplateSettingsBase<DocsGitIgnoreTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.DocsGitIgnoreDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsGitIgnore;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/api/.gitignore</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsApiGitIgnore, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsApiGitIgnoreTemplateSettings : ProjectTemplateSettingsBase<DocsApiGitIgnoreTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.DocsApiGitIgnoreDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsApiGitIgnore;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/api/index.md</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsApiIndex, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsApiIndexTemplateSettings : ProjectTemplateSettingsBase<DocsApiIndexTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.DocsApiIndexDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsApiIndex;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/docfx.json</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsDocfxJson, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsDocfxJsonTemplateSettings : ProjectTemplateSettingsBase<DocsDocfxJsonTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.DocfxJsonDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsDocfxJson;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/docfx-pdf.json</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsDocfxPdfJson, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsDocfxPdfJsonTemplateSettings : ProjectTemplateSettingsBase<DocsDocfxPdfJsonTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.DocfxPdfJsonDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsDocfxPdfJson;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/filterConfig.yml</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsFilterConfig, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsFilterConfigTemplateSettings : ProjectTemplateSettingsBase<DocsFilterConfigTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.FilterConfigDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsFilterConfig;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/index.md</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsIndex, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsIndexTemplateSettings : ProjectTemplateSettingsBase<DocsIndexTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.IndexDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsIndex;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/toc.yml</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsRootToc, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsRootTocTemplateSettings : ProjectTemplateSettingsBase<DocsRootTocTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.RootTocDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsRootToc;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/manual/toc.yml</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsManualToc, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsManualTocTemplateSettings : ProjectTemplateSettingsBase<DocsManualTocTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.ManualTocDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsManualToc;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/pdf/toc.yml</c> template.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsPdfToc, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsPdfTocTemplateSettings : ProjectTemplateSettingsBase<DocsPdfTocTemplateSettings> {
        protected override string DefaultContent => Templates.DocfxTemplates.PdfTocDefaultContent;

        protected override string AssetPath => DocumentationTemplateAssetPaths.DocsPdfToc;
    }

    /// <summary>
    /// Shared project settings asset for the optional documentation branding image used to generate docs image outputs.
    /// </summary>
    [FilePath(DocumentationTemplateAssetPaths.DocsBrandingImage, FilePathAttribute.Location.ProjectFolder)]
    internal sealed class DocsBrandingImageSettings : ScriptableObject, IPackageAuthoringProjectSettingsAsset {
        private static DocsBrandingImageSettings _instance;

        /// <summary>
        /// Source texture used to generate <c>docs/images/favicon.ico</c>.
        /// </summary>
        [field: SerializeField]
        public Texture2D FaviconTexture { get; set; }

        /// <summary>
        /// Source texture used to generate <c>docs/images/logo.png</c>.
        /// </summary>
        [field: SerializeField]
        public Texture2D LogoTexture { get; set; }

        /// <summary>
        /// Shared project settings asset instance for the documentation branding image.
        /// </summary>
        public static DocsBrandingImageSettings Instance => _instance ??= GetOrCreateSettings();

        /// <summary>
        /// Whether at least one source texture has been assigned for docs image generation.
        /// </summary>
        public bool HasAnyImage => FaviconTexture != null || LogoTexture != null;

        /// <summary>
        /// Saves the current settings instance back into the project settings asset.
        /// </summary>
        public void SaveSettings() {
            UnityEditorInternal.InternalEditorUtility.SaveToSerializedFileAndForget(
                new Object[] {
                    this
                },
                DocumentationTemplateAssetPaths.DocsBrandingImage,
                true);
        }

        private void OnDisable() {
            if (_instance == this) {
                _instance = null;
            }
        }

        private static DocsBrandingImageSettings GetOrCreateSettings() {
            Object[] settings = UnityEditorInternal.InternalEditorUtility.LoadSerializedFileAndForget(
                DocumentationTemplateAssetPaths.DocsBrandingImage);
            if (settings.Length > 0 && settings[0] is DocsBrandingImageSettings existingSettings) {
                return existingSettings;
            }

            DocsBrandingImageSettings created = CreateInstance<DocsBrandingImageSettings>();
            created.hideFlags = HideFlags.HideAndDontSave;
            created.AssignDefaultTextures();
            return created;
        }

        private void AssignDefaultTextures() {
            FaviconTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(DocumentationTemplateAssetPaths.DefaultFaviconTextureAsset);
            LogoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(DocumentationTemplateAssetPaths.DefaultLogoTextureAsset);
        }
    }
}
