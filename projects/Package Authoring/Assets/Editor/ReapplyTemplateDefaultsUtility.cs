using System;
using Doji.PackageAuthoring;
using UnityEditor;
using UnityEngine;

namespace Editor {
    /// <summary>
    /// Reapplies the built-in template defaults into the package authoring project settings assets.
    /// </summary>
    internal static class ReapplyTemplateDefaultsUtility {
        /// <summary>
        /// Overwrites every editable template settings asset with the current package-provided default content.
        /// </summary>
        [MenuItem("Tools/Doji/Package Authoring/Reapply Template Defaults")]
        private static void ReapplyTemplateDefaults() {
            if (!EditorUtility.DisplayDialog(
                    "Reapply Template Defaults",
                    "This will overwrite all editable template settings in ProjectSettings with the package defaults.",
                    "Reapply",
                    "Cancel")) {
                return;
            }

            try {
                PackageAuthoringApi.ReapplyAllTemplateDefaults();
                AssetDatabase.Refresh();
                Debug.Log("Reapplied package authoring template defaults.");
            }
            catch (Exception exception) {
                Debug.LogError($"Failed to reapply template defaults.\n{exception}");
            }
        }
    }
}
