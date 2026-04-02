using Doji.PackageAuthoring.Editor.Wizards.Models;
using UnityEditor;
using UnityEngine;
using Doji.PackageAuthoring.Editor.Wizards.UI;

namespace Doji.PackageAuthoring.Editor.Wizards.Drawers {
    /// <summary>
    /// Draws the serialized project-settings block used by the project and package authoring tools.
    /// </summary>
    [CustomPropertyDrawer(typeof(ProjectSettings))]
    internal sealed class ProjectSettingsDrawer : PropertyDrawer {
        private static readonly string CompanyNameField = $"<{nameof(ProjectSettings.CompanyName)}>k__BackingField";
        private static readonly string ProductNameField = $"<{nameof(ProjectSettings.ProductName)}>k__BackingField";
        private static readonly string VersionField = $"<{nameof(ProjectSettings.Version)}>k__BackingField";
        private static readonly string PreferredEditorField = $"<{nameof(ProjectSettings.PreferredEditor)}>k__BackingField";
        private static readonly string GenerateAgentsFileField = $"<{nameof(ProjectSettings.GenerateAgentsFile)}>k__BackingField";

        private static readonly string TargetLocationField =
            $"<{nameof(ProjectSettings.TargetLocation)}>k__BackingField";

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            ProjectSettingsDrawerContext.State state = ProjectSettingsDrawerContext.Current;
            int lineCount = state.IncludeTargetLocation ? 6 : 5;
            return (EditorGUIUtility.singleLineHeight * lineCount) +
                   (EditorGUIUtility.standardVerticalSpacing * (lineCount - 1));
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ProjectSettingsDrawerContext.State state = ProjectSettingsDrawerContext.Current;
            Rect row = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            EditorGUI.BeginProperty(position, label, property);
            DrawField(
                ref row,
                property.FindPropertyRelative(CompanyNameField),
                PackageAuthoringFieldLabels.Project.CompanyName,
                RepositoryLayoutPreviewHoverTargets.ProjectCompanyName);
            DrawField(
                ref row,
                property.FindPropertyRelative(ProductNameField),
                PackageAuthoringFieldLabels.Project.ProductName(state.ProductLabel),
                RepositoryLayoutPreviewHoverTargets.ProductName);
            DrawField(
                ref row,
                property.FindPropertyRelative(VersionField),
                PackageAuthoringFieldLabels.Project.Version,
                RepositoryLayoutPreviewHoverTargets.Version);
            DrawField(
                ref row,
                property.FindPropertyRelative(PreferredEditorField),
                PackageAuthoringFieldLabels.Project.PreferredEditor);
            DrawField(
                ref row,
                property.FindPropertyRelative(GenerateAgentsFileField),
                PackageAuthoringFieldLabels.Project.GenerateAgentsFile);

            if (state.IncludeTargetLocation) {
                DrawField(
                    ref row,
                    property.FindPropertyRelative(TargetLocationField),
                    PackageAuthoringFieldLabels.Project.TargetLocation,
                    RepositoryLayoutPreviewHoverTargets.TargetLocation);
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
    }
}
