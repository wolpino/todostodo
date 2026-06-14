namespace todostodo.api.Models;

public class Settings
{
    public int Id { get; set; }
    public required string Font { get; set; }
    public string UserId { get; set; } = string.Empty;
    public User? User { get; set; }
}
