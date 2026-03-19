using System.Text.Json.Serialization;

namespace ScriptReloader.Models;

/// <summary>Maps <c>docker ps -a --format '{{json .}}'</c> lines.</summary>
public sealed class ContainerInfo
{
    public string? Id { get; set; }

    [JsonConverter(typeof(DockerNamesJsonConverter))]
    public List<string>? Names { get; set; }

    public string? Image { get; set; }

    public string? State { get; set; }

    public string? Status { get; set; }

    public string DisplayName
    {
        get
        {
            if (Names is { Count: > 0 })
            {
                return Names[0].TrimStart('/');
            }

            return string.IsNullOrEmpty(Id) ? "" : Id;
        }
    }

    /// <summary>Prefer container ID for <c>docker restart</c> to avoid shell metacharacters in names.</summary>
    public string RestartTarget => string.IsNullOrWhiteSpace(Id) ? DisplayName : Id;
}
