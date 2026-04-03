# Manual

`com.doji.package-authoring` provides Unity editor tooling for scaffolding reusable UPM packages, companion Unity projects, and the repository files that support them.

## What This Package Does

The package is centered around two editor windows:

- `Tools/Package Creation Wizard` creates a repository that contains a UPM package, optional supporting folders, and a companion Unity project.
- `Tools/Project Creation Wizard` creates a standalone Unity project from the shared project defaults.

The package also adds project-scoped defaults and template editors under `Project Settings > Doji > Package Authoring`.

## Recommended Reading Order

- [Package Creation Wizard](package-creation-wizard.md) for the end-to-end workflow and the generated repository shape.
- [Package Creation Wizard Settings](package-creation-wizard-settings.md) for a field-by-field reference of every setting shown in the main wizard window.
- [Defaults And Presets](defaults-and-presets.md) for project defaults, preset assets, and how values are shared across tools.
- [Templates](templates.md) for repository, package, and documentation template customization.
- [Companion And Standalone Projects](projects.md) for how generated Unity projects are assembled.

## Key Concepts

### Project Defaults

The settings page at `Project Settings > Doji > Package Authoring` stores the baseline values used when a new wizard window opens. These defaults are saved per Unity project.

### Presets

Preset assets capture a reusable package-authoring profile. In the package creation wizard, the package/repository side and the companion-project side each have their own preset button so you can apply only the relevant portion of a preset.

### Templates

Generated text files such as `README.md`, `AGENTS.md`, `LICENSE`, and the DocFX scaffold are backed by editable project-scoped templates. Template placeholders are resolved from the current package, repository, and project values at generation time.

## API Reference

The [Scripting API](../api/index.md) documents the editor-facing types exposed by the package.
