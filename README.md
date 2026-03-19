# Script Reloader

Authored by **Kevyn Watkins**.

Windows 11 WPF app that connects to a Linux host over **SSH** (username and password), lists **Docker** containers, and restarts a selected container by running `docker` on the remote machine.

**Version:** semantic versioning is defined in `Directory.Build.props` (currently aligned with [CHANGELOG.md](CHANGELOG.md)).

## Repository layout (GitHub)

| Doc | Purpose |
|-----|---------|
| [AGENTS.md](AGENTS.md) | Guidance for coding agents and automation |
| [CONTRIBUTING.md](CONTRIBUTING.md) | How to contribute, commits, PR checklist |
| [SECURITY.md](SECURITY.md) | How to report security issues |
| [RELEASING.md](RELEASING.md) | Version bumps, tags, releases |
| [CHANGELOG.md](CHANGELOG.md) | Human-readable release history |

CI: GitHub Actions workflow `.github/workflows/ci.yml` builds the solution on `windows-latest` with .NET 8.

## Requirements

- Windows 11 (or Windows with .NET 8 desktop runtime)
- [.NET 8 SDK](https://dotnet.microsoft.com/download) to build
- Linux host: OpenSSH server, Docker installed, and the SSH user can run `docker` without interactive sudo (see below)

## Build and run

```bash
dotnet build ScriptReloader.sln
dotnet run --project ScriptReloader/ScriptReloader.csproj
```

Or open `ScriptReloader.sln` in Visual Studio and run the `ScriptReloader` project.

## Configuration

### appsettings.json

Copy is next to the built executable (`ScriptReloader/appsettings.json` in the repo is copied to output). Set non-secret defaults:

| Key | Meaning |
|-----|---------|
| `Ssh:Host` | Linux hostname or IP |
| `Ssh:Port` | SSH port (default 22) |
| `Ssh:Username` | SSH user |
| `Ssh:CommandTimeoutSeconds` | Timeout for SSH connection and each `docker` command (default 120) |

**Do not** put passwords in `appsettings.json` or commit secrets.

### Remember connection (UI)

Check **Remember host, port, user, and password** to persist fields between runs for the current Windows user:

- File: `%LocalAppData%\ScriptReloader\session.json`
- Password is **not** stored as plain text; it is protected with **Windows DPAPI** (`CurrentUser`), so it is tied to this profile on this PC.
- Unchecking the box **deletes** the saved file.
- On startup, saved values override `appsettings` / user secrets for those fields when the file loads successfully.

### User Secrets (development)

From the project directory:

```bash
cd ScriptReloader
dotnet user-secrets set "Ssh:Password" "your-ssh-password"
```

Optional: set defaults for host and user:

```bash
dotnet user-secrets set "Ssh:Host" "192.168.1.50"
dotnet user-secrets set "Ssh:Username" "deploy"
```

### Environment variables

Configuration also loads environment variables. Use double underscores for nested keys, for example:

- `Ssh__Host`
- `Ssh__Port`
- `Ssh__Username`
- `Ssh__Password`

### UI

You can type the password in the app each session. If the password box is empty, the app falls back to `Ssh:Password` from user secrets or environment.

## Linux host setup

1. Install and start Docker on the Linux machine.
2. Add the SSH user to the `docker` group so `docker ps` works without `sudo`:

   ```bash
   sudo usermod -aG docker YOUR_USER
   ```

   Then log out and back in (or start a new SSH session).

3. Ensure the Windows PC can reach the Linux host on the SSH port (firewall / security groups).

## Behavior

- **Connect** opens an SSH session and loads containers with `docker ps -a --format '{{json .}}'`.
- **Refresh** runs the same list command again.
- **Restart selected** runs `docker restart --time 10 '<target>'` using the container ID from the list when possible.

If `docker` returns a non-zero exit code, the status bar and message boxes show stderr when available.

## Security notes

- Prefer SSH keys and hardened SSH policy for production; this version uses password auth as requested.
- Anyone with the app and credentials can restart containers on the target host.

## License

Licensed under the [MIT License](LICENSE).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). Use conventional commits and update `CHANGELOG.md` for user-visible changes.
