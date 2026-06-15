using System.ComponentModel.DataAnnotations;

namespace todostodo.api.Models;

public record UpdateSettingsRequest(
    [Required] string Font
);
