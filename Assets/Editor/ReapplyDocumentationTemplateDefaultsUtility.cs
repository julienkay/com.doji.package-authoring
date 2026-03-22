using System;
using Doji.PackageAuthoring.Editor.Wizards.Presets;
using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Utilities {
    /// <summary>
    /// Reapplies the built-in documentation template defaults into the package authoring project settings assets.
    /// </summary>
    internal static class ReapplyDocumentationTemplateDefaultsUtility {
        /// <summary>
        /// Overwrites every documentation template settings asset with the current package-provided default content.
        /// </summary>
        [MenuItem("Tools/Doji/Package Authoring/Reapply Documentation Template Defaults")]
        private static void ReapplyDocumentationTemplateDefaults() {
            if (!EditorUtility.DisplayDialog(
                    "Reapply Documentation Template Defaults",
                    "This will overwrite all documentation template settings in ProjectSettings with the package defaults.",
                    "Reapply",
                    "Cancel")) {
                return;
            }

            try {
                PackageAuthoringProjectSettingsApi.ReapplyDocumentationTemplateDefaults();
                AssetDatabase.Refresh();
                Debug.Log("Reapplied package authoring documentation template defaults.");
            }
            catch (Exception exception) {
                Debug.LogError($"Failed to reapply documentation template defaults.\n{exception}");
            }
        }
    }
}
