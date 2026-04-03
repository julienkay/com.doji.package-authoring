---
name: repo-layout-hover-mapping
description: Add or update repository layout preview hover mappings in this Unity package-authoring project. Use when a wizard field should highlight generated files or folders in the repository layout preview, or when existing hover highlight behavior needs to be fixed.
---

# Repo Layout Hover Mapping

Use this skill for changes to the package-creation repository layout preview hover system.

The workflow spans three places:

1. `Packages/com.doji.package-authoring/Editor/Wizards/UI/RepositoryLayoutPreviewHoverTargets.cs`
   Add or update the semantic target id.
2. `Packages/com.doji.package-authoring/Editor/Wizards/UI/RepositoryLayoutPreviewPanel.cs`
   Map that target in `MatchesTarget(...)`.
3. The relevant drawer or window
   Publish the target from the drawn IMGUI rects via `RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(...)`.

Rules:

- Prefer semantic target names such as `ProjectManifest`, not file-path-shaped names.
- Wire all relevant rects for composite controls, not just the first visible row.
- If the control has custom foldout or dynamic-height behavior, keep the hover wiring aligned with the same rects used by the height calculation.
- If the hovered file or folder is not already present in the repository layout preview tree, add it there too.
- If preview visibility depends on toggles or options, make sure `RepositoryLayoutPreviewData` and `PreviewSnapshotKey.Create(...)` include the relevant state so the preview cache invalidates correctly.

Verification checklist:

- confirm the target constant exists
- confirm `MatchesTarget(...)` maps it to the intended preview rows
- confirm every intended UI rect publishes the target
- confirm the preview tree actually contains the hovered node
- confirm the preview snapshot key changes when the hovered node is conditionally included
