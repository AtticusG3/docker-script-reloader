namespace ScriptReloader.Models;

/// <summary>On-disk JSON for remembered SSH fields (password is DPAPI-protected, not plain text).</summary>
public sealed class SavedConnectionDto
{
    public string Host { get; set; } = "";

    public int Port { get; set; } = 22;

    public string Username { get; set; } = "";

    /// <summary>Base64 of DPAPI(UTF-8 password), CurrentUser scope; null/empty if not stored.</summary>
    public string? PasswordProtected { get; set; }
}
