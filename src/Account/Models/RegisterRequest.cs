using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Account.Models
{
    public class RegisterRequest
    {
        [Required]
        [JsonProperty("username")]
        public string Login { get; set; } = string.Empty;
        [Required]
        public string Password { get; set; } = string.Empty;
        [Required]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
    }
}
