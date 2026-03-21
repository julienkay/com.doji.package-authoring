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
        private static readonly GUIContent DocumentationTemplatesLabel = new(
            "Documentation",
            "Template content written to generated repository documentation scaffold files.");

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
        /// Creates the child settings provider for generated documentation scaffold templates.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateDocumentationTemplateProvider() {
            return new SettingsProvider("Project/Doji/Package Authoring/Templates/Documentation", SettingsScope.Project) {
                guiHandler = _ => DrawDocumentationTemplatesSection()
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

        private static void DrawDocumentationTemplatesSection() {
            EditorGUILayout.LabelField(DocumentationTemplatesLabel, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(3f);
            EditorGUILayout.HelpBox(
                Templates.TemplateTokenResolver.SupportedTokensHelpText,
                MessageType.None);

            DrawTemplateAssetSection("docs/.gitignore", DocsGitIgnoreTemplateSettings.Instance, 120f, () => DocsGitIgnoreTemplateSettings.Instance.SaveSettings());
            DrawTemplateAssetSection("docs/api/.gitignore", DocsApiGitIgnoreTemplateSettings.Instance, 120f, () => DocsApiGitIgnoreTemplateSettings.Instance.SaveSettings());
            DrawTemplateAssetSection("docs/api/index.md", DocsApiIndexTemplateSettings.Instance, 120f, () => DocsApiIndexTemplateSettings.Instance.SaveSettings());
            DrawTemplateAssetSection("docs/docfx.json", DocsDocfxJsonTemplateSettings.Instance, 320f, () => DocsDocfxJsonTemplateSettings.Instance.SaveSettings());
            DrawTemplateAssetSection("docs/docfx-pdf.json", DocsDocfxPdfJsonTemplateSettings.Instance, 360f, () => DocsDocfxPdfJsonTemplateSettings.Instance.SaveSettings());
            DrawTemplateAssetSection("docs/filterConfig.yml", DocsFilterConfigTemplateSettings.Instance, 180f, () => DocsFilterConfigTemplateSettings.Instance.SaveSettings());
            DrawTemplateAssetSection("docs/index.md", DocsIndexTemplateSettings.Instance, 120f, () => DocsIndexTemplateSettings.Instance.SaveSettings());
            DrawTemplateAssetSection("docs/toc.yml", DocsRootTocTemplateSettings.Instance, 120f, () => DocsRootTocTemplateSettings.Instance.SaveSettings());
            DrawTemplateAssetSection("docs/manual/toc.yml", DocsManualTocTemplateSettings.Instance, 120f, () => DocsManualTocTemplateSettings.Instance.SaveSettings());
            DrawTemplateAssetSection("docs/pdf/toc.yml", DocsPdfTocTemplateSettings.Instance, 120f, () => DocsPdfTocTemplateSettings.Instance.SaveSettings());
        }

        private static void DrawTemplateAssetSection(
            string title,
            ProjectTemplateAsset settings,
            float minHeight,
            System.Action saveSettings) {
            using SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.Update();

            PackageAuthoringGui.DrawSection(title, () => {
                SerializedProperty contentProperty = serializedSettings.FindProperty(ContentField);
                DrawTemplateTextArea(contentProperty, minHeight);
            });

            if (serializedSettings.ApplyModifiedProperties()) {
                saveSettings();
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
