using Doji.PackageAuthoring.Editor.Wizards.Models;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Doji.PackageAuthoring.Editor.Wizards.UI;
using Doji.PackageAuthoring.Editor.Wizards.PackageSearch;

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
        private static readonly string IncludedPackagesField =
            $"<{nameof(ProjectSettings.IncludedPackages)}>k__BackingField";
        private static readonly Dictionary<string, bool> IncludedPackagesFoldoutStates = new();
        private static readonly Dictionary<string, bool> AdvancedFoldoutStates = new();

        private static readonly string GenerateAgentsFileField = $"<{nameof(ProjectSettings.GenerateAgentsFile)}>k__BackingField";

        private static readonly string TargetLocationField =
            $"<{nameof(ProjectSettings.TargetLocation)}>k__BackingField";

        /// <inheritdoc />
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {
            ProjectSettingsDrawerContext.State state = ProjectSettingsDrawerContext.Current;
            SerializedProperty includedPackagesProperty = property.FindPropertyRelative(IncludedPackagesField);
            float height = 0f;
            height += GetFieldHeight();
            height += GetFieldHeight();
            height += GetFieldHeight();

            if (state.IncludeTargetLocation) {
                height += GetFieldHeight();
            }

            height += GetIncludedPackagesFieldHeight(property, includedPackagesProperty);
            height += GetAdvancedFieldHeight(property, property.FindPropertyRelative(GenerateAgentsFileField));

            return height - EditorGUIUtility.standardVerticalSpacing;
        }

        /// <inheritdoc />
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
            ProjectSettingsDrawerContext.State state = ProjectSettingsDrawerContext.Current;
            Rect row = new(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            SerializedProperty includedPackagesProperty =
                property.FindPropertyRelative(IncludedPackagesField);

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

            if (state.IncludeTargetLocation) {
                DrawField(
                    ref row,
                    property.FindPropertyRelative(TargetLocationField),
                    PackageAuthoringFieldLabels.Project.TargetLocation,
                    RepositoryLayoutPreviewHoverTargets.TargetLocation);
            }

            DrawIncludedPackagesField(ref row, property, includedPackagesProperty);
            DrawAdvancedField(ref row, property, property.FindPropertyRelative(GenerateAgentsFileField));

            EditorGUI.EndProperty();
        }

        private static void DrawField(
            ref Rect row,
            SerializedProperty property,
            GUIContent label,
            params string[] hoverTargets) {
            float propertyHeight = EditorGUI.GetPropertyHeight(property, label, includeChildren: true);
            Rect propertyRect = new(row.x, row.y, row.width, propertyHeight);
            EditorGUI.PropertyField(propertyRect, property, label, includeChildren: true);
            RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(propertyRect, hoverTargets);
            row.y += propertyHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private static float GetFieldHeight(SerializedProperty property = null) {
            float propertyHeight = property == null
                ? EditorGUIUtility.singleLineHeight
                : EditorGUI.GetPropertyHeight(property, includeChildren: true);
            return propertyHeight + EditorGUIUtility.standardVerticalSpacing;
        }

        private static void DrawIncludedPackagesField(
            ref Rect row,
            SerializedProperty parentProperty,
            SerializedProperty includedPackagesProperty) {
            string foldoutKey = GetIncludedPackagesFoldoutKey(parentProperty);
            bool isExpanded = GetFoldoutState(IncludedPackagesFoldoutStates, foldoutKey);
            SerializedProperty preferredEditorProperty = parentProperty.FindPropertyRelative(PreferredEditorField);

            Rect foldoutRect = new(row.x, row.y, row.width, EditorGUIUtility.singleLineHeight);
            isExpanded = EditorGUI.Foldout(
                foldoutRect,
                isExpanded,
                PackageAuthoringFieldLabels.Project.IncludedPackages,
                true);
            RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(
                foldoutRect,
                RepositoryLayoutPreviewHoverTargets.ProjectManifest);
            IncludedPackagesFoldoutStates[foldoutKey] = isExpanded;
            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (!isExpanded) {
                return;
            }

            DrawField(
                ref row,
                preferredEditorProperty,
                PackageAuthoringFieldLabels.Project.PreferredEditor,
                RepositoryLayoutPreviewHoverTargets.ProjectManifest);

            using (PackageSettingsDrawerContext.Push(UnityRegistryPackageAutocompleteField.SuggestionOverflowMode.Scroll)) {
                float listHeight = EditorGUI.GetPropertyHeight(includedPackagesProperty, GUIContent.none, includeChildren: true);
                Rect listRect = new(row.x, row.y, row.width, listHeight);
                EditorGUI.PropertyField(listRect, includedPackagesProperty, GUIContent.none, includeChildren: true);
                RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(
                    listRect,
                    RepositoryLayoutPreviewHoverTargets.ProjectManifest);
                row.y += listHeight + EditorGUIUtility.standardVerticalSpacing;
            }

            float helpBoxHeight = EditorStyles.helpBox.CalcHeight(
                PackageAuthoringFieldLabels.Project.IncludedPackagesInfo,
                row.width);
            Rect helpBoxRect = new(row.x, row.y, row.width, helpBoxHeight);
            EditorGUI.HelpBox(helpBoxRect, PackageAuthoringFieldLabels.Project.IncludedPackagesInfo.text, MessageType.None);
            RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(
                helpBoxRect,
                RepositoryLayoutPreviewHoverTargets.ProjectManifest);
            row.y += helpBoxHeight + EditorGUIUtility.standardVerticalSpacing + 4f;
        }

        private static void DrawAdvancedField(
            ref Rect row,
            SerializedProperty parentProperty,
            SerializedProperty generateAgentsFileProperty) {
            string foldoutKey = GetAdvancedFoldoutKey(parentProperty);
            bool isExpanded = GetFoldoutState(AdvancedFoldoutStates, foldoutKey);

            Rect foldoutRect = new(row.x, row.y, row.width, EditorGUIUtility.singleLineHeight);
            isExpanded = EditorGUI.Foldout(
                foldoutRect,
                isExpanded,
                PackageAuthoringFieldLabels.Project.Advanced,
                true);
            AdvancedFoldoutStates[foldoutKey] = isExpanded;
            row.y += EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;

            if (!isExpanded) {
                return;
            }

            DrawField(
                ref row,
                generateAgentsFileProperty,
                PackageAuthoringFieldLabels.Project.GenerateAgentsFile,
                RepositoryLayoutPreviewHoverTargets.GenerateAgentsFile);
        }

        private static float GetIncludedPackagesFieldHeight(
            SerializedProperty parentProperty,
            SerializedProperty includedPackagesProperty) {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (!GetFoldoutState(IncludedPackagesFoldoutStates, GetIncludedPackagesFoldoutKey(parentProperty))) {
                return height;
            }

            height += GetFieldHeight(parentProperty.FindPropertyRelative(PreferredEditorField));

            using (PackageSettingsDrawerContext.Push(UnityRegistryPackageAutocompleteField.SuggestionOverflowMode.Scroll)) {
                height += EditorGUI.GetPropertyHeight(includedPackagesProperty, GUIContent.none, includeChildren: true) +
                          EditorGUIUtility.standardVerticalSpacing;
            }

            height += EditorStyles.helpBox.CalcHeight(
                PackageAuthoringFieldLabels.Project.IncludedPackagesInfo,
                EditorGUIUtility.currentViewWidth) + EditorGUIUtility.standardVerticalSpacing + 4f;
            return height;
        }

        private static float GetAdvancedFieldHeight(
            SerializedProperty parentProperty,
            SerializedProperty generateAgentsFileProperty) {
            float height = EditorGUIUtility.singleLineHeight + EditorGUIUtility.standardVerticalSpacing;
            if (!GetFoldoutState(AdvancedFoldoutStates, GetAdvancedFoldoutKey(parentProperty))) {
                return height;
            }

            return height + GetFieldHeight(generateAgentsFileProperty);
        }

        private static string GetIncludedPackagesFoldoutKey(SerializedProperty parentProperty) {
            return $"{parentProperty.serializedObject.targetObject.GetInstanceID()}::{parentProperty.propertyPath}.included-packages";
        }

        private static string GetAdvancedFoldoutKey(SerializedProperty parentProperty) {
            return $"{parentProperty.serializedObject.targetObject.GetInstanceID()}::{parentProperty.propertyPath}.advanced";
        }

        private static bool GetFoldoutState(Dictionary<string, bool> states, string key) {
            if (!states.TryGetValue(key, out bool isExpanded)) {
                isExpanded = false;
                states[key] = isExpanded;
            }

            return isExpanded;
        }
    }
}
