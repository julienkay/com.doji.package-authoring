using Doji.PackageAuthoring.Editor.Wizards.Models;
using UnityEditor;
using UnityEngine;
using Doji.PackageAuthoring.Editor.Wizards.UI;

namespace Doji.PackageAuthoring.Editor.Wizards.Drawers {
    /// <summary>
    /// Draws the serialized repository-settings block shared by package authoring surfaces.
    /// </summary>
    [CustomPropertyDrawer(typeof(RepoSettings))]
    internal sealed class RepoSettingsDrawer : PropertyDrawer {
        private static readonly string CopyrightHolderField =
            $"<{nameof(RepoSettings.CopyrightHolder)}>k__BackingField";

        private static readonly string InitializeGitRepositoryField =
            $"<{nameof(RepoSettings.InitializeGitRepository)}>k__BackingField";

        private static readonly string IncludeReadmeField = $"<{nameof(RepoSettings.IncludeReadme)}>k__BackingField";
        private static readonly string LicenseTypeField = $"<{nameof(RepoSettings.LicenseType)}>k__BackingField";
        private static readonly string RepositoryUrlField = $"<{nameof(RepoSettings.RepositoryUrl)}>k__BackingField";
        private static readonly GUIContent RepositoryFilesSectionLabel = EditorGUIUtility.TrTextContent("Repository Files");

        private static readonly GUIContent LicenseTypeLabel = EditorGUIUtility.TrTextContent(
            "Open Source License",
            "Controls the generated license template.");

        private static readonly GUIContent RepositoryUrlLabel = EditorGUIUtility.TrTextContent(
            "Remote URL",
            "Optional URL assigned to the repository's origin remote after git initialization.");

        private static readonly GUIContent InitializeGitRepositoryLabel = EditorGUIUtility.TrTextContent(
            "Initialize Git Repository",
            "Controls whether the generated repository runs git init and creates an initial commit.");

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            SerializedProperty initializeGitRepositoryProperty =
                property.FindPropertyRelative(InitializeGitRepositoryField);
            bool showRepositoryUrl = initializeGitRepositoryProperty?.boolValue ?? false;
            float lineCount = showRepositoryUrl ? 6f : 5f;
            float spacingCount = showRepositoryUrl ? 5f : 4f;
            return (EditorGUIUtility.singleLineHeight * lineCount) + (EditorGUIUtility.standardVerticalSpacing * spacingCount);
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            Rect row = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(position, label, property);
            EditorGUI.LabelField(row, RepositoryFilesSectionLabel, EditorStyles.boldLabel);
            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            int previousIndent = EditorGUI.indentLevel;
            EditorGUI.indentLevel++;
            EditorGUI.PropertyField(
                row,
                property.FindPropertyRelative(CopyrightHolderField),
                new GUIContent("Copyright Holder"));
            RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(
                row,
                RepositoryLayoutPreviewHoverTargets.RepoCopyrightHolder);
            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(
                row,
                property.FindPropertyRelative(IncludeReadmeField),
                new GUIContent("Include README"));
            RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(
                row,
                RepositoryLayoutPreviewHoverTargets.IncludeRepositoryReadme);
            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            EditorGUI.PropertyField(
                row,
                property.FindPropertyRelative(LicenseTypeField),
                LicenseTypeLabel);
            RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(
                row,
                RepositoryLayoutPreviewHoverTargets.RepoLicenseType);
            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            SerializedProperty initializeGitRepositoryProperty =
                property.FindPropertyRelative(InitializeGitRepositoryField);
            EditorGUI.PropertyField(
                row,
                initializeGitRepositoryProperty,
                InitializeGitRepositoryLabel);
            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (initializeGitRepositoryProperty.boolValue) {
                int repositoryUrlIndent = EditorGUI.indentLevel;
                EditorGUI.indentLevel++;
                EditorGUI.PropertyField(row, property.FindPropertyRelative(RepositoryUrlField), RepositoryUrlLabel);
                EditorGUI.indentLevel = repositoryUrlIndent;
            }

            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }
    }
}
