using System;
using UnityEditor;
using UnityEngine;
using Doji.PackageAuthoring.Editor.Wizards.Models;
using Doji.PackageAuthoring.Editor.Wizards.Presets;

namespace Doji.PackageAuthoring.Editor.Wizards {
    /// <summary>
    /// Centralizes the temporary in-memory state used while a wizard is open.
    /// Wizards edit a transient profile, read serialized values from it, and refresh the window
    /// without mutating saved project defaults until the user generates output.
    /// </summary>
    internal static class WizardStateUtility {
        /// <summary>
        /// Applies the common initialization flow used when a creation wizard window opens.
        /// </summary>
        public static void InitializeWindow(
            EditorWindow window,
            string title,
            Func<bool> tryRestoreSessionState,
            Action applyDefaults,
            Action initializeSerializedState,
            Vector2? minSize = null,
            bool wantsMouseMove = false) {
            if (window == null) {
                return;
            }

            window.titleContent = new GUIContent(title);
            if (minSize.HasValue) {
                window.minSize = minSize.Value;
            }

            window.wantsMouseMove = wantsMouseMove;

            if (!(tryRestoreSessionState?.Invoke() ?? false)) {
                applyDefaults?.Invoke();
            }

            initializeSerializedState?.Invoke();
        }

        /// <summary>
        /// Applies the common cleanup flow used when a creation wizard window closes.
        /// </summary>
        public static void DisposeWindow(
            Action saveSessionState,
            ref PackageAuthoringProfile defaults,
            ref SerializedObject defaultsSerializedObject,
            ref SerializedObject windowSerializedObject,
            ref SerializedProperty autoOpenAfterCreationProperty,
            Action disposeExtra = null) {
            saveSessionState?.Invoke();
            disposeExtra?.Invoke();

            if (defaults != null) {
                UnityEngine.Object.DestroyImmediate(defaults);
                defaults = null;
            }

            defaultsSerializedObject = null;
            windowSerializedObject = null;
            autoOpenAfterCreationProperty = null;
        }

        /// <summary>
        /// Creates a hidden in-memory profile that mirrors the project, package, and repo defaults layout used by the wizards.
        /// </summary>
        public static PackageAuthoringProfile CreateTemporaryProfile() {
            PackageAuthoringProfile profile = ScriptableObject.CreateInstance<PackageAuthoringProfile>();
            profile.hideFlags = HideFlags.HideInHierarchy | HideFlags.DontSaveInEditor;
            profile.ProjectDefaults = new ProjectSettings();
            profile.PackageDefaults = new PackageSettings();
            profile.RepoDefaults = new RepoSettings();
            return profile;
        }

        /// <summary>
        /// Reads a serialized string field while preserving a fallback for partially initialized temporary profiles.
        /// </summary>
        public static string GetSerializedString(SerializedProperty property, string relativePath, string fallback) {
            return property?.FindPropertyRelative(relativePath)?.stringValue ?? fallback;
        }

        /// <summary>
        /// Reads a serialized boolean field while preserving a fallback for partially initialized temporary profiles.
        /// </summary>
        public static bool GetSerializedBool(SerializedProperty property, string relativePath, bool fallback) {
            return property?.FindPropertyRelative(relativePath)?.boolValue ?? fallback;
        }

        /// <summary>
        /// Resets IMGUI focus, refreshes serialized state wrappers, and repaints the host window.
        /// </summary>
        public static void RefreshWindow(
            SerializedObject defaultsSerializedObject,
            SerializedObject windowSerializedObject,
            EditorWindow window) {
            GUI.FocusControl(null);
            defaultsSerializedObject?.Update();
            windowSerializedObject?.Update();
            window?.Repaint();
        }

        /// <summary>
        /// Opens the shared preset menu used by the creation wizards.
        /// </summary>
        public static void ShowPresetMenu(
            Rect buttonRect,
            Action applyDefaultsAndRefresh,
            Action<PackageAuthoringProfile> applyPresetAndRefresh) {
            PackageAuthoringPresetMenu.Show(
                buttonRect,
                applyDefaultsAndRefresh,
                applyPresetAndRefresh);
        }
    }
}
