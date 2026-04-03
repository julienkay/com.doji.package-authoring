# Project Memory

## Repository Purpose

- This repository is a Unity `6000.3.5f1` template project centered on editor-driven scaffolding.
- Its main jobs are creating a new Unity application project and creating a reusable UPM package repository with a companion sample project.
- The center of gravity is editor tooling rather than gameplay or runtime content.

## Current Product Shape

- The main package under development is `Packages/com.doji.package-authoring`.
- That package owns the editor tooling for project creation, package repository creation, generated metadata/templates, and reusable authoring defaults.
- The repository also contains host-project baseline assets and settings used by the template project itself, including `Assets/Input`, `Assets/Settings`, `ProjectSettings`, and `Packages/manifest.json`.

## Package Audit Scope

- Treat `Packages/com.doji.package-authoring` as the only shipped/publishable artifact for package-readiness reviews.
- Do not flag repository-root `Assets/`, `ProjectSettings/`, or ad hoc local preset assets as publish-blocking package issues unless the user explicitly asks for whole-repo cleanup.
- Non-committed local assets such as personal presets are not part of the package audit scope.

## Source Of Truth

- Prefer `Packages/com.doji.package-authoring` for package-authoring behavior and package-owned implementation work.
- Use repository-root `Assets/`, `ProjectSettings`, and `Packages/manifest.json` when the task is explicitly about template-host support assets or generator baseline configuration.

## Repository Structure

- `Packages/com.doji.package-authoring/Editor/Wizards` contains the main authoring flows, models, drawers, presets, and templates.
- `Packages/com.doji.package-authoring/Editor/Utilities` contains supporting editor utilities such as Git, launcher, JSON, and image helpers.
- `Assets/Editor` contains template-project-level editor helpers.
- `docs/` is for human-facing documentation, not durable agent memory.

## Current Intentional Decisions

- The current repository layout assumptions are intentional for now. Do not flag the fixed package/repo/companion-project structure as an issue unless the user asks for configurability work.
- DocFX-specific documentation generation is intentional. Doc generation is optional, and DocFX specialization is acceptable.
- Branding under `Doji` is expected for this package unless the user asks for a rename.
- Minor polish items like placeholder defaults or sample-script identifier cleanup are low priority unless the user explicitly requests them.

## Engineering Conventions

- Keep editor-only code in `Editor` folders and editor-only assemblies.
- Keep asmdef boundaries explicit and coherent with folder ownership.
- Preserve Unity `.meta` files when moving assets or scripts.
- Prefer property-based public APIs over raw public fields.
- Public and non-obvious internal C# types should carry XML summaries.

## Generated Output Expectations

- `ProjectCreationWizard` generates a fresh Unity project from the scaffold it owns and then applies project metadata.
- `PackageCreationWizard` generates a repository with a UPM package folder, optional docs/samples/editor/tests folders, a companion sample project, and generated metadata such as `README.md`, `LICENSE`, manifests, asmdefs, and related template outputs.

## Persistence Guidance

- Durable project context belongs in tracked project memory files, not in conversational history.
- Conversation and assistant history are not part of the repository source of truth.

## Audit History Notes

- A previous audit over-reported issues from the template host project. Future audits should scope findings to the shipped package first.
- Remaining high-signal concerns should focus on package-owned hardcoded assumptions or brittle generation behavior inside `Packages/com.doji.package-authoring`.
