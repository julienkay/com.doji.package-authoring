---
name: package-authoring-test-creation
description: Add or revise Unity editor tests for this package-authoring project. Use when creating regression coverage for generated package or project output, scripting API behavior, token resolution, manifest content, or editor-test asmdef wiring under `com.doji.package-authoring/Tests/Editor`.
---

# Package Authoring Test Creation

Use this skill for tests that validate package-authoring behavior in `com.doji.package-authoring`.

Primary scope:

- `com.doji.package-authoring/Tests/Editor/`
- `com.doji.package-authoring/Editor/API/PackageAuthoringApi.cs`
- generated output rooted in temp directories

Default testing strategy:

1. Prefer black-box editor tests that call `PackageAuthoringApi.GeneratePackage(...)` or `PackageAuthoringApi.GenerateProject(...)`.
2. Generate into a unique temp directory under `Path.GetTempPath()`.
3. Assert on concrete generated files, not internal helper return values.
4. Clean up the temp directory in `TearDown`.

Current preferred assembly layout:

1. Put tests in `com.doji.package-authoring/Tests/Editor/`.
2. Use an editor-only asmdef named `Doji.PackageAuthoring.Editor.Tests`.
3. Reference `Doji.PackageAuthoring.Editor`.
4. Include `"optionalUnityReferences": [ "TestAssemblies" ]`.
5. Preserve matching `.meta` files for the folder, asmdef, and test scripts.

What to cover first:

- tokenized values that must survive UI input and resolve in generated files
- generated `package.json` content such as `author.url`, `documentationUrl`, dependencies, and Unity version fields
- generated companion project `Packages/manifest.json` content such as local package reference, IDE package selection, and `testables`
- generated templated files like repository `README.md`, `AGENTS.md`, docs landing pages, and DocFX config files

What to avoid by default:

- reaching into `internal` template helpers when the public API can exercise the same path
- brittle assertions on full file snapshots when a few targeted assertions capture the contract
- tests that require opening Unity projects, launching external tools, or initializing git unless the user explicitly wants that coverage

Implementation pattern:

```csharp
private readonly string _tempRoot = Path.Combine(
    Path.GetTempPath(),
    "Doji.PackageAuthoring.Tests",
    Guid.NewGuid().ToString("N"));

[TearDown]
public void TearDown() {
    if (Directory.Exists(_tempRoot)) {
        Directory.Delete(_tempRoot, recursive: true);
    }
}
```

Assertion pattern:

1. Build `ProjectSettings`, `PackageSettings`, and `RepoSettings` with only the fields needed for the scenario.
2. Disable git initialization unless the test is explicitly about git behavior.
3. Parse generated JSON with `JObject.Parse(...)` for manifest assertions.
4. For package ids or dependency keys that contain dots, read from the containing `JObject` by exact key, for example `dependencies["com.doji.package-authoring"]`, instead of `SelectToken(...)`.
5. For template outputs, assert both the resolved expected value and absence of unresolved tokens where that matters.

When adding or updating tests:

1. Check whether an existing API-generation test can absorb the new case before creating a new fixture.
2. Keep helpers local to the fixture when they only serve one behavior cluster.
3. Verify the test still reflects current on-disk package behavior, especially after generator refactors.
4. If the work introduces a new generated surface, add at least one regression assertion at that surface rather than only testing supporting internals.
5. Treat dotted package names as literal JSON property names, not JSONPath segments.

Current reference test:

- `com.doji.package-authoring/Tests/Editor/PackageAuthoringApiGenerationTests.cs`

Finish checklist:

- confirm the test assembly exists and stays editor-only
- confirm generated paths match the repository's package-root and companion-project layout
- confirm temp directories are unique per test fixture and cleaned up
- confirm assertions target user-visible output contracts
- if Unity tests were not run in the current environment, say so explicitly
