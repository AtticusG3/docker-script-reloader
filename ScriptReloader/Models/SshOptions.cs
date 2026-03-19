namespace ScriptReloader.Models;

public sealed class SshOptions
{
    public string Host { get; set; } = "";

    public int Port { get; set; } = 22;

    public string Username { get; set; } = "";

    /// <summary>Optional default password from user secrets or environment (never commit).</summary>
    public string? Password { get; set; }

    public int CommandTimeoutSeconds { get; set; } = 120;
}
