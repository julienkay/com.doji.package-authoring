---
name: user-facing-docfx-docs
description: Write or revise user-facing DocFX documentation for this Unity package-authoring project. Use when expanding the manual, documenting wizards/settings/templates, improving DocFX navigation, or separating consumer documentation from maintainer-only build notes.
---

# User-Facing DocFX Docs

Use this skill when editing `docs/` for package consumers.

## Audience split

Keep user-facing docs focused on using the package:

- what the tool does
- when to use each workflow
- what each visible setting changes
- what files or output the user should expect

Keep maintainer-only material out of consumer docs.

## Source of truth

Do not guess from UI text alone. Verify against implementation before documenting behavior.

Read only the files needed for the current page, usually:

- wizard window classes in `com.doji.package-authoring/Editor/Wizards/`
- settings models in `com.doji.package-authoring/Editor/Wizards/Models/`
- drawers and shared GUI in `com.doji.package-authoring/Editor/Wizards/Drawers/` and `.../UI/`
- template providers and template classes in `.../Presets/` and `.../Templates/`

Document actual behavior, including:

- conditional fields
- foldouts and hidden sections
- generated files and folders
- token support
- side effects in manifests or project output

## Documentation standards

Prefer task-oriented docs first, then reference depth:

1. Landing page with workflow map
2. Task pages for major tools
3. Reference pages for dense UI surfaces such as wizard settings
4. Supporting pages for templates, presets/defaults, and generated output

Before rewriting a page or section, decide these three things privately:

1. Where does this page sit in the docs flow?
2. What job should this section do for the future reader?
3. What is the main point the reader must leave with?

Reflect those answers through refactoring, not explanation:

- change headings so they carry the right question or topic
- move sections so the reader gets orientation before detail
- cut repetition that does not help a decision or action
- replace vague framing with concrete effects, outputs, paths, or UI behavior

Write to the future reader of the docs, not to the current user prompt:

- avoid meta lines such as "this page explains" or "the goal of this page is" unless the orientation is genuinely useful
- do not narrate your documentation strategy inside the docs
- show the important consequence directly instead of explaining that you are about to explain it
- if a sentence sounds like reassurance to the requester rather than guidance to the product user, rewrite or delete it
- prefer refactoring-style fixes to the prose: rename headings, reorder sections, tighten labels, or replace meta sentences with direct content

The resulting prose should sound like product documentation:

- direct, specific, and user-facing
- centered on UI behavior, generated output, and reader decisions
- written as statements of fact, consequences, paths, options, and workflows
- structured so headings do orientation work and paragraphs do explanatory work
- free of conversational reassurance, prompt-response tone, or commentary about the writing itself

Prefer showing over telling:

- if the point is that a field changes generated output, show the output it changes
- if the point is that a toggle reveals nested options, document the conditional behavior directly
- if the point is that a page is for reference, make the headings and layout reference-friendly instead of saying so
- if the point is that a workflow is the main entry point, prove it through ordering, screenshots, and link placement

Default to visuals-first documentation:

- if a screenshot explains a UI section faster than prose, place the image before the explanation
- when documenting a specific editor window section, include a screenshot of that section
- if a page introduces a major workflow surface, include an overview screenshot near the top
- create placeholder PNGs in `docs/images/` when the final screenshots are not available yet, and reference them from the markdown immediately

Screenshot best practices:

- prefer cropped, purpose-specific screenshots over one huge full-screen capture when the page is about a specific section
- keep the relevant control visible in the first screenful; do not make the reader hunt for the thing being discussed
- use one overview screenshot for orientation, then smaller section screenshots for detailed reference pages
- when a screenshot is meant to explain a toggle, foldout, or preset menu, capture it in the state being described
- keep filenames descriptive and stable, usually `<page-topic>-<section>.png`
- when adding a placeholder, use the final intended filename so the real screenshot can replace it without markdown changes
- keep alt text functional and specific, describing the UI surface shown rather than repeating the heading verbatim

For wizard documentation:

- document every visible setting in the main window
- explain what the field writes or changes
- call out when a field appears only if another toggle is enabled
- distinguish package, repository, and project scope clearly

For template documentation:

- explain which generated file each template controls
- separate editable templates from built-in locked templates
- list supported tokens only if the implementation supports them

## Project-specific conventions

- Use `docs/manual/` for user-facing guidance pages.
- Keep `docs/index.md` as a concise entry page that points into the manual and API reference.
- Update `docs/manual/toc.yml` when adding or reordering manual pages.
- Keep the tone practical and explicit. Avoid filler marketing language.
- Prefer concrete file names, menu paths, and generated output paths.
- Put screenshot placeholders under `docs/images/`, preferably grouped by topic such as `docs/images/manual/`.

## Local preview workflow

After changing user-facing docs in this repository, default to rebuilding and serving the DocFX site unless the user explicitly says not to.

Use the local wrapper script:

- `./scripts/docfx.sh metadata docs/docfx.json`
- `./scripts/docfx.sh build docs/docfx.json`
- `./scripts/docfx.sh serve docs/_site`

Notes:

- Run `metadata` before `build` when API docs may have changed, especially after changing public types, namespaces, XML documentation, or assembly-visible API shape.
- `build` alone does not regenerate this repository's API metadata under `docs/api/`.
- Run the build after documentation edits so navigation and broken-link issues surface immediately. Treat a successful `build` as the authoritative local verification step.
- `serve` does not rebuild; it only serves the current `docs/_site` output.
- If a serve session is already running for the current workspace, prefer reusing it after a fresh build instead of starting duplicates.
- Do not assume HTTP checks from the current Codex session can reach a DocFX server started by another session. Session-local `curl` failures do not prove the preview is stale or broken.
- Use HTTP checks such as `curl -I --max-time 2 http://127.0.0.1:8080` only as positive confirmation when they succeed from the current session.
- If a preview server likely already exists but this session cannot reach it, report that the build succeeded and that preview reachability appears session-local rather than treating it as a DocFX failure.
- Report the local preview URL or any build errors back to the user. When reusing an existing preview, make it clear whether the URL was confirmed from the current session or inferred from an already-running server.
- If namespaces or generated API file names changed, stale files can linger in `docs/api/` and `docs/_site/api/`. In that case, clean generated API outputs first while preserving hand-authored files such as `docs/api/index.md` and `docs/api/.gitignore`.

## Verification checklist

Before finishing:

- confirm every newly documented field or behavior against code
- confirm every new manual page is linked from `docs/manual/toc.yml`
- confirm `docs/index.md` still matches the manual structure
- confirm user-facing pages do not contain maintainer-only DocFX build instructions
- confirm image references resolve to actual files under `docs/images/`
- run `./scripts/docfx.sh metadata docs/docfx.json` when API docs may have changed
- run `./scripts/docfx.sh build docs/docfx.json` unless the user explicitly skips local verification
- start, refresh, or reuse `./scripts/docfx.sh serve docs/_site` unless the user explicitly skips local preview
- do not treat a failed `curl` from the current session as evidence that an already-running preview server is unusable
- if stale served output is involved, remember that `docfx serve docs/_site` serves existing generated files and does not rebuild them
