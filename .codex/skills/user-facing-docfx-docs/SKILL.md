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

## Verification checklist

Before finishing:

- confirm every newly documented field or behavior against code
- confirm every new manual page is linked from `docs/manual/toc.yml`
- confirm `docs/index.md` still matches the manual structure
- confirm user-facing pages do not contain maintainer-only DocFX build instructions
- if stale served output is involved, remember that `docfx serve docs/_site` serves existing generated files and does not rebuild them
