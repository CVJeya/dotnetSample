### Repository Facts

Root: e:\Documents\Learnings\docs

## Top-Level Structure

- architecture/
- mentorship/

### LLM Summary

# Repository Map: `e:\Documents\Learnings\docs`

## Observed structure
```text
docs/
├── architecture/
└── mentorship/
```

Only these two top-level directories are confirmed. The provided facts list no files, `README`, `AGENTS.md`, nested `docs/`, package manifests, source modules, or CI configs.

## Documentation location
- The repository root is named `docs`; documentation is expected at this root.
- Confirmed topical areas are `architecture/` and `mentorship/`.
- No root `README`, `AGENTS.md`, or nested `docs/` directory is evidenced. Verify before relying on one.

## Read-first entry points
No specific entry files are provided. To navigate without guessing:
1. List the root and read `README.md` or `AGENTS.md` if present.
2. Look for an index/README inside `architecture/`.
3. Look for an index/README inside `mentorship/`.

Do not assume these files exist until the filesystem confirms them.

## Architecture and modules
Architecture is not evidenced beyond the directory names:
- `architecture/` — responsibility implied by name; contents unknown.
- `mentorship/` — responsibility implied by name; contents unknown.

No Azure DevOps work-item assistant code, AI integration, service boundaries, or module wiring is shown in the provided facts.

## Conventions
No naming, structure, error-handling, testing, or code-style conventions are visible. No code, test, or config files are listed.

## Build, test, lint
Unknown. No package manifests, build scripts, test runners, linters, or CI files appear in the provided top-level structure. If this is a docs-only tree, there may be nothing to build; confirm after listing files.

## Gotchas
- The prompt describes an AI coding assistant for Azure DevOps work items, but the repository facts only show `architecture/` and `mentorship/`. Treat that scope as unverified until files are read.
- Absence from the provided top-level facts does not prove absence deeper in the tree.
- Start with filesystem discovery rather than assuming standard entry files.

## Fast navigation
```powershell
Set-Location e:\Documents\Learnings\docs
Get-ChildItem -Force
Get-ChildItem architecture,mentorship -Recurse -Force
```