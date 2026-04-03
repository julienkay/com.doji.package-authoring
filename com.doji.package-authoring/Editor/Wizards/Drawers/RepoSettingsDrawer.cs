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
            EditorGUI.LabelField(row, PackageAuthoringFieldLabels.Repository.FilesSection, EditorStyles.boldLabel);
            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            DrawField(
                ref row,
                property.FindPropertyRelative(CopyrightHolderField),
                PackageAuthoringFieldLabels.Repository.CopyrightHolder,
                RepositoryLayoutPreviewHoverTargets.RepoCopyrightHolder);
            DrawField(
                ref row,
                property.FindPropertyRelative(IncludeReadmeField),
                PackageAuthoringFieldLabels.Repository.IncludeReadme,
                RepositoryLayoutPreviewHoverTargets.IncludeRepositoryReadme);
            DrawField(
                ref row,
                property.FindPropertyRelative(LicenseTypeField),
                PackageAuthoringFieldLabels.Repository.LicenseType,
                RepositoryLayoutPreviewHoverTargets.RepoLicenseType);
            SerializedProperty initializeGitRepositoryProperty =
                property.FindPropertyRelative(InitializeGitRepositoryField);
            DrawField(
                ref row,
                initializeGitRepositoryProperty,
                PackageAuthoringFieldLabels.Repository.InitializeGitRepository);
            if (initializeGitRepositoryProperty.boolValue) {
                EditorGUI.indentLevel++;
                DrawTokenAwareTextField(
                    ref row,
                    property.FindPropertyRelative(RepositoryUrlField),
                    PackageAuthoringFieldLabels.Repository.RepositoryUrl);
                EditorGUI.indentLevel--;
            }

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
            GUIContent label) {
            property.stringValue = InlineRichTextTextField.Draw(row, label, property.stringValue);
            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
        }
    }
}
