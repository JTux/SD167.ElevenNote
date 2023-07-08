using System.ComponentModel.DataAnnotations;

namespace ElevenNote.Models.User;

public class UserLogin
{
    [Required]
    public string? Username { get; set; }

    [Required]
    public string? Password { get; set; }
}