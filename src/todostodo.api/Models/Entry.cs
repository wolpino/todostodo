namespace todostodo.api.Models;

public class Entry
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public EntryStatus Status { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}