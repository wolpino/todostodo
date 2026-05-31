namespace todostodo.Models;

/// <summary>
/// Stores user-specific settings and preferences.
/// </summary>
public class Settings
{
    /// <summary>Gets or sets the unique identifier.</summary>
    public int Id { get; set; }
    
    /// <summary>Gets or sets the color scheme preference.</summary>
    public string ColorScheme { get; set; } = "default";
    
    /// <summary>Gets or sets the font preference.</summary>
    public string Font { get; set; } = "sans-serif";
    
    /// <summary>Gets or sets the foreign key to the owning user.</summary>
    public int ApplicationUserId { get; set; }

    /// <summary>Gets or sets the navigation property to the owning user.</summary>
    public ApplicationUser? ApplicationUser { get; set; }
}
