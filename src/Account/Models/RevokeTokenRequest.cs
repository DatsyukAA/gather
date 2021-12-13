using System.ComponentModel.DataAnnotations;

namespace Account.Models
{
    public class RevokeTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}