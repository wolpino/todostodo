namespace todostodo.api.Models;

public class Entry
{
    public int Id { get; set; }
    public EntryKind Kind { get; set; } = EntryKind.Todo;
    public required string Title { get; set; }
    public EntryStatus Status { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateOnly? AssignedDate { get; set; }
    public TimeOnly? AssignedTime { get; set; }
}