using System.ComponentModel.DataAnnotations;

namespace todostodo.Models;

/// <summary>
/// JWT authentication token data returned to clients after successful login.
/// </summary>
[Serializable]
public class JwtData
{
    /// <summary>
    /// The JWT token string.
    /// </summary>
    [Required]
    public required string Token { get; set; }

    /// <summary>
    /// The expiration date/time of the token.
    /// </summary>
    [Required]
    public DateTime Expiration { get; set; }
}