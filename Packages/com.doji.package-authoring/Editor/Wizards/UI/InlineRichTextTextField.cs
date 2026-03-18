using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Editor.Wizards.UI {
    /// <summary>
    /// Draws a single-line IMGUI text field that highlights supported token spans without giving up native editing.
    /// </summary>
    /// <remarks>
    /// Unity's IMGUI text fields do not render rich text directly, so this control uses two synchronized layers:
    /// a real <see cref="EditorGUI.TextField(UnityEngine.Rect,string,UnityEngine.GUIStyle)"/> for keyboard input,
    /// selection, and caret handling, plus a rich-text label drawn on top for visible colored text.
    ///
    /// The input layer intentionally uses transparent glyph colors. That keeps Unity's built-in text editing behavior
    /// intact while allowing the overlay label to become the only visible text representation.
    ///
    /// Draw order matters. The editable field must render first because <see cref="EditorGUI.TextField"/> paints its
    /// own chrome and content. If the rich-text overlay renders before the field, Unity repaints over it and the text
    /// appears invisible even though selection and editing still work.
    /// </summary>
    internal static class InlineRichTextTextField {
        private static readonly Color TransparentColor = new(0f, 0f, 0f, 0f);

        /// <summary>
        /// Draws the editable field and overlays token-aware rich text for the current value.
        /// </summary>
        /// <param name="position">Full IMGUI rect including the label prefix area.</param>
        /// <param name="label">Field label drawn through Unity's standard prefix-label handling.</param>
        /// <param name="value">Current plain-text field value.</param>
        /// <returns>The updated plain-text value emitted by the underlying IMGUI text field.</returns>
        public static string Draw(Rect position, GUIContent label, string value) {
            value ??= string.Empty;

            int controlId = GUIUtility.GetControlID(FocusType.Keyboard, position);
            Rect fieldRect = EditorGUI.PrefixLabel(position, controlId, label);

            Color originalContentColor = GUI.contentColor;
            GUI.contentColor = TransparentColor;
            string updatedValue = EditorGUI.TextField(fieldRect, value);
            GUI.contentColor = originalContentColor;

            GUIStyle overlayStyle = new(EditorStyles.label) {
                alignment = TextAnchor.MiddleLeft,
                clipping = TextClipping.Clip,
                padding = EditorStyles.textField.padding,
                richText = true
            };
            overlayStyle.normal.textColor = EditorStyles.label.normal.textColor;
            GUI.Label(fieldRect, TokenHighlightRichTextFormatter.Build(updatedValue), overlayStyle);
            return updatedValue;
        }
    }
}
