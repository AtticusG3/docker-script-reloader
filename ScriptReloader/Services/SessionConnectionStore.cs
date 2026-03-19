using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ScriptReloader.Models;

namespace ScriptReloader.Services;

public static class SessionConnectionStore
{
    private static readonly JsonSerializerOptions JsonReadOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly JsonSerializerOptions JsonWriteOptions = new()
    {
        WriteIndented = true,
    };

    private static string FilePath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "ScriptReloader",
        "session.json");

    /// <summary>Host, port, username, and decrypted password when present.</summary>
    public static bool TryLoad(out string host, out int port, out string username, out string? password)
    {
        host = "";
        port = 22;
        username = "";
        password = null;

        try
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            var json = File.ReadAllText(FilePath);
            var dto = JsonSerializer.Deserialize<SavedConnectionDto>(json, JsonReadOptions);
            if (dto is null)
            {
                return false;
            }

            host = dto.Host ?? "";
            port = dto.Port is > 0 and <= 65535 ? dto.Port : 22;
            username = dto.Username ?? "";
            if (!string.IsNullOrEmpty(dto.PasswordProtected))
            {
                var cipher = Convert.FromBase64String(dto.PasswordProtected);
                var plain = ProtectedData.Unprotect(cipher, null, DataProtectionScope.CurrentUser);
                password = Encoding.UTF8.GetString(plain);
            }

            return true;
        }
        catch (CryptographicException)
        {
            return false;
        }
        catch (FormatException)
        {
            return false;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    public static void Save(string host, int port, string username, string? passwordPlain)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);

        string? protectedB64 = null;
        if (!string.IsNullOrEmpty(passwordPlain))
        {
            var plainBytes = Encoding.UTF8.GetBytes(passwordPlain);
            var cipher = ProtectedData.Protect(plainBytes, null, DataProtectionScope.CurrentUser);
            protectedB64 = Convert.ToBase64String(cipher);
        }

        var dto = new SavedConnectionDto
        {
            Host = host,
            Port = port,
            Username = username,
            PasswordProtected = protectedB64,
        };

        var json = JsonSerializer.Serialize(dto, JsonWriteOptions);
        File.WriteAllText(FilePath, json);
    }

    public static void Clear()
    {
        try
        {
            if (File.Exists(FilePath))
            {
                File.Delete(FilePath);
            }
        }
        catch (IOException)
        {
            // ignore
        }
    }
}
