# DocFX Notes

This repository uses `scripts/docfx.sh` as a small wrapper around `docfx`.

On macOS, the wrapper points DocFX at Homebrew's real `.NET` SDK location under `/opt/homebrew/opt/dotnet/libexec`, which avoids SDK lookup failures from the default shim.

## Install

Install DocFX as a global .NET tool if it is not already available:

```bash
dotnet tool install -g docfx
```

## Build HTML

From the repository root:

```bash
./scripts/docfx.sh docs/docfx.json
```

## Preview Locally

Serve the generated site from `docs/_site`:

```bash
./scripts/docfx.sh serve docs/_site
```

DocFX prints the local URL after the server starts. Stop it with `Ctrl+C`.

## Build PDF

Build the PDF-oriented output with:

```bash
./scripts/docfx.sh docs/docfx-pdf.json
```

The generated PDF is written to `docs/_site/pdf/com.doji.package-authoring.pdf`.

## Notes

- The API filter includes the `Doji.PackageAuthoring.Editor` namespaces because this package exposes editor tooling rather than runtime components.
- Generated API YAML files stay ignored under `docs/api/`, so you can rebuild locally without committing intermediate artifacts.
