# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

## [1.1.0] - 2026-03-20

### Added

- **Restart all running**: confirms, then runs `docker restart $(docker ps -q)` on the remote host (via `sh` so there is no error when no containers are running).
- Multi-select in the container grid (Ctrl/click, Shift/click). **Restart selected** runs a single remote `docker restart --time 10` with all selected container IDs/names as separate quoted arguments (one command, not one SSH exec per row).
- **Stop selected** and **Start selected** run one remote `docker stop --time 10` or `docker start` with all selected IDs/names as separate quoted arguments (same batching model as restart).

### Changed

- When a remembered SSH session exists on disk, the app connects automatically on launch (if host, user, and password are available).

## [1.0.2] - 2026-03-20

### Added

- Single self-contained **win-x64** portable publish (`portable-win-x64` profile): one `ScriptReloader.exe`, compression, no PDB; default config embedded; optional side-by-side `appsettings.json` overrides.

### Fixed

- Startup: embedded `appsettings.json` is buffered to a `MemoryStream` before `ConfigurationBuilder.Build()` so the stream is not disposed too early (single-file and published builds start correctly).

## [1.0.1] - 2026-03-20

### Changed

- CI: `actions/checkout` v6, `actions/setup-dotnet` v5 (Dependabot-aligned).

## [1.0.0] - 2026-03-20

### Added

- Initial public-ready release baseline: WPF app, SSH password auth, Docker list/restart, remembered session (DPAPI), configuration via appsettings and user secrets.

[Unreleased]: https://github.com/AtticusG3/docker-script-reloader/compare/v1.1.0...HEAD
[1.1.0]: https://github.com/AtticusG3/docker-script-reloader/releases/tag/v1.1.0
[1.0.2]: https://github.com/AtticusG3/docker-script-reloader/releases/tag/v1.0.2
[1.0.1]: https://github.com/AtticusG3/docker-script-reloader/releases/tag/v1.0.1
[1.0.0]: https://github.com/AtticusG3/docker-script-reloader/releases/tag/v1.0.0
