using Doji.PackageAuthoring.Editor.Wizards.Models;
using Doji.PackageAuthoring.Editor.Wizards.Templates;
using UnityEditorInternal;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Base asset for project-scoped editable templates that resolve shared package authoring tokens.
    /// </summary>
    internal abstract class ProjectTemplateAsset : ScriptableObject {
        /// <summary>
        /// Built-in fallback content used when the project has not customized the template yet.
        /// </summary>
        protected abstract string DefaultContent { get; }

        /// <summary>
        /// Raw template content edited in Project Settings.
        /// </summary>
        [field: SerializeField]
        public string Content { get; set; }

        /// <summary>
        /// Returns the template content with shared tokens resolved against the current scaffold context.
        /// </summary>
        public virtual string GetResolvedContent(PackageContext ctx) {
            return TemplateTokenResolver.Resolve(Content, ctx);
        }

        /// <summary>
        /// Returns the template content with shared tokens resolved from the provided settings objects.
        /// </summary>
        public virtual string GetResolvedContent(
            ProjectSettings project,
            PackageSettings package = null,
            RepoSettings repo = null) {
            return TemplateTokenResolver.Resolve(Content, project, package, repo);
        }

        /// <summary>
        /// Ensures the template content is initialized to the built-in default when absent.
        /// </summary>
        protected void EnsureDefaultContent() {
            if (Content == null) {
                Content = DefaultContent;
            }
        }

        /// <summary>
        /// Replaces the current template content with the built-in default content.
        /// </summary>
        internal void RestoreDefaultContent() {
            Content = DefaultContent;
        }

        /// <summary>
        /// Restores a template settings object from the project's <c>ProjectSettings</c> folder, or falls back to a transient in-memory instance.
        /// </summary>
        /// <remarks>
        /// Unity editor configuration is often stored either as importable assets under <c>Assets</c>/<c>Packages</c> or as
        /// serialized objects under <c>ProjectSettings</c>. These template presets intentionally use the latter so the package
        /// can provide built-in defaults without shipping mutable template files as real package assets.
        ///
        /// <see cref="InternalEditorUtility.LoadSerializedFileAndForget(string)"/> is the usual low-level API for this pattern:
        /// it reads arbitrary serialized Unity objects from a known path even when they are not part of the Asset Database.
        /// When the file does not exist yet, callers still need a working instance so the editor UI can expose default content
        /// and later save the customized version back to disk. That is why this helper creates a hidden transient object instead
        /// of returning <c>null</c>.
        ///
        /// This is typically used by project-scoped editor settings types that behave similarly to <c>ScriptableSingleton</c>
        /// but need explicit control over inheritance, initialization, or save timing.
        /// </remarks>
        protected static T LoadOrCreateSettings<T>(string assetPath)
            where T : ProjectTemplateAsset {
            Object[] settings = InternalEditorUtility.LoadSerializedFileAndForget(assetPath);
            if (settings.Length > 0 && settings[0] is T existingSettings) {
                return existingSettings;
            }

            T created = CreateInstance<T>();
            created.hideFlags = HideFlags.HideAndDontSave;
            return created;
        }

        /// <summary>
        /// Saves the current template asset back into the project settings folder.
        /// </summary>
        protected void SaveSettingsAsset(string assetPath, bool saveAsText = true) {
            EnsureDefaultContent();
            InternalEditorUtility.SaveToSerializedFileAndForget(
                new Object[] {
                    this
                },
                assetPath,
                saveAsText);
        }
    }

    /// <summary>
    /// Base asset for project-scoped templates that know how to persist themselves back into <c>ProjectSettings</c>.
    /// </summary>
    internal abstract class ProjectTemplateSettingsAsset : ProjectTemplateAsset {
        /// <summary>
        /// Serialized project settings path used when this template asset is saved.
        /// </summary>
        protected abstract string AssetPath { get; }

        /// <summary>
        /// Saves the current template settings instance back into the project settings asset.
        /// </summary>
        public void SaveSettings() {
            SaveSettingsAsset(AssetPath);
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

        protected virtual void InitializeSettings() {
            EnsureDefaultContent();
        }

        private void OnDisable() {
            if (_instance == this) {
                _instance = null;
            }
        }

        private static T GetOrCreateSettings() {
            T settings = LoadOrCreateSettings<T>(GetAssetPath());
            settings.InitializeSettings();
            return settings;
        }

        private static string GetAssetPath() {
            T templateSettings = CreateInstance<T>();
            string assetPath = templateSettings.AssetPath;
            Object.DestroyImmediate(templateSettings);
            return assetPath;
        }
    }
}
