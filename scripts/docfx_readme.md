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
./scripts/docfx.sh build docs/docfx.json
```

If API metadata may have changed, regenerate it before the HTML build:

```bash
./scripts/docfx.sh metadata docs/docfx.json
./scripts/docfx.sh build docs/docfx.json
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
./scripts/docfx.sh build docs/docfx-pdf.json
```

The generated PDF is written to `docs/_site/pdf/com.doji.package-authoring.pdf`.

## Notes

- This repository's API docs are generated into `docs/api/`. `build` alone does not refresh that metadata.
- After changing public types, namespaces, or XML comments that affect the generated API reference, run `metadata` before `build`.
- If namespaces or generated API file names changed, stale files can remain in `docs/api/` and `docs/_site/api/`. Clean generated files in those folders while preserving hand-authored files such as `docs/api/index.md` and `docs/api/.gitignore`.
- Generated API YAML files stay ignored under `docs/api/`, so you can rebuild locally without committing intermediate artifacts.
