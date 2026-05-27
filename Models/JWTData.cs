using System.ComponentModel.DataAnnotations;

namespace todostodo.Models;

[Serializable]
public class JwtData
{
    [Required]
    public string Token { get; set; }
    [Required]
    public DateTime Expiration { get; set; }
}