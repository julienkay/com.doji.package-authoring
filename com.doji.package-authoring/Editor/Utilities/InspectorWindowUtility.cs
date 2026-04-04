using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Doji.PackageAuthoring.Utilities {
    /// <summary>
    /// Centralizes best-effort access to Unity's internal Inspector window until a public activation API exists.
    /// </summary>
    internal static class InspectorWindowUtility {
        private static readonly Type InspectorWindowType = Type.GetType("UnityEditor.InspectorWindow,UnityEditor");
        private static readonly MethodInfo ShowTabMethod = typeof(EditorWindow).GetMethod(
            "ShowTab",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        /// <summary>
        /// Attempts to reveal an already-open Inspector tab without creating a new Inspector window.
        /// </summary>
        /// <returns><see langword="true"/> when an open Inspector instance was found and targeted.</returns>
        public static bool TryRevealOpenInspector() {
            if (InspectorWindowType == null) {
                return false;
            }

            Object[] inspectorWindows = Resources.FindObjectsOfTypeAll(InspectorWindowType);
            if (inspectorWindows == null || inspectorWindows.Length == 0) {
                return false;
            }

            EditorWindow inspectorWindow = inspectorWindows[0] as EditorWindow;
            if (inspectorWindow == null) {
                return false;
            }

            ShowTabMethod?.Invoke(inspectorWindow, null);
            inspectorWindow.Focus();
            inspectorWindow.Repaint();
            return true;
        }
    }
}
