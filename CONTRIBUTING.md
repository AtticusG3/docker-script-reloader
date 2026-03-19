# Contributing

Thank you for helping improve Script Reloader.

## Ground rules

- Be respectful and constructive.
- Keep changes scoped to a clear goal; avoid drive-by refactors in the same PR as unrelated features.
- Do not commit secrets (passwords, API keys, private keys, real `.env`).

## Development setup

1. Install [.NET 8 SDK](https://dotnet.microsoft.com/download) on **Windows** (WPF).
2. Clone the repository.
3. Build:

   ```powershell
   dotnet build ScriptReloader.sln -c Release
   ```

4. Optional: configure [user secrets](README.md#user-secrets-development) for SSH defaults during development.

## Commits

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
type(optional-scope): short description in imperative mood
```

Examples:

- `fix(ssh): handle empty docker stderr on refresh`
- `docs: clarify DPAPI session storage path`
- `chore(ci): bump actions/checkout`

Use ASCII in the subject line (avoid Unicode in titles for tooling and Windows consoles).

## Pull requests

- Describe **what** changed and **why** (motivation, user impact).
- Link related issues when applicable.
- Ensure `dotnet build ScriptReloader.sln -c Release` passes locally.
- Update [CHANGELOG.md](CHANGELOG.md) under `[Unreleased]` for user-visible changes.

## Versioning and releases

Maintainers follow [RELEASING.md](RELEASING.md). Contributors do not need to bump `Version` for small PRs unless asked; maintainers batch version bumps at release time.

## Security

See [SECURITY.md](SECURITY.md) for reporting vulnerabilities privately.
