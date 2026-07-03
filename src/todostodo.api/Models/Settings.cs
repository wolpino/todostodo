namespace todostodo.api.Models;

/// <summary>Per-user UI preferences. One row per user (unique index on <see cref="UserId"/>).</summary>
public class Settings
{
    public int Id { get; set; }
    public required string Font { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
}
