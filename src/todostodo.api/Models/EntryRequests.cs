using System.ComponentModel.DataAnnotations;

namespace todostodo.api.Models;

// No UserId on these DTOs — ownership comes from the authenticated user's claim only.
public record CreateEntryRequest(
    [Required][MaxLength(200)] string Title,
    EntryStatus Status = EntryStatus.Active,
    EntryKind Kind = EntryKind.Todo,
    DateOnly? AssignedDate = null,
    TimeOnly? AssignedTime = null
);

// Partial update: only non-null fields are applied in the controller.
public record UpdateEntryRequest(
    [Required] int Id,
    [MaxLength(200)] string? Title,
    EntryStatus? Status,
    EntryKind? Kind,
    DateOnly? AssignedDate = null,
    TimeOnly? AssignedTime = null
);
