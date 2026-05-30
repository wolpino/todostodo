namespace todostodo.Models;

/// <summary>
/// Enum defining the possible status values for entries (tasks, notes, etc.).
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
