namespace ScriptReloader.Services;

public sealed record DockerCommandResult(int ExitCode, string StdOut, string StdErr);
