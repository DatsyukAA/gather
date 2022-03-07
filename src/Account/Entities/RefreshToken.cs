using System.Text.Json.Serialization;
using Account.Data;

namespace Account.Entities
{
    public class RefreshToken : Entity
    {
        
        [JsonIgnore]
        new public int Id { get; set; }

        public string Token { get; set; } = string.Empty;
        public DateTime Expires { get; set; }
        public bool IsExpired => DateTime.UtcNow >= Expires;
        public DateTime Created { get; set; }
        public string CreatedByIp { get; set; } = string.Empty;
        public DateTime? Revoked { get; set; }
        public string? RevokedByIp { get; set; } = null;
        public string? ReplacedByToken { get; set; } = null;
        public bool IsActive => Revoked == null && !IsExpired;
    }
}