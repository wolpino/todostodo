namespace todostodo.Models;

/// <summary>
/// TODO do I need this enum? it was a bool
/// Enum defining the possible status values for entries (todos, notes, etc.).
/// </summary>
public enum EntryStatus
{
    /// <summary>
    /// The entry is active and relevant.
    /// </summary>
    Active,

    /// <summary>
    /// The entry is inactive or archived.
    /// </summary>
    Inactive
}
