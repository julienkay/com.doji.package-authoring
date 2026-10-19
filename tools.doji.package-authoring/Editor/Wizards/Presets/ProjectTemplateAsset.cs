using System.IO;
using Doji.PackageAuthoring.Models;
using Doji.PackageAuthoring.Wizards.Templates;
using UnityEngine;

namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Resolves file-backed template paths under <c>ProjectSettings/PackageAuthoringTemplates</c>.
    /// </summary>
    internal static class PackageAuthoringTemplateStoragePaths {
        private const string TemplatesRoot = "ProjectSettings/PackageAuthoringTemplates";

        public const string PackageGitIgnore = $"{TemplatesRoot}/Package/.gitignore";
        public const string PackageReadme = $"{TemplatesRoot}/Package/README.md";
        public const string RepositoryReadme = $"{TemplatesRoot}/Repository/README.md";
        public const string RepositoryAgents = $"{TemplatesRoot}/Repository/AGENTS.md";
        public const string RepositoryCustomLicense = $"{TemplatesRoot}/Repository/CustomLicense.txt";
        public const string DocsGitIgnore = $"{TemplatesRoot}/Documentation/.gitignore";
        public const string DocsApiGitIgnore = $"{TemplatesRoot}/Documentation/api/.gitignore";
        public const string DocsApiIndex = $"{TemplatesRoot}/Documentation/api/index.md";
        public const string DocsDocfxJson = $"{TemplatesRoot}/Documentation/docfx.json";
        public const string DocsDocfxPdfJson = $"{TemplatesRoot}/Documentation/docfx-pdf.json";
        public const string DocsFilterConfig = $"{TemplatesRoot}/Documentation/filterConfig.yml";
        public const string DocsIndex = $"{TemplatesRoot}/Documentation/index.md";
        public const string DocsRootToc = $"{TemplatesRoot}/Documentation/toc.yml";
        public const string DocsManualToc = $"{TemplatesRoot}/Documentation/manual/toc.yml";
        public const string DocsPdfToc = $"{TemplatesRoot}/Documentation/pdf/toc.yml";
        public const string DocsBrandingImageAsset = "ProjectSettings/PackageAuthoringDocsBrandingImage.asset";
    }

    /// <summary>
    /// Handles project-root-relative template file IO.
    /// </summary>
    internal static class ProjectTemplateStorage {
        private static string _projectRootPathOverride;

        internal static string LoadContent(string settingsFilePath, string defaultContent) {
            string absoluteSettingsPath = GetAbsolutePath(settingsFilePath);
            if (File.Exists(absoluteSettingsPath)) {
                return File.ReadAllText(absoluteSettingsPath);
            }

            return defaultContent;
        }

        internal static void SaveContent(string settingsFilePath, string content) {
            string absoluteSettingsPath = GetAbsolutePath(settingsFilePath);
            string directoryPath = Path.GetDirectoryName(absoluteSettingsPath);
            if (!string.IsNullOrEmpty(directoryPath)) {
                Directory.CreateDirectory(directoryPath);
            }

            File.WriteAllText(absoluteSettingsPath, content ?? string.Empty);
        }

        internal static void OverrideProjectRootPath(string projectRootPath) {
            _projectRootPathOverride = projectRootPath;
        }

        internal static void ClearProjectRootPathOverride() {
            _projectRootPathOverride = null;
        }

        private static string GetAbsolutePath(string relativePath) {
            return Path.GetFullPath(Path.Combine(GetProjectRootPath(), relativePath));
        }

        private static string GetProjectRootPath() {
            if (!string.IsNullOrWhiteSpace(_projectRootPathOverride)) {
                return _projectRootPathOverride;
            }

            return Directory.GetParent(Application.dataPath)?.FullName ?? Directory.GetCurrentDirectory();
        }
    }

    /// <summary>
    /// Base in-memory settings object for project-scoped editable templates that resolve shared package authoring tokens.
    /// </summary>
    internal abstract class ProjectTemplateAsset : ScriptableObject {
        /// <summary>
        /// Raw template content edited in Project Settings.
        /// </summary>
        [field: SerializeField]
        public string Content { get; set; }

        /// <summary>
        /// Built-in fallback content used when the project has not customized the template yet.
        /// </summary>
        protected abstract string DefaultContent { get; }

        /// <summary>
        /// Project-root-relative file that stores the editable template content.
        /// </summary>
        protected abstract string SettingsFilePath { get; }

        /// <summary>
        /// Returns the template content with shared tokens resolved against the current scaffold context.
        /// </summary>
        public virtual string GetResolvedContent(PackageContext ctx) {
            RefreshContentFromStorage();
            return TemplateTokenResolver.Resolve(Content, ctx);
        }

        /// <summary>
        /// Returns the template content with shared tokens resolved from the provided settings objects.
        /// </summary>
        public virtual string GetResolvedContent(
            ProjectSettings project,
            PackageSettings package = null,
            RepoSettings repo = null) {
            RefreshContentFromStorage();
            return TemplateTokenResolver.Resolve(Content, project, package, repo);
        }

        /// <summary>
        /// Ensures the in-memory template content has been populated from the file-backed settings store.
        /// </summary>
        protected void EnsureContentLoaded() {
            if (Content == null) {
                RefreshContentFromStorage();
            }
        }

        /// <summary>
        /// Refreshes the in-memory template content from current on-disk settings file.
        /// </summary>
        protected void RefreshContentFromStorage() {
            Content = ProjectTemplateStorage.LoadContent(SettingsFilePath, DefaultContent);
        }

        /// <summary>
        /// Replaces the current template content with the built-in default content.
        /// </summary>
        internal void RestoreDefaultContent() {
            Content = DefaultContent;
        }

        /// <summary>
        /// Initializes the transient settings object from the current on-disk template state.
        /// </summary>
        protected virtual void InitializeSettings() {
            EnsureContentLoaded();
        }

        /// <summary>
        /// Saves the current template content back into <c>ProjectSettings/PackageAuthoringTemplates</c>.
        /// </summary>
        protected void SaveSettingsFile() {
            EnsureContentLoaded();
            ProjectTemplateStorage.SaveContent(SettingsFilePath, Content);
        }
    }

    /// <summary>
    /// Base settings object for project-scoped templates that know how to persist themselves back into plain text files.
    /// </summary>
    internal abstract class
        ProjectTemplateSettingsAsset : ProjectTemplateAsset, IPackageAuthoringTemplateSettingsAsset {
        /// <summary>
        /// Saves the current template settings instance back into the project settings file.
        /// </summary>
        public void SaveSettings() {
            SaveSettingsFile();
        }

        void IPackageAuthoringTemplateSettingsAsset.RestoreDefaultContent() {
            RestoreDefaultContent();
        }
    }

    /// <summary>
    /// Generic singleton-style loader for project-scoped template settings stored under <c>ProjectSettings</c>.
    /// </summary>
    internal abstract class ProjectTemplateSettingsBase<T> : ProjectTemplateSettingsAsset
        where T : ProjectTemplateSettingsBase<T> {
        private static T _instance;

        /// <summary>
        /// Shared project settings asset instance for this template settings type.
        /// </summary>
        public static T Instance => _instance ??= GetOrCreateSettings();

        private void OnDisable() {
            if (_instance == this) {
                _instance = null;
            }
        }

        private static T GetOrCreateSettings() {
            T settings = CreateInstance<T>();
            settings.hideFlags = HideFlags.HideAndDontSave;
            settings.InitializeSettings();
            return settings;
        }
    }
}
