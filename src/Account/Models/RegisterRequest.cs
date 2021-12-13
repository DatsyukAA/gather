using System.ComponentModel.DataAnnotations;

namespace Account.Models
{
    public class RegisterRequest
    {
        [Required]
        public string Login { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string Name { get; set; }
    }
}
