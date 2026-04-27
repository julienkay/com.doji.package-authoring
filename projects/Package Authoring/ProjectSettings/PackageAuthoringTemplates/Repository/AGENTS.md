# AGENTS.md
test123
## Project Intent

This repository contains the Unity package `{{PACKAGE_NAME}}` and its companion project `{{PROJECT_NAME}}`.

{{PACKAGE_DESCRIPTION}}

Unity 6000.3 LTS. There is no compile step, CLI build command, or test runner to invoke. Do not attempt to build, compile, or run the project.

## Package Direction

- <replace this with custom, package-related instructions>

## Source Of Truth

Prefer these locations:

- repository root: orient from the repository root for searches, docs, and cross-package work; from this companion Unity project working directory, the repository root is `../..`
- `{{PACKAGE_NAME}}/`: local package under development
- `docs/`: user-facing package documentation only; keep maintainer notes and internal architecture docs out of DocFX content
- `.codex/current-work.md`: first-read session resume doc with active tasks, blockers, and next steps
- `.codex/project-brief.md`: stable project intent, constraints, and repo context
- `.codex/architecture/`: durable maintainer reference docs; read only the files relevant to the current task
- `.codex/skills/`: project-specific skills; when a task matches one, read and follow the relevant `SKILL.md`
- `.codex/project-memory.md`: durable project memory and agent-oriented project context
- `projects/{{PROJECT_NAME}}/Assets`: companion-project assets and test scenes

Prefer working in the package folder when functionality is package-owned.

## Internal Docs

Read `.codex/current-work.md` first for most sessions as handoff doc.
Treat `.codex/index.md` as the main entry point for internal maintainer docs when you need the broader doc map beyond that.
Keep durable project memory and maintainer-only planning and backlog notes under `.codex/`.
Use the `current-work-maintenance` skill whenever updating `.codex/current-work.md` or when a task changes the active project focus, blockers, or next steps enough that the next session needs an updated handoff.

## Conventions

When adding code:

- organize scripts by task area; prefer colocated or clearly scoped folders such as docs-, tooling-, or workflow-specific script locations over a shared miscellaneous scripts bucket

## C# Style (Unity)

- No raw public fields.
- Prefer properties over public fields.
- Use `[field: SerializeField] public TYPE Name { get; private set; }` when inspector/serializer input should be externally read-only.
- Use `[field: SerializeField] public TYPE Name { get; set; }` for serialized data objects/DTOs that are intentionally edited through tooling code.
- Use `[SerializeField] private TYPE name;` for private Inspector fields.
- Private instance fields use `_camelCase`.
- Private computed properties may use `PascalCase` when they read like derived values rather than stored state.
- Private static readonly shared collections/values may use `PascalCase` when treated as internal constants/shared state.
- Place multiple attributes on separate lines.
- Prefer property-based public APIs; keep backing fields private or compiler-generated.
- Follow Unity conventions for `MonoBehaviour` and `EditorWindow` methods; do not add unnecessary `Update()` or `FixedUpdate()`.

## Documentation Style

- Document every public type and any non-obvious internal type with an XML summary.
- Non-obvious private methods may also get brief comments when that improves readability or preserves important UI, rendering, path, or state-management intent.
- Start summaries high-level: explain role and intent before mechanics.
- Do not narrate obvious code. Avoid filler like `Gets or sets`, `Helper for`, or comments that restate the method name.
- Add detail only where behavior is surprising, constrained by Unity/serialization, or shaped by a workaround.
- Prefer short summaries plus targeted `<param>` notes over long blocks.
- Document why a piece exists or what contract it preserves when that is not obvious from the implementation.

## Avoid

- Do not modify `Library/`, `Temp/`, `Obj/`, `Logs/`, `.idea` or `UserSettings` unless the task is explicitly about them.

## Validation

- Unity-specific moves must preserve matching `.meta` files
- each completed implementation phase must add or update tests that cover phase output before treating phase as done
- when a change affects package users or package-author workflow, add or update user-facing docs under `docs/` in same phase
- after larger refactors, verify the current file contents before describing the final state; do not rely on intent, partial diffs, or memory
- when multiple files should have changed, confirm each affected file explicitly
