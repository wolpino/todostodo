using System.ComponentModel.DataAnnotations;

namespace todostodo.api.Models;

public record CreateEntryRequest(
    [Required][MaxLength(200)] string Title,
    [MaxLength(1000)] string? Description,
    EntryStatus Status = EntryStatus.Active
);

public record UpdateEntryRequest(
    [Required] int Id,
    [MaxLength(200)] string? Title,
    EntryStatus? Status
);
