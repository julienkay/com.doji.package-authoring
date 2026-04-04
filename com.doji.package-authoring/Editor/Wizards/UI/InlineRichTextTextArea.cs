using UnityEditor;
using UnityEngine;

namespace Doji.PackageAuthoring.Wizards.UI {
    /// <summary>
    /// Draws a multiline IMGUI text area with an overlay that colorizes supported template tokens.
    /// </summary>
    /// <remarks>
    /// This mirrors the single-line field approach: the real text area handles editing and selection while a label
    /// drawn afterward provides the visible formatted text. The control uses its own scroll view so long templates
    /// remain navigable inside constrained settings panes while the overlay stays aligned with the editable text.
    /// </remarks>
    internal static class InlineRichTextTextArea {
        private static readonly Color TransparentColor = new(0f, 0f, 0f, 0f);
        private static readonly System.Collections.Generic.Dictionary<string, Vector2> ScrollPositions = new();

        /// <summary>
        /// Draws the editable multiline field with inline token highlighting.
        /// </summary>
        /// <param name="controlKey">Stable key used to preserve the scroll position for this text area.</param>
        /// <param name="value">Current plain-text content.</param>
        /// <param name="minHeight">Visible viewport height reserved for the text area.</param>
        /// <returns>The updated plain-text content emitted by the underlying IMGUI text area.</returns>
        public static string DrawLayout(string controlKey, string value, float minHeight) {
            value ??= string.Empty;

            GUIStyle inputStyle = EditorStyles.textArea;
            Rect viewportRect = GetViewportRect(minHeight);
            Rect contentRect = GetContentRect(value, viewportRect.width, minHeight, inputStyle);
            Vector2 scrollPosition = GetScrollPosition(controlKey);

            scrollPosition = GUI.BeginScrollView(viewportRect, scrollPosition, contentRect);

            Rect areaRect = new(0f, 0f, contentRect.width, contentRect.height);
            Color originalContentColor = GUI.contentColor;
            GUI.contentColor = TransparentColor;
            string updatedValue = EditorGUI.TextArea(areaRect, value);
            GUI.contentColor = originalContentColor;

            GUIStyle overlayStyle = CreateOverlayStyle(inputStyle, EditorStyles.label.normal.textColor);
            GUI.Label(areaRect, TokenHighlightRichTextFormatter.Build(updatedValue), overlayStyle);

            GUI.EndScrollView();
            ScrollPositions[controlKey] = scrollPosition;
            return updatedValue;
        }

        /// <summary>
        /// Draws a read-only multiline field that keeps token highlighting while still allowing selection and copy.
        /// </summary>
        /// <remarks>
        /// A disabled IMGUI text area blocks selection, so the read-only variant uses a transparent selectable layer for
        /// text interaction and a muted rich-text overlay for the visible content.
        /// </remarks>
        /// <param name="controlKey">Stable key used to preserve the scroll position for this text area.</param>
        /// <param name="value">Plain-text content to display.</param>
        /// <param name="minHeight">Visible viewport height reserved for the text area.</param>
        /// <param name="baseTextColor">Muted base color used for non-token text.</param>
        public static void DrawReadOnlyLayout(string controlKey, string value, float minHeight, Color baseTextColor) {
            value ??= string.Empty;

            GUIStyle selectionStyle = new(EditorStyles.textArea);
            Rect viewportRect = GetViewportRect(minHeight);
            Rect contentRect = GetContentRect(value, viewportRect.width, minHeight, selectionStyle);
            Vector2 scrollPosition = GetScrollPosition(controlKey);

            scrollPosition = GUI.BeginScrollView(viewportRect, scrollPosition, contentRect);

            Rect areaRect = new(0f, 0f, contentRect.width, contentRect.height);
            Color originalContentColor = GUI.contentColor;
            GUI.contentColor = TransparentColor;
            EditorGUI.SelectableLabel(areaRect, value, selectionStyle);
            GUI.contentColor = originalContentColor;

            GUIStyle overlayStyle = CreateOverlayStyle(selectionStyle, baseTextColor);
            GUI.Label(areaRect, TokenHighlightRichTextFormatter.Build(value), overlayStyle);

            GUI.EndScrollView();
            ScrollPositions[controlKey] = scrollPosition;
        }

        private static Rect GetViewportRect(float minHeight) {
            return EditorGUILayout.GetControlRect(
                hasLabel: false,
                height: minHeight,
                GUILayout.MinHeight(minHeight),
                GUILayout.ExpandHeight(false));
        }

        private static Rect GetContentRect(string value, float viewportWidth, float minHeight, GUIStyle style) {
            float contentWidth = Mathf.Max(viewportWidth - 18f, 120f);
            float calculatedHeight = Mathf.Max(minHeight, style.CalcHeight(new GUIContent(value), contentWidth));
            return new Rect(0f, 0f, contentWidth, calculatedHeight);
        }

        private static Vector2 GetScrollPosition(string controlKey) {
            if (string.IsNullOrEmpty(controlKey)) {
                return Vector2.zero;
            }

            return ScrollPositions.TryGetValue(controlKey, out Vector2 scrollPosition)
                ? scrollPosition
                : Vector2.zero;
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
