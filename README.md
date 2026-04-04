<p align="right">
  <a href="https://www.doji-tech.com/">
    <img src="https://www.doji-tech.com/assets/favicon.ico" alt="Doji" title="Doji" height="70" />
  </a>
</p>

# Package Authoring

Unity editor tooling to create reusable UPM packages, companion Unity projects, and supporting repository files.

[OpenUPM] · [Asset Store] · [Documentation]

## About

This repository is the source for the `com.doji.package-authoring` package.

The tooling currently focuses on two editor workflows:

- `Package Authoring/Create Package...` generates a package repository with a UPM package, repository files, optional docs, and a companion Unity project.
- `Tools/Project Creation Wizard` generates a standalone Unity project from the same shared project baseline and settings model.

## What It Includes

- `Package Creation Wizard` for creating a new package repository with optional `Editor`, `Samples~`, `Tests`, `docs`, `LICENSE`, `README.md`, and `AGENTS.md` output.
- `Project Creation Wizard` for creating standalone Unity projects from the maintained baseline in this repo.
- Presets and project settings providers for authoring defaults, templates, and reusable generation profiles.
- Package dependency search and manifest generation helpers for companion projects.
- DocFX documentation templates and generated documentation for the package itself.

## Installation Options

### [OpenUPM]

CLI: ```openupm add com.doji.package-authoring```

<details>
  <summary>Install via the Unity Package Manager UI</summary>

  1. In `Edit -> Project Settings -> Package Manager`, add a new scoped registry:

         Name: package.openupm.com
         URL: https://package.openupm.com
         Scope(s): com.doji

  2. Open the Package Manager and switch to `My Registries`.

  3. Install `com.doji.package-authoring` either by name or by selecting it from the package list.

</details>

### [Asset Store]

### Git URL

Add the package from Git in Unity Package Manager:

```text
https://github.com/julienkay/com.doji.package-authoring.git?path=com.doji.package-authoring
```

### Local Development

This repository already contains a companion Unity project under [`projects/Package Authoring`][companion-project] for working on the package locally.

Open that project in Unity `6000.3.10f1` or a compatible Unity 6.3 editor, then use:

- `Package Authoring/Create Package...`
- `Tools/Project Creation Wizard`

## Repository Layout

- [`com.doji.package-authoring`][package-root] contains the package source.
- [`com.doji.package-authoring/Editor`][package-editor] contains the editor-only implementation, wizards, templates, presets, and supporting utilities.
- [`projects/Package Authoring`][companion-project] is the host Unity project used to develop and validate the package.
- [`docs`][docs-root] contains the DocFX manual and API reference source.

## Documentation

The package documentation is available at:

- [Manual]
- [Package Creation Wizard]
- [Templates]
- [Scripting API]

[OpenUPM]: https://openupm.com/packages/com.doji.package-authoring/
[Asset Store]: https://assetstore.unity.com/packages/slug/371718
[Documentation]: https://docs.doji-tech.com/com.doji.package-authoring/
[Manual]: https://docs.doji-tech.com/com.doji.package-authoring/manual/index.html
[Package Creation Wizard]: https://docs.doji-tech.com/com.doji.package-authoring/manual/package-creation-wizard.html
[Templates]: https://docs.doji-tech.com/com.doji.package-authoring/manual/templates.html
[Scripting API]: https://docs.doji-tech.com/com.doji.package-authoring/api/index.html
[companion-project]: ./projects/Package%20Authoring
[package-root]: ./com.doji.package-authoring
[package-editor]: ./com.doji.package-authoring/Editor
[docs-root]: ./docs
