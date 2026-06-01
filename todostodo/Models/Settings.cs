namespace todostodo.Models;

public class Settings
{
    public int Id { get; set; }
    
    public string ColorScheme { get; set; } = "default";
    
    public string Font { get; set; } = "sans-serif";
    
    // Foreign Key
    public int ApplicationUserId { get; set; }
    public ApplicationUser? ApplicationUser { get; set; }
}
