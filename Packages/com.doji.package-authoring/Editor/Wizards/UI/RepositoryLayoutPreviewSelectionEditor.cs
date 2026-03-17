using System.IO;
using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.UI {
    /// <summary>
    /// Displays selected repository preview entries as read-only text in the Inspector.
    /// </summary>
    [CustomEditor(typeof(RepositoryLayoutPreviewSelection))]
    internal sealed class RepositoryLayoutPreviewSelectionEditor : UnityEditor.Editor {
        private const float RowHeight = 22f;
        private const float IconButtonWidth = 30f;
        private const float IconButtonGap = 6f;

        private GUIStyle _contentStyle;
        private GUIStyle _pathFieldStyle;
        private GUIStyle _iconButtonStyle;

        /// <summary>
        /// Draws a read-only inspector that mirrors Unity's simple text-asset viewing flow.
        /// </summary>
        public override void OnInspectorGUI() {
            RepositoryLayoutPreviewSelection preview = (RepositoryLayoutPreviewSelection)target;
            _contentStyle ??= new GUIStyle(EditorStyles.textArea) {
                wordWrap = false,
                richText = false
            };
            _pathFieldStyle ??= new GUIStyle(EditorStyles.textField);
            _iconButtonStyle ??= new GUIStyle(EditorStyles.miniButton) {
                alignment = TextAnchor.MiddleCenter,
                padding = new RectOffset(4, 4, 4, 4),
                margin = new RectOffset(0, 0, 0, 0),
                fixedWidth = IconButtonWidth,
                fixedHeight = RowHeight
            };

            DrawPathRow(
                "File",
                preview.DisplayName,
                icon: EditorGUIUtility.IconContent("DefaultAsset Icon", "Open this file with the default app"),
                enabled: File.Exists(preview.SourceFilePath),
                onClick: () => EditorUtility.OpenWithDefaultApp(preview.SourceFilePath));

            DrawPathRow(
                "Path",
                preview.RelativePath,
                icon: EditorGUIUtility.IconContent("d_FolderOpened Icon", "Reveal the containing folder"),
                enabled: Directory.Exists(preview.SourceFolderPath),
                onClick: () => EditorUtility.RevealInFinder(preview.SourceFolderPath));
            
            EditorGUILayout.Space(6f);

            float width = Mathf.Max(1f, EditorGUIUtility.currentViewWidth - 28f);
            float height = Mathf.Max(220f, _contentStyle.CalcHeight(new GUIContent(preview.Content), width) + 8f);
            EditorGUILayout.SelectableLabel(
                preview.Content,
                _contentStyle,
                GUILayout.MinHeight(height),
                GUILayout.ExpandWidth(true));
        }

        /// <summary>
        /// Draws one labeled header row with the selected value and its optional action button.
        /// </summary>
        private void DrawPathRow(string label, string value, GUIContent icon, bool enabled, System.Action onClick) {
            using (new EditorGUILayout.HorizontalScope()) {
                EditorGUILayout.PrefixLabel(label);
                Rect fullRect = EditorGUILayout.GetControlRect(GUILayout.Height(RowHeight));
                Rect valueRect = new Rect(
                    fullRect.x,
                    fullRect.y,
                    Mathf.Max(0f, fullRect.width - IconButtonWidth - IconButtonGap),
                    fullRect.height);
                Rect iconRect = new Rect(
                    valueRect.xMax + IconButtonGap,
                    fullRect.y + 1f,
                    IconButtonWidth,
                    RowHeight - 2f);

                EditorGUI.SelectableLabel(valueRect, value ?? string.Empty, _pathFieldStyle);
                using (new EditorGUI.DisabledScope(!enabled)) {
                    EditorGUIUtility.AddCursorRect(iconRect, MouseCursor.Link);
                    if (GUI.Button(iconRect, icon, _iconButtonStyle)) {
                        onClick?.Invoke();
                    }
                }
            }
        }
    }
}
