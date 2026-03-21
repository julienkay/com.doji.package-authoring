using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.UI {
    /// <summary>
    /// Draws a multiline IMGUI text area with an overlay that colorizes supported template tokens.
    /// </summary>
    /// <remarks>
    /// This mirrors the single-line field approach: the real text area handles editing and selection while a label
    /// drawn afterward provides the visible formatted text. The control height is expanded to fit the content so the
    /// outer inspector or settings scroll view handles overflow instead of relying on an internal text-area scroll
    /// offset that the overlay could not keep in sync.
    /// </remarks>
    internal static class InlineRichTextTextArea {
        private static readonly Color TransparentColor = new(0f, 0f, 0f, 0f);

        /// <summary>
        /// Draws the editable multiline field with inline token highlighting.
        /// </summary>
        /// <param name="value">Current plain-text content.</param>
        /// <param name="minHeight">Minimum reserved height for the text area.</param>
        /// <returns>The updated plain-text content emitted by the underlying IMGUI text area.</returns>
        public static string DrawLayout(string value, float minHeight) {
            value ??= string.Empty;

            GUIStyle inputStyle = EditorStyles.textArea;
            float width = Mathf.Max(EditorGUIUtility.currentViewWidth - 48f, 120f);
            float calculatedHeight = Mathf.Max(minHeight, inputStyle.CalcHeight(new GUIContent(value), width));
            Rect areaRect = EditorGUILayout.GetControlRect(
                hasLabel: false,
                height: calculatedHeight,
                inputStyle,
                GUILayout.MinHeight(minHeight),
                GUILayout.ExpandHeight(false));

            Color originalContentColor = GUI.contentColor;
            GUI.contentColor = TransparentColor;
            string updatedValue = EditorGUI.TextArea(areaRect, value);
            GUI.contentColor = originalContentColor;

            GUIStyle overlayStyle = new(EditorStyles.label) {
                alignment = TextAnchor.UpperLeft,
                clipping = TextClipping.Clip,
                padding = inputStyle.padding,
                richText = true
            };
            overlayStyle.normal.textColor = EditorStyles.label.normal.textColor;

            GUI.Label(areaRect, TokenHighlightRichTextFormatter.Build(updatedValue), overlayStyle);
            return updatedValue;
        }
    }
}
