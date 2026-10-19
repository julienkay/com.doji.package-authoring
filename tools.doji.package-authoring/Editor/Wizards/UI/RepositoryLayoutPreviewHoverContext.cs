using System.Collections.Generic;
using UnityEngine;

namespace Doji.PackageAuthoring.Wizards.UI {
    /// <summary>
    /// Tracks the field currently hovered in the wizard so the preview can softly emphasize related rows.
    /// </summary>
    /// <remarks>
    /// The shared drawers can publish hover intents from multiple host surfaces, but only
    /// <see cref="RepositoryLayoutPreviewPanel"/> currently consumes this state.
    /// </remarks>
    internal static class RepositoryLayoutPreviewHoverContext {
        private static readonly HashSet<string> HoveredTargets = new();

        public static IReadOnlyCollection<string> CurrentTargets => HoveredTargets;

        public static void BeginFrame() {
            HoveredTargets.Clear();
        }

        public static void SetHoveredTargetsIfHovered(Rect rect, params string[] hoverTargets) {
            if (hoverTargets == null || hoverTargets.Length == 0 || !rect.Contains(Event.current.mousePosition)) {
                return;
            }

            HoveredTargets.Clear();
            foreach (string hoverTarget in hoverTargets) {
                if (!string.IsNullOrWhiteSpace(hoverTarget)) {
                    HoveredTargets.Add(hoverTarget);
                }
            }
        }
    }
}
