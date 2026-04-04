---
name: package-changelog-writing
description: Write or revise changelog entries for this Unity package-authoring project. Use when updating `com.doji.package-authoring/CHANGELOG.md`, deciding how unreleased work should be categorized, or summarizing package-facing changes for the upcoming release.
---

# Package Changelog Writing

Use this skill for edits to `com.doji.package-authoring/CHANGELOG.md`.

Primary source of truth:

- `com.doji.package-authoring/CHANGELOG.md`
- the current package code under `com.doji.package-authoring/`

Scope rules:

- Write changelog entries for the shipped package, not for the template host project, unless the host-project asset is part of the package-facing workflow.
- Summarize user-facing package capabilities, generated output behavior, public API additions, and meaningful workflow changes.
- Do not narrate internal refactors unless they affect package users, generated output, or the exposed scripting API.
- Do not add bullets for documentation work, local tooling notes, or maintainer workflow changes unless those changes introduce a package-facing capability users actually receive.

Pre-1.0 release rule:

- While the package is still preparing for its initial release, treat changelog edits as part of the upcoming release scope.
- Do not invent release history that did not happen.
- Avoid `Changed` language that implies an already released baseline unless the package actually shipped that earlier behavior.
- Prefer folding new unreleased work into the upcoming release's `Added` section when the feature has not been released before.

Section choice rules:

- Use `Added` for new user-visible capabilities, public APIs, new generated outputs, or new configuration surfaces.
- Use `Changed` only when a previously released behavior was intentionally altered in a way users should know about.
- Use `Fixed` only for regressions or incorrect released behavior.
- Avoid filler sections when there is nothing meaningful to record there.

Writing rules:

- Write for package users, not for commit history.
- Prefer one concise bullet per distinct capability.
- Do not split one package-facing feature into multiple bullets just because it introduced supporting types, helper models, or namespace cleanup.
- Lead with the outcome, not implementation detail.
- Name public namespaces, menu paths, generated file paths, or major outputs when they help users understand the change.
- Keep bullets specific enough that a future reader can tell what shipped without reading the code.
- Exclude package-maintainer housekeeping such as documentation additions, local build workflow notes, or process clarifications unless they change the shipped package experience.

Checks before finishing:

- confirm each bullet matches current package behavior
- confirm entries describe package-facing changes rather than editor-internal churn
- confirm category choice (`Added`, `Changed`, `Fixed`) matches actual release state
- for pre-1.0 work, confirm the entry reads like upcoming release scope rather than retroactive history
