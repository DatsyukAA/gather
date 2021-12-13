using System.ComponentModel.DataAnnotations;

public class AuthenticateRequest
{
    [Required]
    public string Login { get; set; }
    [Required]
    public string Password { get; set; }
}