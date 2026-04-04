using System.IO;
using UnityEditor;
using UnityEngine;
using Doji.PackageAuthoring;
using Doji.PackageAuthoring.Models;
using Doji.PackageAuthoring.Wizards.Presets;
using Doji.PackageAuthoring.Wizards.UI;

namespace Doji.PackageAuthoring.Wizards {
    /// <summary>
    /// Editor window that scaffolds a standalone Unity project from the shared project defaults or a preset.
    /// </summary>
    internal class ProjectCreationWizard : EditorWindow {
        private const string SessionStateKey = "Doji.PackageAuthoring.ProjectCreationWizard.SessionState";
        private const string ProjectSectionPresetTooltip = "Apply project defaults or a preset asset.";
        private const string ProjectSectionTooltip =
            "The generated project starts from this template project's baseline. The generated project includes the project container, a generated Assets folder, copied Packages and ProjectSettings, and a generated .gitignore. These values customize the generated project where product metadata is written.";

        private static readonly string ProductNameField =
            $"<{nameof(Doji.PackageAuthoring.Models.ProjectSettings.ProductName)}>k__BackingField";

        private static readonly string TargetLocationField =
            $"<{nameof(Doji.PackageAuthoring.Models.ProjectSettings.TargetLocation)}>k__BackingField";

        [SerializeField] private bool _autoOpenAfterCreation = true;

        private PackageAuthoringProfile _defaults;
        private SerializedObject _defaultsSerializedObject;
        private SerializedObject _windowSerializedObject;
        private SerializedProperty _autoOpenAfterCreationProperty;

        private PackageAuthoringProfile Defaults => _defaults ??= CreateTemporaryProfile();
        private string ScopedSessionStateKey => WizardSessionStateUtility.GetProjectScopedKey(SessionStateKey);
        private ProjectSettings ProjectSettings => Defaults.ProjectDefaults;
        [MenuItem("Tools/Project Creation Wizard")]
        public static void ShowWindow() {
            GetWindow<ProjectCreationWizard>().titleContent = new GUIContent("Project Creation");
        }

        /// <summary>
        /// Initializes the window title and seeds the initial ad hoc state from project defaults.
        /// </summary>
        private void OnEnable() {
            WizardStateUtility.InitializeWindow(
                this,
                "Project Creation",
                RestoreSessionState,
                ApplyProjectDefaults,
                InitializeSerializedState);
        }

        private void OnDisable() {
            WizardStateUtility.DisposeWindow(
                SaveSessionState,
                ref _defaults,
                ref _defaultsSerializedObject,
                ref _windowSerializedObject,
                ref _autoOpenAfterCreationProperty);
        }

        /// <summary>
        /// Draws the standalone project wizard UI.
        /// </summary>
        private void OnGUI() {
            if (_defaultsSerializedObject == null || _windowSerializedObject == null) {
                InitializeSerializedState();
            }

            _defaultsSerializedObject.Update();
            _windowSerializedObject.Update();
            GUILayout.Space(10);
            DrawProjectSettingsSection();

            GUILayout.Space(8f);
            PackageAuthoringGui.DrawSection("Output", DrawOutputSection);

            _defaultsSerializedObject.ApplyModifiedProperties();
            _windowSerializedObject.ApplyModifiedProperties();

            GUILayout.Space(10f);
            if (GUILayout.Button("Create Project")) {
                CreateProjectStructure();
            }
        }

        /// <summary>
        /// Resets the current in-window state from the project-wide defaults.
        /// </summary>
        private void ApplyProjectDefaults() {
            Defaults.CopyFrom(PackageAuthoringProjectSettings.Instance);
        }

        /// <summary>
        /// Restores unsaved wizard input captured before the last domain reload within the current editor session.
        /// </summary>
        /// <returns><c>true</c> when prior session state existed.</returns>
        private bool RestoreSessionState() {
            return WizardSessionStateUtility.TryRestoreProfile(ScopedSessionStateKey, Defaults);
        }

        /// <summary>
        /// Saves the current ad hoc wizard input so script recompiles do not reset the open form.
        /// </summary>
        private void SaveSessionState() {
            WizardSessionStateUtility.SaveProfile(ScopedSessionStateKey, _defaults);
        }

        /// <summary>
        /// Applies only the project-facing portion of a preset to the current ad hoc window state.
        /// </summary>
        private void ApplyPreset(PackageAuthoringProfile preset) {
            if (preset == null) {
                ApplyProjectDefaults();
                return;
            }

            Defaults.CopyFrom(preset);
        }

        /// <summary>
        /// Restores project defaults and repaints the window so the UI reflects the new state immediately.
        /// </summary>
        private void ApplyProjectDefaultsAndRefresh() {
            ApplyProjectDefaults();
            WizardStateUtility.RefreshWindow(_defaultsSerializedObject, _windowSerializedObject, this);
        }

        /// <summary>
        /// Applies the selected preset and repaints the window.
        /// </summary>
        private void ApplyPresetAndRefresh(PackageAuthoringProfile preset) {
            ApplyPreset(preset);
            WizardStateUtility.RefreshWindow(_defaultsSerializedObject, _windowSerializedObject, this);
        }

        /// <summary>
        /// Rebuilds the serialized wrappers and cached window properties around the current wizard state.
        /// </summary>
        private void InitializeSerializedState() {
            _defaultsSerializedObject = new SerializedObject(Defaults);
            _windowSerializedObject = new SerializedObject(this);
            _autoOpenAfterCreationProperty = _windowSerializedObject.FindProperty(nameof(_autoOpenAfterCreation));
        }

        /// <summary>
        /// Opens the shared preset context menu for the standalone project wizard.
        /// </summary>
        private void ShowPresetMenu(Rect buttonRect) {
            WizardStateUtility.ShowPresetMenu(
                buttonRect,
                ApplyProjectDefaultsAndRefresh,
                ApplyPresetAndRefresh);
        }

        /// <summary>
        /// Draws editable project identity fields specific to standalone project creation.
        /// </summary>
        private void DrawProjectSettingsSection() {
            PackageAuthoringGui.DrawGeneratedProjectSettingsSection(
                _defaultsSerializedObject,
                _autoOpenAfterCreationProperty,
                "Project Settings",
                ProjectSectionTooltip,
                ProjectSectionPresetTooltip,
                ShowPresetMenu);
        }

        /// <summary>
        /// Draws the destination fields and resolved output folder preview.
        /// </summary>
        private void DrawOutputSection() {
            PackageAuthoringGui.DrawProjectOutputField(_defaultsSerializedObject);
            EditorGUILayout.LabelField("Project Folder", PreviewProjectDirectory, EditorStyles.miniLabel);
        }

        private string PreviewProjectDirectory => Path.Combine(CurrentTargetLocation, CurrentProductName);

        private string CurrentProductName => GetSerializedString(
            PackageAuthoringGui.FindProjectDefaultsProperty(_defaultsSerializedObject),
            ProductNameField,
            ProjectSettings.ProductName);

        private string CurrentTargetLocation => GetSerializedString(
            PackageAuthoringGui.FindProjectDefaultsProperty(_defaultsSerializedObject),
            TargetLocationField,
            ProjectSettings.TargetLocation);

        private static string GetSerializedString(SerializedProperty property, string relativePath, string fallback) {
            return WizardStateUtility.GetSerializedString(property, relativePath, fallback);
        }

        private static PackageAuthoringProfile CreateTemporaryProfile() {
            return WizardStateUtility.CreateTemporaryProfile();
        }

        /// <summary>
        /// Copies the template project into the chosen location and optionally opens it in Unity.
        /// </summary>
        private void CreateProjectStructure() {
            PackageAuthoringGenerationUtility.GenerateProject(ProjectSettings, _autoOpenAfterCreation);
        }
    }
}
