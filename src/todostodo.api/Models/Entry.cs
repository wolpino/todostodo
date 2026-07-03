namespace todostodo.api.Models;

/// <summary>
/// A single todo/note/event row. Flat table with <see cref="EntryKind"/> rather than
/// inheritance — enough for MVP; subtype-specific columns can be added as nullable fields later.
/// </summary>
public class Entry
{
    public int Id { get; set; }
    public EntryKind Kind { get; set; } = EntryKind.Todo;
    public required string Title { get; set; }
    public EntryStatus Status { get; set; }
    // Set from the identity claim on create — never from the request body.
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ModifiedAt { get; set; }
    // Stamped when status moves to Completed — useful for sorting and future due-date views.
    public DateTime? CompletedAt { get; set; }
    // Reserved for Note/Event kinds; unused in Todo MVP UI.
    public DateOnly? AssignedDate { get; set; }
    public TimeOnly? AssignedTime { get; set; }
}