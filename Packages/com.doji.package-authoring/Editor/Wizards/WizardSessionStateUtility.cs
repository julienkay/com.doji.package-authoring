using UnityEditor;
using Doji.PackageAuthoring.Editor.Wizards.Models;

namespace Doji.PackageAuthoring.Editor.Wizards {
    /// <summary>
    /// Stores transient wizard input across script reloads without promoting ad hoc form state into project defaults.
    /// </summary>
    internal static class WizardSessionStateUtility {
        /// <summary>
        /// Attempts to overwrite the target profile from editor session state.
        /// </summary>
        /// <param name="key">Unique storage key for the wizard.</param>
        /// <param name="target">Existing profile instance to hydrate.</param>
        /// <returns><c>true</c> when persisted state existed and was applied.</returns>
        public static bool TryRestoreProfile(string key, PackageAuthoringProfile target) {
            string json = SessionState.GetString(key, string.Empty);
            if (string.IsNullOrWhiteSpace(json) || target == null) {
                return false;
            }

            target.ProjectDefaults ??= new ProjectSettings();
            target.PackageDefaults ??= new PackageSettings();
            target.RepoDefaults ??= new RepoSettings();
            EditorJsonUtility.FromJsonOverwrite(json, target);
            return true;
        }

        /// <summary>
        /// Persists the current profile into editor session state for restoration after domain reload.
        /// </summary>
        /// <param name="key">Unique storage key for the wizard.</param>
        /// <param name="source">Current ad hoc wizard profile.</param>
        public static void SaveProfile(string key, PackageAuthoringProfile source) {
            if (source == null) {
                return;
            }

            SessionState.SetString(key, EditorJsonUtility.ToJson(source));
        }
    }
}
