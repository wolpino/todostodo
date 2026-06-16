using System.Text.Json.Serialization;

namespace todostodo.api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntryKind
{
    Todo,
    Note,
    Event,
}

