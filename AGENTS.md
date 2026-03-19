# AGENTS.md

Instructions for coding agents and automation working on this repository.

## Project

**Script Reloader** is a Windows WPF (.NET 8) desktop app. It connects to a Linux host over SSH (password), runs `docker ps` (JSON lines) and `docker restart`, and shows results in a DataGrid.

Authored by Kevyn Watkins.

## Build and verify

```powershell
dotnet restore ScriptReloader.sln
dotnet build ScriptReloader.sln -c Release
```

There is no test project yet; CI only builds. Prefer adding tests under a `tests/` folder if you introduce testable core logic.

WPF requires **Windows** to build and run. Do not assume Linux agents can compile the WPF project.

## Versioning

- **Semantic versioning (SemVer 2.0)** is defined in [Directory.Build.props](Directory.Build.props) as `Version` / `AssemblyVersion` / `FileVersion`.
- User-visible or distributable changes should update [CHANGELOG.md](CHANGELOG.md) under `[Unreleased]`, then move entries under a version section when releasing.
- Git tags for releases: `vMAJOR.MINOR.PATCH` (example: `v1.0.0`). See [RELEASING.md](RELEASING.md).

## Commits and pull requests

- Use [Conventional Commits](https://www.conventionalcommits.org/) style: `type(scope): summary` (ASCII only in titles).
- Common types: `feat`, `fix`, `docs`, `chore`, `ci`, `refactor`, `perf`, `test`.
- Keep PRs focused; avoid unrelated refactors mixed with feature work.

## Security and secrets

- Never commit passwords, keys, tokens, or `.env` files with real values.
- `appsettings.json` in repo is a template only; operators use user secrets, environment variables, or the in-app remembered session file on disk.
- If you find committed secrets, notify the maintainer and rotate credentials; do not paste secrets into issues or PRs.

## Code style

- Match existing patterns in the solution (nullable reference types enabled, implicit usings).
- Prefer clear error messages for operators (SSH/Docker failures).
- For any new console or log output intended for Windows `cmd.exe`, use ASCII-only status text (no emoji).

## Repository map

| Path | Role |
|------|------|
| `ScriptReloader/` | WPF application |
| `ScriptReloader/Services/` | SSH + Docker remote execution, session store |
| `ScriptReloader/Models/` | Options, DTOs, JSON converters |
| `.github/workflows/` | CI |
| `.cursor/rules/` | Cursor agent rules (semver, commits, security) |

## Maintainer docs

- [CONTRIBUTING.md](CONTRIBUTING.md) for contributors.
- [SECURITY.md](SECURITY.md) for vulnerability reporting.
- [RELEASING.md](RELEASING.md) for version bumps and tags.
