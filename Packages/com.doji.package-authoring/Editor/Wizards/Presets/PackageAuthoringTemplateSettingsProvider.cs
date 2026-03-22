using Doji.PackageAuthoring.Editor.Wizards.UI;
using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.Presets {
    /// <summary>
    /// Registers and renders the Project Settings page for editable generation templates.
    /// </summary>
    internal static class PackageAuthoringTemplateSettingsProvider {
        private static bool _hasInitializedDocumentationTemplateSettings;
        private static bool _pendingUndoRedoSave;
        private static readonly string ContentField = $"<{nameof(GitIgnoreTemplateSettings.Content)}>k__BackingField";
        private static readonly string FaviconTextureField = $"<{nameof(DocsBrandingImageSettings.FaviconTexture)}>k__BackingField";
        private static readonly string LogoTextureField = $"<{nameof(DocsBrandingImageSettings.LogoTexture)}>k__BackingField";
        private static readonly GUIContent GitIgnoreTemplateLabel = new(
            ".gitignore",
            "Template content written to generated project .gitignore files.");
        private static readonly GUIContent RepositoryReadmeTemplateLabel = new(
            "Repository README",
            "Template content written to generated repository README.md files.");
        private static readonly GUIContent CustomLicenseTemplateLabel = new(
            "Custom License",
            "Template content written to generated repository LICENSE files when Open Source License is set to Custom.");
        private static readonly GUIContent DocumentationTemplatesLabel = new(
            "Documentation",
            "Template content written to generated repository documentation scaffold files.");
        private static readonly GUIContent DocumentationFaviconTextureLabel = new(
            "Favicon Source",
            "Optional texture asset used to generate docs/images/favicon.ico. Expected resolution: 128x128 or higher.");
        private static readonly GUIContent DocumentationLogoTextureLabel = new(
            "Logo Source",
            "Optional texture asset used to generate docs/images/logo.png. Expected resolution: 50x50.");

        static PackageAuthoringTemplateSettingsProvider() {
            Undo.undoRedoPerformed += HandleUndoRedoPerformed;
        }

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
        /// Creates the Package templates category shown under <c>Project/Doji/Package Authoring/Templates/Package</c>.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreatePackageTemplatesProvider() {
            return new SettingsProvider("Project/Doji/Package Authoring/Templates/Package", SettingsScope.Project) {
                guiHandler = _ => DrawTemplateCategoryLandingPage("Package")
            };
        }

        /// <summary>
        /// Creates the Repository templates category shown under <c>Project/Doji/Package Authoring/Templates/Repository</c>.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateRepositoryTemplatesProvider() {
            return new SettingsProvider("Project/Doji/Package Authoring/Templates/Repository", SettingsScope.Project) {
                guiHandler = _ => DrawTemplateCategoryLandingPage("Repository")
            };
        }

        /// <summary>
        /// Creates the child settings provider for the generated project <c>.gitignore</c> template.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateGitIgnoreTemplateProvider() {
            return new SettingsProvider("Project/Doji/Package Authoring/Templates/Package/.gitignore", SettingsScope.Project) {
                guiHandler = _ => DrawGitIgnoreSection(),
                deactivateHandler = SaveGitIgnoreSettingsOnDeactivate
            };
        }

        /// <summary>
        /// Creates the child settings provider for the generated custom repository license template.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateCustomLicenseTemplateProvider() {
            return new SettingsProvider("Project/Doji/Package Authoring/Templates/Repository/Custom License", SettingsScope.Project) {
                guiHandler = _ => DrawCustomLicenseSection(),
                deactivateHandler = SaveCustomLicenseSettingsOnDeactivate,
            };
        }

        /// <summary>
        /// Creates the child settings provider for the generated repository README template.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateRepositoryReadmeTemplateProvider() {
            return new SettingsProvider("Project/Doji/Package Authoring/Templates/Repository/README", SettingsScope.Project) {
                guiHandler = _ => DrawRepositoryReadmeSection(),
                deactivateHandler = SaveRepositoryReadmeSettingsOnDeactivate,
            };
        }

        /// <summary>
        /// Creates the child settings provider for generated documentation scaffold templates.
        /// </summary>
        [SettingsProvider]
        public static SettingsProvider CreateDocumentationTemplateProvider() {
            return new SettingsProvider("Project/Doji/Package Authoring/Templates/Documentation (DocFX)", SettingsScope.Project) {
                guiHandler = _ => DrawDocumentationTemplatesSection(),
                deactivateHandler = SaveDocumentationTemplateSettingsOnDeactivate
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

        /// <summary>
        /// Draws a landing page for one template category under the main Templates section.
        /// </summary>
        private static void DrawTemplateCategoryLandingPage(string categoryName) {
            EditorGUILayout.Space(8f);
            EditorGUILayout.HelpBox(
                $"Select a {categoryName.ToLowerInvariant()} template subsection to edit the generated file content.",
                MessageType.Info);
        }

        /// <summary>
        /// Draws the editable project-level <c>.gitignore</c> template section and persists changes to <c>ProjectSettings</c>.
        /// </summary>
        private static void DrawGitIgnoreSection() {
            GitIgnoreTemplateSettings settings = GitIgnoreTemplateSettings.Instance;
            using SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.Update();

            PackageAuthoringGui.DrawSection(".gitignore Template", () => {
                SerializedProperty contentProperty = serializedSettings.FindProperty(ContentField);
                DrawTemplateTextArea(contentProperty, minHeight: 420f);
            });

            ApplyAndSaveOnChangeOrUndo(serializedSettings, settings.SaveSettings);
            _pendingUndoRedoSave = false;
        }

        /// <summary>
        /// Draws the editable repository README template with token guidance and explicit project-settings persistence.
        /// </summary>
        private static void DrawRepositoryReadmeSection() {
            RepositoryReadmeTemplateSettings settings = RepositoryReadmeTemplateSettings.Instance;
            using SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.Update();

            PackageAuthoringGui.DrawSection("Repository README Template", () => {
                EditorGUILayout.LabelField(RepositoryReadmeTemplateLabel, EditorStyles.wordWrappedMiniLabel);
                EditorGUILayout.Space(3f);
                EditorGUILayout.HelpBox(
                    Templates.TemplateTokenResolver.SupportedTokensHelpText,
                    MessageType.None);
                SerializedProperty contentProperty = serializedSettings.FindProperty(ContentField);
                DrawTemplateTextArea(contentProperty, minHeight: 260f);
            });

            ApplyAndSaveOnChangeOrUndo(serializedSettings, settings.SaveSettings);
            _pendingUndoRedoSave = false;
        }

        /// <summary>
        /// Draws the editable custom license template with token guidance and explicit project-settings persistence.
        /// </summary>
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

            ApplyAndSaveOnChangeOrUndo(serializedSettings, settings.SaveSettings);
            _pendingUndoRedoSave = false;
        }

        /// <summary>
        /// Draws the documentation template settings page, mixing editable project overrides with locked built-in templates.
        /// </summary>
        private static void DrawDocumentationTemplatesSection() {
            EnsureDocumentationTemplateSettingsInitialized();

            EditorGUILayout.LabelField(DocumentationTemplatesLabel, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(3f);

            DrawDocumentationBrandingSection();
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

            _pendingUndoRedoSave = false;
        }

        /// <summary>
        /// Persists all documentation template settings once before the documentation page first renders.
        /// </summary>
        /// <remarks>
        /// Documentation template settings are loaded lazily and can exist only as transient in-memory defaults until a
        /// save path is exercised. Initializing them here ensures a fresh project gets concrete <c>ProjectSettings</c>
        /// assets for both editable and locked documentation templates the first time this page is opened.
        /// </remarks>
        private static void EnsureDocumentationTemplateSettingsInitialized() {
            if (_hasInitializedDocumentationTemplateSettings) {
                return;
            }

            PackageAuthoringProjectSettingsApi.SaveAllProjectSettings();
            _hasInitializedDocumentationTemplateSettings = true;
        }

        /// <summary>
        /// Draws one editable template asset section backed by a serialized <see cref="ProjectTemplateAsset"/>.
        /// </summary>
        /// <param name="title">Visible section title that also reflects the generated output path.</param>
        /// <param name="settings">Project-settings asset containing the editable template content.</param>
        /// <param name="minHeight">Minimum text-area height used before content-driven expansion.</param>
        /// <param name="saveSettings">Persistence callback required because these assets are stored under <c>ProjectSettings</c>.</param>
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

            ApplyAndSaveOnChangeOrUndo(serializedSettings, saveSettings);
        }

        /// <summary>
        /// Draws one built-in template section as locked content while preserving selection and copy behavior.
        /// </summary>
        /// <param name="title">Visible section title that also reflects the generated output path.</param>
        /// <param name="settings">Template asset supplying the built-in content.</param>
        /// <param name="minHeight">Minimum text-area height used before content-driven expansion.</param>
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

        /// <summary>
        /// Draws the highlighted editable template text area and writes the updated value back to the serialized backing field.
        /// </summary>
        /// <remarks>
        /// The actual text rendering lives in <see cref="InlineRichTextTextArea"/> so the settings provider only manages
        /// serialized-property synchronization and change detection here.
        /// </remarks>
        private static void DrawTemplateTextArea(SerializedProperty contentProperty, float minHeight) {
            string currentContent = contentProperty.stringValue ?? string.Empty;
            string updatedContent = InlineRichTextTextArea.DrawLayout(currentContent, minHeight);
            if (!string.Equals(updatedContent, currentContent, System.StringComparison.Ordinal)) {
                contentProperty.stringValue = updatedContent;
            }
        }

        /// <summary>
        /// Draws a locked template text area with token highlighting, muted base text, and selectable content.
        /// </summary>
        /// <remarks>
        /// Built-in templates must look non-editable without using a real disabled text area because disabled IMGUI controls
        /// stop supporting text selection and copy.
        /// </remarks>
        private static void DrawReadOnlyTemplateTextArea(string content, float minHeight) {
            content ??= string.Empty;
            Color baseTextColor = EditorStyles.label.normal.textColor;
            InlineRichTextTextArea.DrawReadOnlyLayout(
                content,
                minHeight,
                new Color(baseTextColor.r, baseTextColor.g, baseTextColor.b, 0.6f));
        }

        /// <summary>
        /// Draws the documentation image asset settings used to generate branding files inside <c>docs/images</c>.
        /// </summary>
        private static void DrawDocumentationBrandingSection() {
            DocsBrandingImageSettings settings = DocsBrandingImageSettings.Instance;
            using SerializedObject serializedSettings = new SerializedObject(settings);
            serializedSettings.Update();

            PackageAuthoringGui.DrawSection("docs/images", () => {
                DrawTextureObjectField(serializedSettings, FaviconTextureField, DocumentationFaviconTextureLabel);
                EditorGUILayout.Space(6f);
                DrawTextureObjectField(serializedSettings, LogoTextureField, DocumentationLogoTextureLabel);
            });

            if (serializedSettings.ApplyModifiedProperties()) {
                settings.SaveSettings();
            }
        }

        /// <summary>
        /// Draws one texture object field for a serialized branding image reference and records the assignment when changed.
        /// </summary>
        /// <param name="serializedSettings">Serialized wrapper around the branding settings asset.</param>
        /// <param name="propertyName">Backing-field property name for the texture reference.</param>
        /// <param name="label">Descriptive label shown above the object picker.</param>
        private static void DrawTextureObjectField(
            SerializedObject serializedSettings,
            string propertyName,
            GUIContent label) {
            EditorGUILayout.LabelField(label, EditorStyles.wordWrappedMiniLabel);
            EditorGUILayout.Space(3f);
            SerializedProperty textureProperty = serializedSettings.FindProperty(propertyName);
            EditorGUI.BeginChangeCheck();
            Texture2D updatedTexture = (Texture2D)EditorGUILayout.ObjectField(
                GUIContent.none,
                textureProperty.objectReferenceValue,
                typeof(Texture2D),
                allowSceneObjects: false);
            if (EditorGUI.EndChangeCheck()) {
                textureProperty.objectReferenceValue = updatedTexture;
            }
        }

        /// <summary>
        /// Marks the next GUI pass as requiring an explicit save after Unity replays serialized changes from undo or redo.
        /// </summary>
        private static void HandleUndoRedoPerformed() {
            _pendingUndoRedoSave = true;
        }

        /// <summary>
        /// Persists the project-level <c>.gitignore</c> template when the settings page is left.
        /// </summary>
        private static void SaveGitIgnoreSettingsOnDeactivate() {
            GitIgnoreTemplateSettings.Instance.SaveSettings();
        }

        /// <summary>
        /// Persists the repository README template when the settings page is left.
        /// </summary>
        private static void SaveRepositoryReadmeSettingsOnDeactivate() {
            RepositoryReadmeTemplateSettings.Instance.SaveSettings();
        }

        /// <summary>
        /// Persists the custom license template when the settings page is left.
        /// </summary>
        private static void SaveCustomLicenseSettingsOnDeactivate() {
            CustomLicenseTemplateSettings.Instance.SaveSettings();
        }

        /// <summary>
        /// Persists all editable documentation template assets when the documentation settings page is left.
        /// </summary>
        private static void SaveDocumentationTemplateSettingsOnDeactivate() {
            PackageAuthoringProjectSettingsApi.SaveAllProjectSettings();
        }

        /// <summary>
        /// Applies serialized changes and persists project-settings assets even when the current GUI update is an undo/redo pass.
        /// </summary>
        /// <remarks>
        /// Project-settings objects saved through explicit <c>SaveSettings()</c> calls can fall out of sync with disk when a custom
        /// text control redraws correctly after undo/redo but no normal property-apply path reports a fresh modification. Undo can
        /// also restore the in-memory value before the affected section is drawn, so by the time the field code runs there may no
        /// longer be a detectable current-vs-updated string difference. Use this helper for editable settings UIs that write to
        /// <c>ProjectSettings</c>, especially around custom text areas or overlay-based IMGUI controls where the visual edit flow is
        /// decoupled from Unity's default inspector field pipeline. Editable template providers also add a narrow
        /// <see cref="SettingsProvider.deactivateHandler"/> fallback so navigating away from the page persists the
        /// current in-memory state even if a custom text control sidesteps the usual change notifications.
        /// </remarks>
        private static void ApplyAndSaveOnChangeOrUndo(SerializedObject serializedSettings, System.Action saveSettings) {
            // Undo/redo updates the in-memory settings object, but these ProjectSettings-backed assets still need an explicit save.
            if (serializedSettings.ApplyModifiedProperties() || _pendingUndoRedoSave) {
                saveSettings();
            }
        }

        /// <summary>
        /// Creates a muted section-header style used for built-in templates that are intentionally locked.
        /// </summary>
        private static GUIStyle CreateReadOnlyHeaderStyle() {
            GUIStyle readOnlyHeaderStyle = new(EditorStyles.boldLabel);
            Color baseColor = readOnlyHeaderStyle.normal.textColor;
            readOnlyHeaderStyle.normal.textColor = new Color(baseColor.r, baseColor.g, baseColor.b, 0.6f);
            readOnlyHeaderStyle.focused.textColor = readOnlyHeaderStyle.normal.textColor;
            readOnlyHeaderStyle.hover.textColor = readOnlyHeaderStyle.normal.textColor;
            readOnlyHeaderStyle.active.textColor = readOnlyHeaderStyle.normal.textColor;
            return readOnlyHeaderStyle;
        }

        /// <summary>
        /// Draws the lock icon shown in read-only template section headers to distinguish built-in content from editable overrides.
        /// </summary>
        private static void DrawReadOnlyHeaderLock() {
            GUIContent lockIcon = EditorGUIUtility.IconContent("LockIcon-On", "Built-in template content");
            Rect lockRect = GUILayoutUtility.GetRect(lockIcon, GUIStyle.none, GUILayout.Width(18f), GUILayout.Height(18f));
            GUI.Label(lockRect, lockIcon);
        }
    }
}
