using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Project-scoped editable template used when generating project <c>.gitignore</c> files.
    /// </summary>
    [FilePath("ProjectSettings/PackageAuthoringGitIgnoreTemplate.asset", FilePathAttribute.Location.ProjectFolder)]
    internal sealed class GitIgnoreTemplateSettings : ProjectTemplateAsset {
        private static GitIgnoreTemplateSettings _instance;

        protected override string DefaultContent => Templates.GitIgnoreTemplate.DefaultContent;

        /// <summary>
        /// Shared project settings asset for the generated project <c>.gitignore</c> template.
        /// </summary>
        public static GitIgnoreTemplateSettings Instance => _instance ??= GetOrCreateSettings();

        /// <summary>
        /// Saves the current settings instance back into the project settings asset.
        /// </summary>
        public void SaveSettings() {
            Save(true);
        }

        private void OnDisable() {
            if (_instance == this) {
                _instance = null;
            }
        }

        private static GitIgnoreTemplateSettings GetOrCreateSettings() {
            GitIgnoreTemplateSettings settings = LoadOrCreate();
            settings.EnsureDefaultContent();
            return settings;
        }

        private static GitIgnoreTemplateSettings LoadOrCreate() {
            Object[] settings =
                InternalEditorUtility.LoadSerializedFileAndForget(
                    "ProjectSettings/PackageAuthoringGitIgnoreTemplate.asset");
            if (settings.Length > 0 && settings[0] is GitIgnoreTemplateSettings gitIgnoreTemplateSettings) {
                return gitIgnoreTemplateSettings;
            }

            GitIgnoreTemplateSettings created = CreateInstance<GitIgnoreTemplateSettings>();
            created.hideFlags = HideFlags.HideAndDontSave;
            return created;
        }

        private void Save(bool saveAsText) {
            EnsureDefaultContent();
            InternalEditorUtility.SaveToSerializedFileAndForget(
                new Object[] {
                    this
                },
                "ProjectSettings/PackageAuthoringGitIgnoreTemplate.asset",
                saveAsText);
        }
    }
}
