# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](http://keepachangelog.com/en/1.0.0/)
and this project adheres to [Semantic Versioning](http://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2026-03-14

### Added

- Package and companion project creation wizards for scaffolding UPM-ready repository layouts.
- Template generation for package manifests, companion project manifests, asmdefs, assembly info, licenses, readmes, changelogs, sample scripts, and docfx documentation files.
- Standalone project creation wizard for cloning the template project's `Packages` and `ProjectSettings` into a new Unity project.
- Project-wide defaults and preset assets for reusing shared package, repository, and project authoring configuration across both wizards.
- Structure preview and output path previews for generated package repositories and companion projects.
- Dependency editing with manifest-style list controls and package autocomplete sourced from Unity and scoped registries.
- Registry-aware autocomplete suggestions with source badges, version hints, and scrollable overflow handling inside Project Settings and wizard UIs.
- Optional auto-open behavior for generated companion or standalone projects in the currently running Unity editor.
- Supporting editor utilities for repository initialization.
