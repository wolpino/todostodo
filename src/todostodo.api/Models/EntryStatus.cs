using System.Text.Json.Serialization;

namespace todostodo.api.Models;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum EntryStatus
{
    // when an entry is created, it is not started yet
    // Active instead NotStarted to add flexibility for future entry types
    Active,
    // when an entry is being worked on, it is in progress
    InProgress,
    // when an entry is completed
    Completed,
    // when an entry is in the past but hasn't been removed
    Archived,
    // when an entry is "removed" but not deleted from the database
    Inactive
}

