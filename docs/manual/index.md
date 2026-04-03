# Manual

`com.doji.package-authoring` is an editor-only Unity package, so the DocFX site for this repository is built from the C# sources under `com.doji.package-authoring`.

## Build On macOS

Install DocFX as a global .NET tool if it is not already available:

```bash
dotnet tool install -g docfx
```

Use the repo wrapper on macOS. It points DocFX at Homebrew's real `.NET` SDK location under `/opt/homebrew/opt/dotnet/libexec`, which avoids the SDK lookup failure from the default shim:

```bash
./scripts/docfx.sh docs/docfx.json
```

Build the HTML site from the repository root:

```bash
./scripts/docfx.sh docs/docfx.json
```

Preview the generated site locally:

```bash
./scripts/docfx.sh serve docs/_site
```

DocFX prints the local URL after the server starts. Stop it with `Ctrl+C`.

## PDF Output

Build the PDF-oriented output with:

```bash
./scripts/docfx.sh docs/docfx-pdf.json
```

The generated PDF is written under `docs/_site/pdf/com.doji.package-authoring.pdf`.

## Notes

- The API filter includes the `Doji.PackageAuthoring.Editor` namespaces because this package exposes editor tooling rather than runtime components.
- Generated API YAML files stay ignored under `docs/api/`, so you can rebuild locally without committing intermediate artifacts.
