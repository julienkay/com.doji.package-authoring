using UnityEditor;
using UnityEngine;
using Doji.PackageAuthoring.Editor.Wizards.Models;
using Doji.PackageAuthoring.Editor.Wizards.UI;

namespace Doji.PackageAuthoring.Editor.Wizards {
    /// <summary>
    /// Draws the serialized package-settings block and delegates dependency editing to the nested list drawer.
    /// </summary>
    [CustomPropertyDrawer(typeof(PackageSettings))]
    internal sealed class PackageSettingsDrawer : PropertyDrawer {
        private static readonly string PackageNameField = $"<{nameof(PackageSettings.PackageName)}>k__BackingField";
        private static readonly string PackageDisplayNameField =
            $"<{nameof(PackageSettings.PackageDisplayName)}>k__BackingField";
        private static readonly string AssemblyNameField = $"<{nameof(PackageSettings.AssemblyName)}>k__BackingField";
        private static readonly string NamespaceNameField = $"<{nameof(PackageSettings.NamespaceName)}>k__BackingField";
        private static readonly string DescriptionField = $"<{nameof(PackageSettings.Description)}>k__BackingField";
        private static readonly string CompanyNameField = $"<{nameof(PackageSettings.CompanyName)}>k__BackingField";
        private static readonly string IncludeAuthorField = $"<{nameof(PackageSettings.IncludeAuthor)}>k__BackingField";
        private static readonly string AuthorUrlField = $"<{nameof(PackageSettings.AuthorUrl)}>k__BackingField";
        private static readonly string AuthorEmailField = $"<{nameof(PackageSettings.AuthorEmail)}>k__BackingField";
        private static readonly string DocumentationUrlField =
            $"<{nameof(PackageSettings.DocumentationUrl)}>k__BackingField";

        private static readonly string IncludeMinimumUnityVersionField =
            $"<{nameof(PackageSettings.IncludeMinimumUnityVersion)}>k__BackingField";

        private static readonly string MinimumUnityMajorField =
            $"<{nameof(PackageSettings.MinimumUnityMajor)}>k__BackingField";

        private static readonly string MinimumUnityMinorField =
            $"<{nameof(PackageSettings.MinimumUnityMinor)}>k__BackingField";

        private static readonly string MinimumUnityReleaseField =
            $"<{nameof(PackageSettings.MinimumUnityRelease)}>k__BackingField";

        private static readonly string CreateDocsFolderField =
            $"<{nameof(PackageSettings.CreateDocsFolder)}>k__BackingField";
        private static readonly string IncludeReadmeField =
            $"<{nameof(PackageSettings.IncludeReadme)}>k__BackingField";

        private static readonly string CreateSamplesFolderField =
            $"<{nameof(PackageSettings.CreateSamplesFolder)}>k__BackingField";

        private static readonly string CreateEditorFolderField =
            $"<{nameof(PackageSettings.CreateEditorFolder)}>k__BackingField";

        private static readonly string CreateTestsFolderField =
            $"<{nameof(PackageSettings.CreateTestsFolder)}>k__BackingField";

        private static readonly string DependenciesField = $"<{nameof(PackageSettings.Dependencies)}>k__BackingField";

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            int lineCount = 14;
            if (property.FindPropertyRelative(IncludeAuthorField).boolValue) {
                lineCount += 2;
            }

            if (property.FindPropertyRelative(IncludeMinimumUnityVersionField).boolValue) {
                lineCount += 3;
            }

            float lineHeight = (EditorGUIUtility.singleLineHeight * lineCount) +
                               (EditorGUIUtility.standardVerticalSpacing * (lineCount - 1));
            SerializedProperty dependenciesProperty = property.FindPropertyRelative(DependenciesField);
            float dependenciesHeight = EditorGUI.GetPropertyHeight(
                dependenciesProperty,
                GUIContent.none,
                includeChildren: true);
            return lineHeight + 8f + dependenciesHeight;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Rect row = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(position, label, property);
            DrawField(
                ref row,
                property.FindPropertyRelative(PackageNameField),
                new GUIContent("Identifier"),
                RepositoryLayoutPreviewHoverTargets.PackageName);
            DrawField(
                ref row,
                property.FindPropertyRelative(PackageDisplayNameField),
                new GUIContent("Package Name", "User-facing package display name written to package.json."),
                RepositoryLayoutPreviewHoverTargets.PackageDisplayName);
            DrawField(
                ref row,
                property.FindPropertyRelative(AssemblyNameField),
                new GUIContent("Assembly Name"),
                RepositoryLayoutPreviewHoverTargets.AssemblyName);
            DrawField(
                ref row,
                property.FindPropertyRelative(NamespaceNameField),
                new GUIContent("Namespace"),
                RepositoryLayoutPreviewHoverTargets.NamespaceName);
            DrawField(
                ref row,
                property.FindPropertyRelative(DescriptionField),
                new GUIContent("Description"),
                RepositoryLayoutPreviewHoverTargets.Description);
            DrawField(
                ref row,
                property.FindPropertyRelative(CompanyNameField),
                new GUIContent("Company Name"),
                RepositoryLayoutPreviewHoverTargets.PackageCompanyName);

            SerializedProperty includeAuthorProperty = property.FindPropertyRelative(IncludeAuthorField);
            DrawField(
                ref row,
                includeAuthorProperty,
                new GUIContent("Include Author Metadata"),
                RepositoryLayoutPreviewHoverTargets.IncludeAuthor);
            if (includeAuthorProperty.boolValue) {
                EditorGUI.indentLevel++;
                DrawField(
                    ref row,
                    property.FindPropertyRelative(AuthorUrlField),
                    new GUIContent("URL"),
                    RepositoryLayoutPreviewHoverTargets.AuthorUrl);
                DrawField(
                    ref row,
                    property.FindPropertyRelative(AuthorEmailField),
                    new GUIContent("Email"),
                    RepositoryLayoutPreviewHoverTargets.AuthorEmail);
                EditorGUI.indentLevel--;
            }

            SerializedProperty includeUnityVersionProperty =
                property.FindPropertyRelative(IncludeMinimumUnityVersionField);
            DrawField(
                ref row,
                includeUnityVersionProperty,
                new GUIContent("Minimum Unity Version"),
                RepositoryLayoutPreviewHoverTargets.IncludeMinimumUnityVersion);
            if (includeUnityVersionProperty.boolValue) {
                EditorGUI.indentLevel++;
                DrawField(
                    ref row,
                    property.FindPropertyRelative(MinimumUnityMajorField),
                    new GUIContent("Major"),
                    RepositoryLayoutPreviewHoverTargets.MinimumUnityVersion);
                DrawField(
                    ref row,
                    property.FindPropertyRelative(MinimumUnityMinorField),
                    new GUIContent("Minor"),
                    RepositoryLayoutPreviewHoverTargets.MinimumUnityVersion);
                DrawField(
                    ref row,
                    property.FindPropertyRelative(MinimumUnityReleaseField),
                    new GUIContent("Release"),
                    RepositoryLayoutPreviewHoverTargets.MinimumUnityVersion);
                EditorGUI.indentLevel--;
            }

            DrawTokenAwareTextField(
                ref row,
                property.FindPropertyRelative(DocumentationUrlField),
                new GUIContent("Documentation URL"),
                RepositoryLayoutPreviewHoverTargets.DocumentationUrl);

            DrawField(
                ref row,
                property.FindPropertyRelative(IncludeReadmeField),
                new GUIContent("Include Package README"),
                RepositoryLayoutPreviewHoverTargets.IncludePackageReadme);
            DrawField(
                ref row,
                property.FindPropertyRelative(CreateDocsFolderField),
                new GUIContent("Create Documentation Folder"),
                RepositoryLayoutPreviewHoverTargets.CreateDocsFolder);
            DrawField(
                ref row,
                property.FindPropertyRelative(CreateSamplesFolderField),
                new GUIContent("Create Samples Folder"),
                RepositoryLayoutPreviewHoverTargets.CreateSamplesFolder);
            DrawField(
                ref row,
                property.FindPropertyRelative(CreateEditorFolderField),
                new GUIContent("Create Editor Folder"),
                RepositoryLayoutPreviewHoverTargets.CreateEditorFolder);
            DrawField(
                ref row,
                property.FindPropertyRelative(CreateTestsFolderField),
                new GUIContent("Create Tests Folder"),
                RepositoryLayoutPreviewHoverTargets.CreateTestsFolder);

            row.y += 8f - EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty dependenciesProperty = property.FindPropertyRelative(DependenciesField);
            float dependenciesHeight = EditorGUI.GetPropertyHeight(
                dependenciesProperty,
                GUIContent.none,
                includeChildren: true);
            EditorGUI.PropertyField(
                new Rect(row.x, row.y, row.width, dependenciesHeight),
                dependenciesProperty,
                new GUIContent("Dependencies"),
                includeChildren: true);
            RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(
                new Rect(row.x, row.y, row.width, dependenciesHeight),
                RepositoryLayoutPreviewHoverTargets.Dependencies);
            EditorGUI.EndProperty();
        }

        private static void DrawField(
            ref Rect row,
            SerializedProperty property,
            GUIContent label,
            params string[] hoverTargets) {
            EditorGUI.PropertyField(row, property, label);
            RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(row, hoverTargets);
            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private static void DrawTokenAwareTextField(
            ref Rect row,
            SerializedProperty property,
            GUIContent label,
            params string[] hoverTargets) {
            property.stringValue = InlineRichTextTextField.Draw(row, label, property.stringValue);
            RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(row, hoverTargets);
            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
