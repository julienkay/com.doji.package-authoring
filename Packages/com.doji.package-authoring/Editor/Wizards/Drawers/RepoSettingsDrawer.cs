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

        private static readonly string IncludeReadmeField = $"<{nameof(RepoSettings.IncludeReadme)}>k__BackingField";
        private static readonly string LicenseTypeField = $"<{nameof(RepoSettings.LicenseType)}>k__BackingField";
        private static readonly GUIContent RepositoryFilesSectionLabel = EditorGUIUtility.TrTextContent("Repository Files");

        private static readonly GUIContent LicenseTypeLabel = EditorGUIUtility.TrTextContent(
            "Open Source License",
            "Controls the generated license template.");

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            return (EditorGUIUtility.singleLineHeight * 4f) + (EditorGUIUtility.standardVerticalSpacing * 3f);
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
            EditorGUI.indentLevel = previousIndent;
            EditorGUI.EndProperty();
        }
    }
}
