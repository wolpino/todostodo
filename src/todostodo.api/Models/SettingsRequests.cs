using System.ComponentModel.DataAnnotations;

namespace todostodo.api.Models;

// Font is validated against SettingsController.AllowedFonts — not a free-form string.
public record UpdateSettingsRequest(
    [Required] string Font
);
