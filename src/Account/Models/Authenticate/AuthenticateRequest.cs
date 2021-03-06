using System.ComponentModel.DataAnnotations;

namespace Account.Models.Authenticate
{
    public class AuthenticateRequest
    {
        [Required]
        public string Login { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
    }
}