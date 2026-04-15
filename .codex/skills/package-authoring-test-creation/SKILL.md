---
name: package-authoring-test-creation
description: Add or revise Unity editor tests in the existing package-authoring test suite. Use when creating regression coverage for generated package or project output, scripting API behavior, token resolution, manifest content, or test suite architecture under `com.doji.package-authoring/Tests/Editor`.
---

# Package Authoring Test Creation

Use this skill for tests that extend the existing package-authoring editor test suite in `com.doji.package-authoring`.

Primary scope:

- `com.doji.package-authoring/Tests/Editor/`
- `com.doji.package-authoring/Editor/API/PackageAuthoringApi.cs`
- generated output rooted in temp directories

Choose the destination fixture before writing the test:

- Add manifest assertions to `PackageAuthoringApiManifestGenerationTests`.
- Add templated text-file assertions to `PackageAuthoringApiTemplateResolutionTests`.
- Add companion project manifest assertions to `PackageAuthoringApiProjectGenerationTests`.
- Only create a new fixture when the new assertions introduce a distinct output contract that would make the existing fixtures mixed or noisy.

Default testing strategy:

1. Prefer black-box editor tests that call `PackageAuthoringApi.GeneratePackage(...)` or `PackageAuthoringApi.GenerateProject(...)`.
2. Generate into a unique temp directory under `Path.GetTempPath()`.
3. Assert on concrete generated files, not internal helper return values.
4. Clean up the temp directory in `TearDown`.

What to cover first:

- tokenized values that must survive UI input and resolve in generated files
- generated `package.json` content such as `author.url`, `documentationUrl`, dependencies, and Unity version fields
- generated companion project `Packages/manifest.json` content such as local package reference, IDE package selection, and `testables`
- generated templated files like repository `README.md`, `AGENTS.md`, docs landing pages, and DocFX config files

What to avoid by default:

- reaching into `internal` template helpers when the public API can exercise the same path
- brittle assertions on full file snapshots when a few targeted assertions capture the contract
- tests that require opening Unity projects, launching external tools, or initializing git unless the user explicitly wants that coverage
- creating a new fixture when one of the existing output-contract fixtures is already the right home

Implementation pattern:

```csharp
internal sealed class PackageAuthoringApiManifestGenerationTests : PackageAuthoringApiTestBase {
    [Test]
    public void GeneratePackage_ResolvesTokenizedManifestUrls() {
        ProjectSettings projectSettings = CreateProjectSettings("Tokenized Companion");
        PackageSettings packageSettings = CreatePackageSettings(
            authorUrl: "https://docs.doji-tech.com/{{PACKAGE_NAME}}",
            documentationUrl: "https://docs.doji-tech.com/{{PACKAGE_NAME}}/manual");

        string rootDirectory = PackageAuthoringApi.GeneratePackage(
            projectSettings,
            packageSettings,
            CreateRepoSettings(),
            openProjectAfterCreation: false);

        string packageManifestPath = Path.Combine(
            rootDirectory,
            "com.doji.tests.tokenized",
            "package.json");

        // Assert on generated files here.
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

1. Start by placing the test in the existing fixture whose output contract already matches the new assertion.
2. Reuse `PackageAuthoringApiTestBase` for repeated temp-directory lifecycle and canonical settings builders unless there is a concrete reason not to.
3. Keep `PackageAuthoringApiTestBase` focused on stable cross-fixture concerns only; keep output-specific assertions and scenario-specific helpers in the concrete fixtures.
4. Create a new fixture only when the new test target is a separate output contract from the existing suites.
5. Verify the test still reflects current on-disk package behavior, especially after generator refactors.
6. If the work introduces a new generated surface, add at least one regression assertion at that surface rather than only testing supporting internals.
7. Treat dotted package names as literal JSON property names, not JSONPath segments.

Current reference test:

- `com.doji.package-authoring/Tests/Editor/PackageAuthoringApiManifestGenerationTests.cs`
- `com.doji.package-authoring/Tests/Editor/PackageAuthoringApiTemplateResolutionTests.cs`
- `com.doji.package-authoring/Tests/Editor/PackageAuthoringApiProjectGenerationTests.cs`

Finish checklist:

- confirm assertions target user-visible output contracts
