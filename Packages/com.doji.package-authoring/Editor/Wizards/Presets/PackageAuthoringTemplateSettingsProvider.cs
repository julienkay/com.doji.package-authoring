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
            return new SettingsProvider("Project/Doji/Package Authoring/Templates/Documentation (DocFX)", SettingsScope.Project) {
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

            DrawReadOnlyTemplateAssetSection("docs/.gitignore", DocsGitIgnoreTemplateSettings.Instance, 120f);
            DrawEditableTemplateAssetSection("docs/docfx.json", DocsDocfxJsonTemplateSettings.Instance, 320f, () => DocsDocfxJsonTemplateSettings.Instance.SaveSettings());
            DrawEditableTemplateAssetSection("docs/docfx-pdf.json", DocsDocfxPdfJsonTemplateSettings.Instance, 360f, () => DocsDocfxPdfJsonTemplateSettings.Instance.SaveSettings());
            DrawEditableTemplateAssetSection("docs/filterConfig.yml", DocsFilterConfigTemplateSettings.Instance, 180f, () => DocsFilterConfigTemplateSettings.Instance.SaveSettings());
            DrawEditableTemplateAssetSection("docs/index.md", DocsIndexTemplateSettings.Instance, 120f, () => DocsIndexTemplateSettings.Instance.SaveSettings());
            DrawReadOnlyTemplateAssetSection("docs/api/.gitignore", DocsApiGitIgnoreTemplateSettings.Instance, 120f);
            DrawEditableTemplateAssetSection("docs/api/index.md", DocsApiIndexTemplateSettings.Instance, 120f, () => DocsApiIndexTemplateSettings.Instance.SaveSettings());
            DrawReadOnlyTemplateAssetSection("docs/toc.yml", DocsRootTocTemplateSettings.Instance, 120f);
            DrawReadOnlyTemplateAssetSection("docs/manual/toc.yml", DocsManualTocTemplateSettings.Instance, 120f);
            DrawReadOnlyTemplateAssetSection("docs/pdf/toc.yml", DocsPdfTocTemplateSettings.Instance, 120f);
        }

        private static void DrawEditableTemplateAssetSection(
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

        private static void DrawReadOnlyTemplateAssetSection(
            string title,
            ProjectTemplateAsset settings,
            float minHeight) {
            using SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.Update();

            GUIStyle readOnlyHeaderStyle = CreateReadOnlyHeaderStyle();
            PackageAuthoringGui.DrawSection(
                title,
                () => {
                    SerializedProperty contentProperty = serializedSettings.FindProperty(ContentField);
                    DrawReadOnlyTemplateTextArea(contentProperty?.stringValue, minHeight);
                },
                drawHeaderAction: DrawReadOnlyHeaderLock,
                titleStyle: readOnlyHeaderStyle);
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

        private static void DrawReadOnlyTemplateTextArea(string content, float minHeight) {
            content ??= string.Empty;
            // Settings providers can be instantiated before EditorStyles is ready, so create GUI styles lazily during OnGUI.
            GUIStyle readOnlyTemplateStyle = new(EditorStyles.textArea);

            float width = Mathf.Max(EditorGUIUtility.currentViewWidth - 48f, 120f);
            float calculatedHeight = Mathf.Max(minHeight, readOnlyTemplateStyle.CalcHeight(new GUIContent(content), width));
            Color originalContentColor = GUI.contentColor;
            GUI.contentColor = new Color(originalContentColor.r, originalContentColor.g, originalContentColor.b, 0.75f);
            EditorGUILayout.SelectableLabel(
                content,
                readOnlyTemplateStyle,
                GUILayout.MinHeight(calculatedHeight),
                GUILayout.ExpandWidth(true));
            GUI.contentColor = originalContentColor;
        }

        private static GUIStyle CreateReadOnlyHeaderStyle() {
            GUIStyle readOnlyHeaderStyle = new(EditorStyles.boldLabel);
            Color baseColor = readOnlyHeaderStyle.normal.textColor;
            readOnlyHeaderStyle.normal.textColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.6f);
            readOnlyHeaderStyle.focused.textColor = readOnlyHeaderStyle.normal.textColor;
            readOnlyHeaderStyle.hover.textColor = readOnlyHeaderStyle.normal.textColor;
            readOnlyHeaderStyle.active.textColor = readOnlyHeaderStyle.normal.textColor;
            return readOnlyHeaderStyle;
        }

        private static void DrawReadOnlyHeaderLock() {
            GUIContent lockIcon = EditorGUIUtility.IconContent("LockIcon-On", "Built-in template content");
            Rect lockRect = GUILayoutUtility.GetRect(lockIcon, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f));
            GUI.Label(lockRect, lockIcon);
        }
    }
}
