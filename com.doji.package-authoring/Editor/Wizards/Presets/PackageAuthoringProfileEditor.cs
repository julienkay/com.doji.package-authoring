using Doji.PackageAuthoring.Wizards.UI;
using UnityEditor;

namespace Doji.PackageAuthoring.Wizards.Presets {
    /// <summary>
    /// Renders package authoring preset assets with the shared grouped UI.
    /// </summary>
    [CustomEditor(typeof(PackageAuthoringProfile), true)]
    internal sealed class PackageAuthoringProfileEditor : Editor {
        /// <summary>
        /// Draws the preset asset inspector using the shared authoring sections.
        /// </summary>
        public override void OnInspectorGUI() {
            serializedObject.Update();

            EditorGUILayout.Space(4f);
            PackageAuthoringGui.DrawPackageSettingsSection(serializedObject, "Package Defaults");

            EditorGUILayout.Space(8f);
            PackageAuthoringGui.DrawRepoSettingsSection(serializedObject, "Repo Defaults");

            EditorGUILayout.Space(8f);
            PackageAuthoringGui.DrawProjectSettingsSection(
                serializedObject,
                "Project Defaults",
                "Project Name");

            serializedObject.ApplyModifiedProperties();
        }
    }
}
