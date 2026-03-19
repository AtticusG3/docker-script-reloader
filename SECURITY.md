# Security policy

## Supported versions

Only the latest minor release on the default branch is actively maintained unless noted otherwise in release notes.

## Reporting a vulnerability

Please **do not** open a public GitHub issue for undisclosed security problems.

Instead, contact the maintainer privately (for example via GitHub Security Advisories for this repository, if enabled, or email if published in the repo or maintainer profile).

Include:

- Description of the issue and impact
- Steps to reproduce (if safe to share)
- Affected version or commit if known

We will aim to acknowledge receipt and coordinate a fix and disclosure timeline.

## Scope notes

- This application can restart Docker containers on remote hosts when given valid SSH credentials. Treat workstations running it and saved session files as sensitive.
- Prefer SSH keys and least-privilege Linux accounts in production environments; password auth is a convenience tradeoff.
