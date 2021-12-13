using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Account.Data;

namespace Account.Entities
{
    public class User : Entity
    {
        [Required]
        public string Username { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Avatar { get; set; }

        [JsonIgnore]
        [Required]
        public string Password { get; set; }

        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }

        public DateTime CreationDate { get; } = DateTime.UtcNow;
    }
}