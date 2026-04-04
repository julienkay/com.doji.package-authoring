# Scripting API

Editor scripts can drive the same generation pipeline as the package-authoring windows through `Doji.PackageAuthoring.PackageAuthoringApi`.

Use this entry point when you want to automate project or package scaffolding from editor code without opening the editor windows manually.

## Namespace And Types

Import these namespaces in your editor script:

```csharp
using Doji.PackageAuthoring;
using Doji.PackageAuthoring.Models;
```

The main public types are:

- `PackageAuthoringApi`
- `ProjectSettings`
- `PackageSettings`
- `RepoSettings`
- `PackageDependencyList`
- `PackageDependencyEntry`
- `PreferredEditor`
- `LicenseType`

## Generate A Standalone Project

Use `PackageAuthoringApi.GenerateProject(...)` when you only want a generated Unity project.

```csharp
using Doji.PackageAuthoring;
using Doji.PackageAuthoring.Models;

ProjectSettings project = new ProjectSettings {
    CompanyName = "Doji",
    ProductName = "My Game",
    Version = "1.0.0",
    PreferredEditor = PreferredEditor.Rider,
    TargetLocation = "../Generated"
};

project.IncludedPackages.Items.Add(new PackageDependencyEntry {
    PackageName = "com.unity.addressables",
    Version = "2.7.4"
});

string projectPath = PackageAuthoringApi.GenerateProject(project);
```

This creates a standalone project at:

`<Target Location>/<Project Name>`

## Generate A Package Repository

Use `PackageAuthoringApi.GeneratePackage(...)` when you want:

- a repository root
- a generated package folder
- optional docs, samples, editor, and tests folders
- a companion Unity project under `projects/<Project Name>`

```csharp
using Doji.PackageAuthoring;
using Doji.PackageAuthoring.Models;

ProjectSettings project = new ProjectSettings {
    CompanyName = "Doji",
    ProductName = "My Package Sandbox",
    Version = "1.0.0",
    PreferredEditor = PreferredEditor.Rider,
    TargetLocation = "../Generated"
};

PackageSettings package = new PackageSettings {
    PackageName = "com.doji.mypackage",
    PackageDisplayName = "My Package",
    AssemblyName = "Doji.MyPackage",
    NamespaceName = "Doji.MyPackage",
    Description = "Example generated package.",
    CompanyName = "Doji",
    CreateDocsFolder = true,
    CreateSamplesFolder = true,
    CreateEditorFolder = true
};

RepoSettings repo = new RepoSettings {
    CopyrightHolder = "Doji",
    LicenseType = LicenseType.MIT,
    InitializeGitRepository = true,
    IncludeReadme = true,
    RepositoryUrl = "https://github.com/doji/{{PACKAGE_NAME}}"
};

string repositoryPath = PackageAuthoringApi.GeneratePackage(project, package, repo);
```

This creates a repository at:

`<Target Location>/<Package Identifier>`

The companion project is generated at:

`<Target Location>/<Package Identifier>/projects/<Project Name>`

## Open The Generated Project Automatically

Both generation methods accept an optional `openProjectAfterCreation` argument.

```csharp
string projectPath = PackageAuthoringApi.GenerateProject(project, openProjectAfterCreation: true);
```

For package generation, this opens the generated companion project.

## Save Or Reset Project-Scoped Settings

`PackageAuthoringApi` also exposes the project-settings helpers used by the settings UI:

- `PackageAuthoringApi.SaveAllProjectSettings()` writes the editable package-authoring settings assets back into `ProjectSettings`.
- `PackageAuthoringApi.ReapplyAllTemplateDefaults()` restores the built-in template content and then saves those settings assets.

## Template Resolution Behavior

The scripting API uses the same template settings and generation behavior as the editor windows.

That means:

- generated `README.md`, `AGENTS.md`, `LICENSE`, and DocFX files use the current project-scoped template settings
- tokenized values such as `{{PACKAGE_NAME}}`, `{{PROJECT_NAME}}`, and `{{NAMESPACE_NAME}}` are resolved from the settings objects you pass in
- package generation uses the same optional folder toggles and companion-project manifest behavior as the package creation wizard
