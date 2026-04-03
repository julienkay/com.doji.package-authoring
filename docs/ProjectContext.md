# Project Context

## Purpose

This repository is a Unity `6000.3.5f1` template project for editor-driven scaffolding.

Its main job is to help create either:

- a new Unity application project
- a reusable UPM package repository with a companion sample project

The center of gravity is editor tooling, not gameplay/runtime content.

## Current Product Shape

The main package under development is `Packages/com.doji.package-authoring`.

That package provides Unity editor tooling for:

- creating new Unity projects from this template
- creating new UPM-style package repositories
- generating common package and repository files from templates
- managing reusable authoring defaults through project settings and preset assets

The repository also carries baseline Unity assets and settings used by the template project itself:

- `Assets/Input` for template-project Input System setup
- `Assets/Settings` for template-project URP and rendering setup
- `ProjectSettings` for template-project configuration and authoring defaults
- `Packages/manifest.json` for template-level package dependencies

## Source Of Truth

When making changes, prefer these locations:

- `Packages/` for package-owned functionality
- `Assets/` for template-project support assets and editor helpers
- `ProjectSettings` and `Packages/manifest.json` for template-project configuration and generator assumptions

For package-authoring behavior, `Packages/com.doji.package-authoring` should be treated as the primary implementation area.

## Repository Structure

- `Packages/com.doji.package-authoring/Editor/Wizards` contains the main authoring flows, models, drawers, presets, and templates.
- `Packages/com.doji.package-authoring/Editor/Utilities` contains supporting editor utilities such as Git, launcher, and JSON helpers.
- `Assets/Editor` contains template-project-level editor helpers.
- `docs/` contains human-facing documentation for this repository.

## Important Decisions

- Editor-only code stays in `Editor` folders and editor-only assemblies.
- Assembly definition boundaries should stay explicit and coherent with folder ownership.
- Unity `.meta` files must be preserved when moving Unity assets or scripts.
- Public APIs should use properties rather than raw public fields.
- Public and non-obvious internal C# types should carry XML summaries.
- This repository should keep durable project context in tracked documentation rather than relying on local IDE or agent history.

## Repository Layout Preview Hover Mapping

The package-creation UI includes a repository layout preview that can softly highlight generated files or folders when related form fields are hovered.

The hover-target workflow spans three places:

- `Packages/com.doji.package-authoring/Editor/Wizards/UI/RepositoryLayoutPreviewHoverTargets.cs`
  Holds the semantic hover target ids.
- `Packages/com.doji.package-authoring/Editor/Wizards/UI/RepositoryLayoutPreviewPanel.cs`
  Resolves each target id to one or more preview rows in `MatchesTarget(...)`.
- the relevant drawers or editor windows
  Publish active hover targets through `RepositoryLayoutPreviewHoverContext.SetHoveredTargetsIfHovered(...)`.

When adding a new hover mapping:

1. Add a new semantic target constant in `RepositoryLayoutPreviewHoverTargets`.
2. Add the corresponding `case` in `RepositoryLayoutPreviewPanel.MatchesTarget(...)`.
3. Wire the target from every IMGUI rect that should trigger the preview highlight.

Guidelines:

- Prefer semantic names such as `ProjectManifest` instead of file-path-shaped names.
- For composite controls, attach hover targets to all visible rects that should behave as one field.
- If a custom control has custom height or foldout behavior, keep hover wiring aligned with the same drawn rects.

## Current Defaults And Branding

The current defaults are still Doji-branded in several places, including package metadata and project settings presets. Notable examples:

- package name defaults such as `com.doji.package`
- company name defaults such as `Doji Technologies`
- documentation and author URLs under `doji-tech.com`

Those defaults are functional but should be considered template defaults, not permanent product truth.

## Generated Output Expectations

`ProjectCreationWizard` is expected to generate a fresh Unity project from the scaffold it owns and then apply project metadata.

`PackageCreationWizard` is expected to generate a repository that includes:

- a UPM package folder
- optional docs, samples, editor, and tests folders
- a companion sample project
- generated metadata files such as `README.md`, `LICENSE`, manifests, asmdefs, and related templates

## What Is Not Stored In Git

Conversation and assistant history are not part of the repository source of truth.

The repository `.gitignore` also excludes local IDE state such as `.idea/` and Unity-generated folders such as `Library/`, `Temp/`, `Obj/`, `Logs/`, and `UserSettings/`.

## How To Maintain This File

Update this document when any of the following changes:

- the template's intended scope
- the main package or tooling ownership boundaries
- the generated repository layout or expectations
- naming, branding, or default metadata assumptions
- architectural decisions that future work should preserve

This file is meant to capture durable project memory, not a chronological transcript.
