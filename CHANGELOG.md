# Changelog

All notable changes to this project are documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

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

[Unreleased]: https://github.com/AtticusG3/docker-script-reloader/compare/v1.0.2...HEAD
[1.0.2]: https://github.com/AtticusG3/docker-script-reloader/releases/tag/v1.0.2
[1.0.1]: https://github.com/AtticusG3/docker-script-reloader/releases/tag/v1.0.1
[1.0.0]: https://github.com/AtticusG3/docker-script-reloader/releases/tag/v1.0.0
