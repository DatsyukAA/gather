using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Account.Data;

namespace Account.Entities
{
    public class User : Entity
    {
        [Required]
        public string Username { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        [JsonIgnore]
        [Required]
        public string Password { get; set; } = string.Empty;

        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; } = new();
    }
}