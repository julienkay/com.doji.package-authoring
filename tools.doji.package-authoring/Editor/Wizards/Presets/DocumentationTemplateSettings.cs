using Doji.PackageAuthoring.Wizards.Templates;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Stores GUIDs for built-in package assets used as documentation branding defaults.
    /// </summary>
    internal static class DocumentationTemplateAssetGuids {
        public const string DefaultFaviconTextureAsset = "bfd9b7566b204ad3ab36d52013f02dce";
        public const string DefaultLogoTextureAsset = "a5bc8f2cf1ab46f79af8cdad680f2cd0";
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/.gitignore</c> template.
    /// </summary>
    internal sealed class DocsGitIgnoreTemplateSettings : ProjectTemplateSettingsBase<DocsGitIgnoreTemplateSettings> {
        protected override string DefaultContent => DocfxTemplates.DocsGitIgnoreDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.DocsGitIgnore;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/api/.gitignore</c> template.
    /// </summary>
    internal sealed class
        DocsApiGitIgnoreTemplateSettings : ProjectTemplateSettingsBase<DocsApiGitIgnoreTemplateSettings> {
        protected override string DefaultContent => DocfxTemplates.DocsApiGitIgnoreDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.DocsApiGitIgnore;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/api/index.md</c> template.
    /// </summary>
    internal sealed class DocsApiIndexTemplateSettings : ProjectTemplateSettingsBase<DocsApiIndexTemplateSettings> {
        protected override string DefaultContent => DocfxTemplates.DocsApiIndexDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.DocsApiIndex;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/docfx.json</c> template.
    /// </summary>
    internal sealed class DocsDocfxJsonTemplateSettings : ProjectTemplateSettingsBase<DocsDocfxJsonTemplateSettings> {
        protected override string DefaultContent => DocfxTemplates.DocfxJsonDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.DocsDocfxJson;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/docfx-pdf.json</c> template.
    /// </summary>
    internal sealed class
        DocsDocfxPdfJsonTemplateSettings : ProjectTemplateSettingsBase<DocsDocfxPdfJsonTemplateSettings> {
        protected override string DefaultContent => DocfxTemplates.DocfxPdfJsonDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.DocsDocfxPdfJson;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/filterConfig.yml</c> template.
    /// </summary>
    internal sealed class
        DocsFilterConfigTemplateSettings : ProjectTemplateSettingsBase<DocsFilterConfigTemplateSettings> {
        protected override string DefaultContent => DocfxTemplates.FilterConfigDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.DocsFilterConfig;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/index.md</c> template.
    /// </summary>
    internal sealed class DocsIndexTemplateSettings : ProjectTemplateSettingsBase<DocsIndexTemplateSettings> {
        protected override string DefaultContent => DocfxTemplates.IndexDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.DocsIndex;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/toc.yml</c> template.
    /// </summary>
    internal sealed class DocsRootTocTemplateSettings : ProjectTemplateSettingsBase<DocsRootTocTemplateSettings> {
        protected override string DefaultContent => DocfxTemplates.RootTocDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.DocsRootToc;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/manual/toc.yml</c> template.
    /// </summary>
    internal sealed class DocsManualTocTemplateSettings : ProjectTemplateSettingsBase<DocsManualTocTemplateSettings> {
        protected override string DefaultContent => DocfxTemplates.ManualTocDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.DocsManualToc;
    }

    /// <summary>
    /// Shared project settings asset for the generated repository <c>docs/pdf/toc.yml</c> template.
    /// </summary>
    internal sealed class DocsPdfTocTemplateSettings : ProjectTemplateSettingsBase<DocsPdfTocTemplateSettings> {
        protected override string DefaultContent => DocfxTemplates.PdfTocDefaultContent;

        protected override string SettingsFilePath => PackageAuthoringTemplateStoragePaths.DocsPdfToc;
    }

    /// <summary>
    /// Shared project settings asset for the optional documentation branding image used to generate docs image outputs.
    /// </summary>
    [FilePath(PackageAuthoringTemplateStoragePaths.DocsBrandingImageAsset, FilePathAttribute.Location.ProjectFolder)]
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

        private void OnDisable() {
            if (_instance == this) {
                _instance = null;
            }
        }

        /// <summary>
        /// Saves the current settings instance back into the project settings asset.
        /// </summary>
        public void SaveSettings() {
            InternalEditorUtility.SaveToSerializedFileAndForget(
                new Object[] {
                    this
                },
                PackageAuthoringTemplateStoragePaths.DocsBrandingImageAsset,
                true);
        }

        private static DocsBrandingImageSettings GetOrCreateSettings() {
            Object[] settings = InternalEditorUtility.LoadSerializedFileAndForget(
                PackageAuthoringTemplateStoragePaths.DocsBrandingImageAsset);
            if (settings.Length > 0 && settings[0] is DocsBrandingImageSettings existingSettings) {
                return existingSettings;
            }

            DocsBrandingImageSettings created = CreateInstance<DocsBrandingImageSettings>();
            created.hideFlags = HideFlags.HideAndDontSave;
            created.AssignDefaultTextures();
            return created;
        }

        private void AssignDefaultTextures() {
            FaviconTexture = LoadTextureFromGuid(DocumentationTemplateAssetGuids.DefaultFaviconTextureAsset);
            LogoTexture = LoadTextureFromGuid(DocumentationTemplateAssetGuids.DefaultLogoTextureAsset);
        }

        private static Texture2D LoadTextureFromGuid(string guid) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            if (string.IsNullOrEmpty(assetPath)) {
                Debug.LogWarning($"Could not resolve documentation default texture GUID '{guid}'.");
                return null;
            }

            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
        }
    }
}
