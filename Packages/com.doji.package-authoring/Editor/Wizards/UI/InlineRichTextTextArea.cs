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
            Rect areaRect = GetTextAreaRect(value, minHeight, inputStyle);

            Color originalContentColor = GUI.contentColor;
            GUI.contentColor = TransparentColor;
            string updatedValue = EditorGUI.TextArea(areaRect, value);
            GUI.contentColor = originalContentColor;

            GUIStyle overlayStyle = CreateOverlayStyle(inputStyle, EditorStyles.label.normal.textColor);
            GUI.Label(areaRect, TokenHighlightRichTextFormatter.Build(updatedValue), overlayStyle);
            return updatedValue;
        }

        /// <summary>
        /// Draws a read-only multiline field that keeps token highlighting while still allowing selection and copy.
        /// </summary>
        /// <remarks>
        /// A disabled IMGUI text area blocks selection, so the read-only variant uses a transparent selectable layer for
        /// text interaction and a muted rich-text overlay for the visible content.
        /// </remarks>
        /// <param name="value">Plain-text content to display.</param>
        /// <param name="minHeight">Minimum reserved height for the text area.</param>
        /// <param name="baseTextColor">Muted base color used for non-token text.</param>
        public static void DrawReadOnlyLayout(string value, float minHeight, Color baseTextColor) {
            value ??= string.Empty;

            GUIStyle selectionStyle = new(EditorStyles.textArea);
            Rect areaRect = GetTextAreaRect(value, minHeight, selectionStyle);

            Color originalContentColor = GUI.contentColor;
            GUI.contentColor = TransparentColor;
            EditorGUI.SelectableLabel(areaRect, value, selectionStyle);
            GUI.contentColor = originalContentColor;

            GUIStyle overlayStyle = CreateOverlayStyle(selectionStyle, baseTextColor);
            GUI.Label(areaRect, TokenHighlightRichTextFormatter.Build(value), overlayStyle);
        }

        private static Rect GetTextAreaRect(string value, float minHeight, GUIStyle style) {
            float width = Mathf.Max(EditorGUIUtility.currentViewWidth - 48f, 120f);
            float calculatedHeight = Mathf.Max(minHeight, style.CalcHeight(new GUIContent(value), width));
            return EditorGUILayout.GetControlRect(
                hasLabel: false,
                height: calculatedHeight,
                style,
                GUILayout.MinHeight(minHeight),
                GUILayout.ExpandHeight(false));
        }

        private static GUIStyle CreateOverlayStyle(GUIStyle inputStyle, Color textColor) {
            GUIStyle overlayStyle = new(EditorStyles.label) {
                alignment = TextAnchor.UpperLeft,
                clipping = TextClipping.Clip,
                padding = inputStyle.padding,
                richText = true
            };
            overlayStyle.normal.textColor = textColor;
            overlayStyle.hover.textColor = textColor;
            overlayStyle.focused.textColor = textColor;
            overlayStyle.active.textColor = textColor;
            return overlayStyle;
        }
    }
}
