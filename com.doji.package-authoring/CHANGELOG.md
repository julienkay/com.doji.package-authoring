# Changelog

All notable changes to this package will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this package adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-04-03

### Added

- Package and companion project creation wizards for scaffolding UPM-ready repository layouts.
- Standalone project creation wizard for cloning the template project's `Packages` and `ProjectSettings` into a new Unity project.
- Template generation for package manifests, companion project manifests, asmdefs, assembly info, licenses, readmes, changelogs, sample scripts, docfx documentation files, and repository `AGENTS.md` files.
- Project-wide defaults and preset assets for reusing shared package, repository, and project authoring configuration across both wizards.
- Structure preview and output path previews for generated package repositories and companion projects, including repository hover mappings and inspector previews.
- Dependency editing with manifest-style list controls and package autocomplete sourced from Unity and scoped registries.
- Registry-aware autocomplete suggestions with source badges, version hints, and scrollable overflow handling inside Project Settings and wizard UIs.
- Configurable repository initialization, remote URL templating, optional readme generation, configurable documentation branding assets, and configurable preferred editor selection for generated companion projects.
- Unity `.meta` file generation for scaffolded package assets so generated repositories preserve expected Unity asset metadata.
