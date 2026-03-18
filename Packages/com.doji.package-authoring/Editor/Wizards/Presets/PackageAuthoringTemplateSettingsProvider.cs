using UnityEditor;
using UnityEngine;
using Doji.PackageAuthoring.Editor.Wizards.UI;

namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Registers and renders the Project Settings page for editable generation templates.
    /// </summary>
    internal static class PackageAuthoringTemplateSettingsProvider {
        private static readonly string ContentField = $"<{nameof(GitIgnoreTemplateSettings.Content)}>k__BackingField";
        private static readonly GUIContent GitIgnoreTemplateLabel = new(
            ".gitignore",
            "Template content written to generated project .gitignore files.");
        private static readonly GUIContent CustomLicenseTemplateLabel = new(
            "Custom License",
            "Template content written to generated repository LICENSE files when Open Source License is set to Custom.");

        /// <summary>
        /// Creates the parent settings provider shown under <c>Project/Doji/Package Authoring/Templates</c>.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateTemplatesProvider() {
            return new SettingsProvider("Project/Doji/Package Authoring/Templates", SettingsScope.Project) {
                guiHandler = _ => DrawTemplatesGui()
            };
        }

        /// <summary>
        /// Creates the child settings provider for the generated project <c>.gitignore</c> template.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateGitIgnoreTemplateProvider() {
            return new SettingsProvider("Project/Doji/Package Authoring/Templates/.gitignore", SettingsScope.Project) {
                guiHandler = _ => DrawGitIgnoreSection()
            };
        }

        /// <summary>
        /// Creates the child settings provider for the generated custom repository license template.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateCustomLicenseTemplateProvider() {
            return new SettingsProvider("Project/Doji/Package Authoring/Templates/Custom License", SettingsScope.Project) {
                guiHandler = _ => DrawCustomLicenseSection()
            };
        }

        /// <summary>
        /// Draws the parent templates landing page.
        /// </summary>
        private static void DrawTemplatesGui() {
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                "Select a template subsection to edit the generated file content.",
                MessageType.Info);

            EditorGUILayout.Space(8f);
            PackageAuthoringGui.DrawSection("Available Tokens", () => {
                EditorGUILayout.LabelField(
                    "Copy these placeholders into project-scoped templates. They are resolved during preview and generation when the required context is available.",
                    EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(4f);
                EditorGUILayout.SelectableLabel(
                    Templates.TemplateTokenResolver.SupportedTokensReferenceText,
                    EditorStyles.textArea,
                    GUILayout.MinHeight(180f));
            });
        }

        private static void DrawGitIgnoreSection() {
            GitIgnoreTemplateSettings settings = GitIgnoreTemplateSettings.Instance;
            using SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.Update();

            PackageAuthoringGui.DrawSection(".gitignore Template", () => {
                SerializedProperty contentProperty = serializedSettings.FindProperty(ContentField);
                DrawTemplateTextArea(contentProperty, minHeight: 420f);
            });

            if (serializedSettings.ApplyModifiedProperties()) {
                settings.SaveSettings();
            }
        }

        private static void DrawCustomLicenseSection() {
            CustomLicenseTemplateSettings settings = CustomLicenseTemplateSettings.Instance;
            using SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.Update();

            PackageAuthoringGui.DrawSection("Custom License Template", () => {
                EditorGUILayout.LabelField(CustomLicenseTemplateLabel, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(3f);
                EditorGUILayout.HelpBox(
                    Templates.TemplateTokenResolver.SupportedTokensHelpText,
                    MessageType.None);
                SerializedProperty contentProperty = serializedSettings.FindProperty(ContentField);
                DrawTemplateTextArea(contentProperty, minHeight: 220f);
            });

            if (serializedSettings.ApplyModifiedProperties()) {
                settings.SaveSettings();
            }
        }

        private static void DrawTemplateTextArea(SerializedProperty contentProperty, float minHeight) {
            EditorGUI.BeginChangeCheck();
            string updatedContent = InlineRichTextTextArea.DrawLayout(
                contentProperty.stringValue,
                minHeight);
            if (EditorGUI.EndChangeCheck()) {
                contentProperty.stringValue = updatedContent;
            }
        }
    }
}
