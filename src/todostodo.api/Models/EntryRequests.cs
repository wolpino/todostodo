using System.ComponentModel.DataAnnotations;

namespace todostodo.api.Models;

public record CreateEntryRequest(
    [Required][MaxLength(200)] string Title,
    EntryStatus Status = EntryStatus.Active,
    EntryKind Kind = EntryKind.Todo,
    DateOnly? AssignedDate = null,
    TimeOnly? AssignedTime = null
);

public record UpdateEntryRequest(
    [Required] int Id,
    [MaxLength(200)] string? Title,
    EntryStatus? Status,
    EntryKind? Kind,
    DateOnly? AssignedDate = null,
    TimeOnly? AssignedTime = null
);
