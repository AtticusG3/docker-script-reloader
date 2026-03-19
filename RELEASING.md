# Releasing

This project uses **Semantic Versioning 2.0** ([semver.org](https://semver.org/)).

## Before a release

1. Ensure `main` (or the release branch) builds cleanly:

   ```powershell
   dotnet build ScriptReloader.sln -c Release
   ```

2. Update version constants in [Directory.Build.props](Directory.Build.props):
   - `Version` (SemVer: `MAJOR.MINOR.PATCH`; optional pre-release labels per SemVer, e.g. `1.1.0-beta.1`)
   - `AssemblyVersion` and `FileVersion` (typically aligned as `MAJOR.MINOR.PATCH.0` for apps)

3. Update [CHANGELOG.md](CHANGELOG.md):
   - Move items from `[Unreleased]` into a new section `## [X.Y.Z] - YYYY-MM-DD`
   - Update the compare links at the bottom (replace `OWNER/REPO` with the real GitHub `owner/repo`)

4. Commit with a conventional commit, for example:

   ```text
   chore(release): v1.0.1
   ```

5. Tag the release:

   ```powershell
   git tag -a v1.0.1 -m "v1.0.1"
   git push origin v1.0.1
   ```

6. On GitHub, create a **Release** from that tag and paste the changelog section for that version into the release notes.

## SemVer quick reference

- **MAJOR**: breaking changes operators must react to (UX, config file format, security defaults, etc.)
- **MINOR**: backward-compatible features
- **PATCH**: backward-compatible fixes

Pre-release versions (optional): `1.0.0-alpha.1`, `1.0.0-rc.1` per SemVer 2.0.
