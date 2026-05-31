namespace todostodo.Models;

/// <summary>
/// Base class for all types of entries (tasks, notes, events) in the system.
/// </summary>
public class Entry
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }
    
    /// <summary>Gets or sets the entry description.</summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>Gets or sets the creation date in UTC.</summary>
    public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
    
    /// <summary>Gets or sets the last modification date in UTC.</summary>
    public DateTime? ModifiedDate { get; set; }
    
    /// <summary>Gets or sets the entry status (Active or Inactive).</summary>
    public EntryStatus Status { get; set; } = EntryStatus.Active;
    
    /// <summary>Gets or sets the foreign key to the owning user.</summary>
    public int ApplicationUserId { get; set; }

    /// <summary>Gets or sets the navigation property to the owning user.</summary>
    public ApplicationUser? ApplicationUser { get; set; }
}
