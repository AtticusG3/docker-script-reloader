# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- Windows releases are built as a **single self-contained `ScriptReloader.exe`** (publish profile `portable-win-x64`): compressed single-file, no PDB, default config embedded; optional `appsettings.json` beside the exe still overrides.

## [1.0.1] - 2026-03-20

### Changed

- CI: `actions/checkout` v6, `actions/setup-dotnet` v5 (Dependabot-aligned).

## [1.0.0] - 2026-03-20

### Added

- Initial public-ready release baseline: WPF app, SSH password auth, Docker list/restart, remembered session (DPAPI), configuration via appsettings and user secrets.

[Unreleased]: https://github.com/AtticusG3/docker-script-reloader/compare/v1.0.1...HEAD
[1.0.1]: https://github.com/AtticusG3/docker-script-reloader/releases/tag/v1.0.1
[1.0.0]: https://github.com/AtticusG3/docker-script-reloader/releases/tag/v1.0.0
