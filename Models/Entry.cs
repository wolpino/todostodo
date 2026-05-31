namespace todostodo.Models;

public class Entry
{
    public int Id { get; set; }
    
    public string Description { get; set; } = string.Empty;
    
    public DateTime? CreatedDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? ModifiedDate { get; set; }
    
    public EntryStatus Status { get; set; } = EntryStatus.Active;
    
    // Foreign Key
    public int ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
}
