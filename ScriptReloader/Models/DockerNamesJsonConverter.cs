using System.Text.Json;
using System.Text.Json.Serialization;

namespace ScriptReloader.Models;

/// <summary>
/// Docker <c>ps --format '{{json .}}'</c> emits <c>Names</c> as a string; older/API-style output may use a JSON array.
/// </summary>
public sealed class DockerNamesJsonConverter : JsonConverter<List<string>?>
{
    public override List<string>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.Null:
                return null;
            case JsonTokenType.String:
            {
                var s = reader.GetString();
                return string.IsNullOrEmpty(s) ? [] : [s];
            }
            case JsonTokenType.StartArray:
            {
                var list = new List<string>();
                while (reader.Read() && reader.TokenType != JsonTokenType.EndArray)
                {
                    if (reader.TokenType == JsonTokenType.String)
                    {
                        var item = reader.GetString();
                        if (!string.IsNullOrEmpty(item))
                        {
                            list.Add(item);
                        }
                    }
                    else
                    {
                        reader.Skip();
                    }
                }

                return list;
            }
            default:
                return [];
        }
    }

    public override void Write(Utf8JsonWriter writer, List<string>? value, JsonSerializerOptions options)
    {
        if (value is null)
        {
            writer.WriteNullValue();
            return;
        }

        writer.WriteStartArray();
        foreach (var n in value)
        {
            writer.WriteStringValue(n);
        }

        writer.WriteEndArray();
    }
}
